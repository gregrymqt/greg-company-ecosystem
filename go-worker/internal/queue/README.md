# 📁 internal/queue/

**Objetivo:** Definição de interfaces, nomes de exchanges e filas (Tipagem estrita).

**O que colocar aqui:**
- Tipagem ou constantes das filas (ex: `video.process.request.queue`).
- Nome dos Exchanges (ex: `marketplace.exchange`).
- DTOs ou Structs que representam o payload JSON que trafega na mensageria.

**Regras:**
- Mantenha este diretório como um contrato (Contract/Schema). Ele dita como as mensagens devem chegar e qual o modelo de dados esperado.
