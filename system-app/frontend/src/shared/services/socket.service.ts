import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from "@microsoft/signalr";
import { StorageService, STORAGE_KEYS } from "./storage.service";
import { AppHubsBIFastAPI, type AnyAppHub } from "@/shared/enums/hub.enums";

class WebSocketService {
  // Agora o mapa aceita qualquer um dos dois tipos de Hub
  private connections: Map<string, HubConnection> = new Map();

  /**
   * Identifica qual a Base URL correta para o Hub solicitado
   */
  private getBaseUrlForHub(hubPath: string): string {
    // Verifica se o hubPath pertence aos valores do enum de BI (Python)
    const isPythonHub = Object.values(AppHubsBIFastAPI as Record<string, string>).includes(hubPath);
    
    if (isPythonHub) {
      return import.meta.env.VITE_BI_API_URL || "http://localhost:8000"; // Porta do seu FastAPI
    }
    return import.meta.env.VITE_GENERAL__BASEURL || "https://localhost:5045"; // Porta do seu C#
  }

  public async connect(hubPath: AnyAppHub): Promise<void> {
    if (this.connections.has(hubPath) && this.connections.get(hubPath)?.state === HubConnectionState.Connected) {
      return;
    }

    const baseUrl = this.getBaseUrlForHub(hubPath);
    const fullUrl = `${baseUrl}${hubPath}`;

    const connection = new HubConnectionBuilder()
      .withUrl(fullUrl, {
        accessTokenFactory: () => {
          const token = StorageService.getItem<string>(STORAGE_KEYS.TOKEN);
          return Promise.resolve(token || "");
        },
        skipNegotiation: true,
        transport: 1, // WebSockets apenas
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connections.set(hubPath, connection);

    try {
      await connection.start();
      console.log(`✅ [${baseUrl}] Socket Conectado: ${hubPath}`);
    } catch (err) {
      console.error(`❌ Erro ao conectar em ${hubPath}:`, err);
    }
  }

  public on<T>(hubPath: AnyAppHub, methodName: string, callback: (data: T) => void): void {
    const connection = this.connections.get(hubPath);
    if (!connection) {
      console.warn(`Tentou ouvir evento em ${hubPath} mas não há conexão ativa.`);
      return;
    }
    connection.off(methodName);
    connection.on(methodName, callback);
  }

  public off(hubPath: AnyAppHub, methodName: string): void {
    this.connections.get(hubPath)?.off(methodName);
  }

  public async invoke<T>(hubPath: AnyAppHub, methodName: string, ...args: T[]): Promise<void> {
    const connection = this.connections.get(hubPath);
    if (!connection || connection.state !== HubConnectionState.Connected) {
      await this.connect(hubPath);
    }
    try {
      await this.connections.get(hubPath)?.invoke(methodName, ...args);
    } catch (err) {
      console.error(`❌ Erro ao invocar ${methodName}:`, err);
    }
  }

  public disconnect(hubPath?: AnyAppHub): void {
    if (hubPath) {
      this.connections.get(hubPath)?.stop();
      this.connections.delete(hubPath);
    } else {
      this.connections.forEach((conn) => conn.stop());
      this.connections.clear();
    }
  }
}

export const socketService = new WebSocketService();