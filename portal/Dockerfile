# ========================================================
# Estágio 1: Build (Ambiente Node.js 20 LTS)
# ========================================================
FROM node:20-alpine AS build

WORKDIR /app

# Cache de camadas: Copia manifestos de dependência primeiro
COPY package*.json ./

# Instalação limpa e estrita baseada no lockfile do monorepo
RUN npm ci --silent

# Copia o restante do código-fonte do portal do usuário
COPY . .

# Injeção da URL da API como argumento de build para o Vite
ARG GENERAL_BASEURL
ENV VITE_GENERAL_BASEURL=$GENERAL_BASEURL

# Compila e gera os ativos estáticos otimizados na pasta /app/dist
RUN npm run build

# ========================================================
# Estágio 2: Runtime (Nginx Seguro e Sem Privilégios)
# ========================================================
FROM nginxinc/nginx-unprivileged:alpine

# Copia os artefatos estáticos gerados no estágio de build
COPY --from=build /app/dist /usr/share/nginx/html

# Copia a configuração local do Nginx com as regras de SPA/History API
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Portas abaixo de 1024 são bloqueadas para usuários não-root. Usamos a 8080.
EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]