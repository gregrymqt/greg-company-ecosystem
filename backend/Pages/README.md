# 📁 Pages/

**Objetivo:** Views renderizadas no lado do servidor (Server-Side Rendering) nativas do ASP.NET Core (Razor Pages).

Embora o ecossistema utilize React (Portal e Admin) na porta da frente como *Single Page Applications*, o Backend C# pode expor algumas páginas brutas.

**O que colocar aqui:**
- Páginas de *Error* padrão.
- Páginas de aceite de convite.
- Páginas de status (ex: Sucesso após integração via Webhook).

**Regras:**
- Tente não misturar regras de negócio densas no Code-Behind (`.cshtml.cs`). Use os Services da pasta `Features` para injetar a lógica nessas páginas caso precise acessar o banco.
