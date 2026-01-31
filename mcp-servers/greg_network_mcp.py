from mcp.server.fastmcp import FastMCP
from fastapi import FastAPI
from pydantic import BaseModel, Field
import uvicorn
import threading
from datetime import datetime
import json
from pathlib import Path
from typing import List, Optional

# --- 1. MODELO DE DADOS (SSOT - Single Source of Truth) ---
class LogEntry(BaseModel):
    """Modelo Pydantic para garantir IntelliSense e validação de tipos."""
    source: str = Field(..., example="Front", description="Origem da requisição")
    url: str = Field(..., example="http://localhost:5045/api/courses")
    method: str = Field(..., example="GET")
    status: int = Field(..., example=200)
    timestamp: Optional[str] = None

# --- 2. CONFIGURAÇÕES E ESTADO ---
mcp = FastMCP("Greg-Network-Monitor")
app = FastAPI(title="Greg Company Network API")
network_logs: List[LogEntry] = []

LOG_DIR = Path("../logs")
LOG_FILE = LOG_DIR / "network_logs.json"
LOG_DIR.mkdir(exist_ok=True)

# Carga inicial com parsing automático para a Model
if LOG_FILE.exists():
    try:
        with open(LOG_FILE, 'r', encoding='utf-8') as f:
            data = json.load(f)
            # Converte dicionários brutos de volta para objetos LogEntry
            network_logs = [LogEntry(**item) for item in data[-200:]]
    except Exception:
        network_logs = []

# --- 3. ENDPOINT API ---
@app.post("/log")
async def receive_log(entry: LogEntry):
    """
    Recebe o log do Front (React) ou Back (C#).
    O FastAPI valida automaticamente se o JSON bate com a Model.
    """
    # Adiciona o timestamp no servidor
    entry.timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    network_logs.append(entry)
    
    # Mantém os últimos 200 logs
    if len(network_logs) > 200:
        network_logs.pop(0)
    
    # Persistência assíncrona simples
    try:
        with open(LOG_FILE, 'w', encoding='utf-8') as f:
            # Serializa a lista de objetos usando o método model_dump()
            json.dump([obj.model_dump() for obj in network_logs], f, indent=2, ensure_ascii=False)
    except Exception:
        pass
    
    return {"status": "ok", "received": entry.method}

# --- 4. FERRAMENTA MCP ---
@mcp.tool()
def monitorar_greg_company() -> str:
    """Mostra o tráfego em tempo real do Front (React) e Back (C#)."""
    if not network_logs: 
        return "Silêncio total na rede... Aguardando requisições."
    
    resumo = "🛡️ MONITORAMENTO GREG COMPANY:\n"
    # Agora temos IntelliSense total aqui (log.source, log.status, etc)
    for log in reversed(network_logs):
        icon = "🌐" if log.source == 'Front' else "⚙️"
        status_icon = "✅" if log.status < 400 else "❌"
        resumo += f"{icon} {status_icon} [{log.timestamp}] {log.method} {log.url} (Status: {log.status})\n"
    return resumo

# --- 5. EXECUÇÃO ---
def run_http_server():
    # Rodando na 8888 para não conflitar com o .NET na 5045 
    uvicorn.run(app, host="127.0.0.1", port=8888, log_level="error")

if __name__ == "__main__":
    threading.Thread(target=run_http_server, daemon=True).start()
    mcp.run()