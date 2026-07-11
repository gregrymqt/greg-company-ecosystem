# 📁 Features/

**Objetivo:** Agrupar código por contexto de domínio, seguindo o padrão **Vertical Slice Architecture**.

Diferente do MVC tradicional, aqui tudo que pertence a um domínio (ex: Cursos) fica no mesmo lugar.

**O que você vai encontrar em cada Feature (ex: `Features/Courses/`):**
- **Controllers/:** Os *Endpoints* (APIs) daquela feature.
- **Services/:** Regras de negócio e casos de uso específicos.
- **Repositories/:** Interação direta com o banco de dados via driver MongoDB, focada nas entidades deste domínio.
- **DTOs/:** Modelos de entrada/saída de dados (Data Transfer Objects).
- **Interfaces/:** Contratos de injeção de dependência.
- **Domain/Entities/:** As entidades cruas (ex: `Course.cs`).

**Regras Cruciais:**
- Ao criar um `Service` ou `Repository` novo, ele será injetado automaticamente se a classe estiver em conformidade com o Scrutor (normalmente casando o nome da Classe com a Interface, ex: `CourseService : ICourseService`).
- As features devem ter baixo acoplamento umas com as outras. Se a feature A precisa muito da feature B, considere extrair a lógica mútua para a pasta `Features/Shared/`.
