import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [react()],

    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },

    server: {
      proxy: {
        "/api": {
          target: env.VITE_GENERAL_BASEURL || "https://localhost:5045",
          changeOrigin: true,
          secure: false,
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
          // MELHORIA AQUI:
          // 1. Usamos o alias @ para garantir o caminho correto
          // 2. Certifique-se que o arquivo em src/styles se chama "_variables.scss" ou "Variables.scss"
          // O ; no final é obrigatório no Sass
          additionalData: `@use "@/styles/_variables" as *;`, 
        },
      },
    },
  };
});