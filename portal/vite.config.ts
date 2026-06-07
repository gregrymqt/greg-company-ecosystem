import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  // Base URL padrão para HTTP/HTTPS
  const backendUrl = env.VITE_GENERAL_BASEURL || "https://localhost:5045";
  
  // Nova variável para o WebSocket/Socket (Se não configurada, usa a URL base como fallback)
  const wsBackendUrl = env.VITE_WS_URL || backendUrl;

  return {
    plugins: [react()],

    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },

    server: {
      port: 5174,
      proxy: {
        // Proxy para as chamadas normais da API
        "/api": {
          target: backendUrl,
          changeOrigin: true,
          secure: false,
        },
        // Proxy para a rota de Uploads de arquivos staticos/mídia
        "/uploads": {
          target: backendUrl,
          changeOrigin: true,
          secure: false,
        },
        // Proxy para o WebSocket/SignalR
        "/ws": {
          target: wsBackendUrl,
          changeOrigin: true,
          secure: false,
          ws: true, // 💡 CRUCIAL: Habilita o suporte a proxying de WebSockets
        },
      },
    },

    build: {
      chunkSizeWarningLimit: 1500,
      rollupOptions: {
        output: {
          manualChunks(id) {
            if (id.includes("node_modules")) {
              if (
                id.includes("react") ||
                id.includes("react-dom") ||
                id.includes("react-router-dom")
              ) {
                return "react-vendor";
              }
              return "vendor";
            }
          },
        },
      },
    },

    css: {
      preprocessorOptions: {
        scss: {
          additionalData: `@use "@/styles/_variables" as *;`, 
        },
      },
    },
  };
});