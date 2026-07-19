# ========================================================
# Estágio 1: Build (Ambiente Node.js LTS)
# ========================================================
FROM node:20-alpine AS build

WORKDIR /app

# Cache de camadas: Copia apenas os manifestos de dependências primeiro
COPY package*.json ./

# Instalação limpa, estrita e determinística baseada no lockfile
RUN npm ci --silent

# Copia o restante do código-fonte do portal
COPY . .

# Injeção das variáveis de ambiente necessárias para o build do Vite
ARG GENERAL_BASEURL
ENV VITE_GENERAL_BASEURL=$GENERAL_BASEURL
ENV VITE_GENERAL__BASEURL=$GENERAL_BASEURL

# Compila e gera os arquivos estáticos otimizados na pasta /app/dist
RUN npm run build

# ========================================================
# Estágio 2: Runtime (Nginx Seguro e Sem Privilégios)
# ========================================================
FROM nginxinc/nginx-unprivileged:alpine

# Copia os artefatos estáticos gerados no estágio de build
COPY --from=build /app/dist /usr/share/nginx/html

# Copia a configuração local do Nginx (essencial para SPAs com React Router)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Portas abaixo de 1024 são restritas ao root. O Nginx Unprivileged usa a 8080.
EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]