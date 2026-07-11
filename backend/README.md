# 📁 backend/

**Objetivo:** A API principal e monolito do ecossistema, desenvolvida em **ASP.NET 8 (C#)**.

Este projeto (MeuCrudCsharp) atua como a espinha dorsal de transações financeiras, controle de usuários, inteligência de negócios (BI) e mensageria distribuída.

**Arquitetura Base:**
- Baseado em **Clean Architecture** e focado 100% no padrão **Vertical Slice Architecture**.
- Ao invés de pastas técnicas (ex: todos os `Controllers/` juntos, todos os `Services/` juntos), o código é agrupado por *Features* (ex: Autenticação, Produtos, Cursos).

**Padrões Cruciais:**
- **Injeção de Dependências (DI):** Usamos a biblioteca *Scrutor* para auto-registrar classes. Se você criar um Serviço dentro de uma *Feature*, ele será detectado automaticamente (se seguir os sufixos e namespaces corretos).
- **Transactional Outbox:** Usado na integração do Banco com a Mensageria. Eventos (como disparar um e-mail após a compra) são salvos no banco de dados na mesma transação da compra, e só então despachados para o RabbitMQ, evitando falhas distribuídas.

**Como Rodar Localmente:**
```bash
dotnet run
```
Acesse `/swagger` no navegador para ver os endpoints documentados.
