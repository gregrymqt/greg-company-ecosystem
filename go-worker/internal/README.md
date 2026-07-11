# 📁 internal/

**Objetivo:** Código privado da aplicação e domínios de negócio.

No ecossistema Go, pacotes dentro de `internal/` não podem ser importados por projetos externos. Isso garante um forte encapsulamento da lógica.

**Subdiretórios:**
- `config/`: Leitura de `.env` e mapeamento de variáveis de ambiente.
- `messaging/`: Consumidores e Publicadores (RabbitMQ).
- `processor/`: Lógicas core de transformação (ex: FFmpeg para vídeos).
- `queue/`: Definições brutas de canais e exchanges.
- `storage/`: Integrações com provedores de arquivos (ex: Supabase S3).
