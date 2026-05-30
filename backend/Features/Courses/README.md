# Feature: Courses

Gerencia o catálogo de cursos da plataforma. Expõe dois controllers: um administrativo (restrito a `Admin`) para CRUD completo, e um público para listagem paginada acessível a usuários autenticados.

## Estrutura

```
Courses/
├── Controllers/
│   ├── CoursesAdminController.cs   # CRUD completo — restrito a Role "Admin"
│   └── CoursesPublicController.cs  # Leitura paginada — usuários autenticados
├── DTOs/
│   └── CourseDto.cs                # Contratos de entrada e saída
├── Interfaces/
│   ├── ICourseRepository.cs        # Contrato de acesso a dados
│   └── ICourseService.cs           # Contrato de negócio
├── Mappers/
│   └── CourseMapper.cs             # Mapeamento Course → CourseDto
├── Repositories/
│   └── CourseRepository.cs         # Implementação EF Core
└── Services/
    └── CourseService.cs            # Regras de negócio + cache + paginação
```

## Endpoints

### Admin (`/api/admin/courses`) — `[Authorize(Roles = "Admin")]`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/admin/courses` | Lista cursos paginados (com vídeos) |
| `GET` | `/api/admin/courses/{id:guid}` | Busca curso por PublicId |
| `GET` | `/api/admin/courses/search?name=` | Busca cursos por nome |
| `POST` | `/api/admin/courses` | Cria novo curso |
| `PUT` | `/api/admin/courses/{id:guid}` | Atualiza curso existente |
| `DELETE` | `/api/admin/courses/{id:guid}` | Remove curso (falha se tiver vídeos) |

### Público (`/api/public/courses`) — JWT obrigatório

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/public/courses/paginated` | Lista cursos paginados |

## DTOs

### Saída — `CourseDto`

```json
{
  "publicId": "uuid",
  "name": "string",
  "description": "string",
  "videos": [
    { "id": "uuid", "title": "string" }
  ]
}
```

### Entrada — `CreateUpdateCourseDto`

```json
{
  "name": "string (3–100 chars, obrigatório)",
  "description": "string (máx 500 chars)"
}
```

### Paginação — parâmetros via query string

| Parâmetro | Default Admin | Default Public | Descrição |
|-----------|:---:|:---:|-----------|
| `pageNumber` | 1 | 1 | Página atual |
| `pageSize` | 10 | 5 | Itens por página |

Resposta: `PaginatedResultDto<CourseDto>` com total de itens e metadados de paginação.

## Cache

O `CourseService` usa versionamento de cache via chave `courses_cache_version` no Redis:

- Cache por página: `Courses_v{version}_Page{n}_Size{s}` — TTL de 10 minutos
- Qualquer escrita (create, update, delete) chama `InvalidateCacheByKeyAsync(CoursesCacheVersionKey)`, invalidando todas as páginas cacheadas de uma vez

## Regras de Negócio

| Operação | Regra |
|----------|-------|
| Create | Nome do curso deve ser único (`ExistsByNameAsync`) |
| Delete | Curso com vídeos associados não pode ser deletado → `409 Conflict` |
| FindByPublicId | Lança `ResourceNotFoundException` se não encontrado |
| GetOrCreateByName | Cria o curso se não existir — não persiste sozinho, depende do UnitOfWork do caller |

## Mapper

`CourseMapper` possui dois métodos estáticos:

| Método | Inclui vídeos | Uso |
|--------|:---:|-----|
| `ToDtoWithVideos` | Sim | Listagem paginada |
| `ToDto` | Não | Create/Update (retorno leve) |

## Tratamento de Erros

| Exceção | HTTP |
|---------|------|
| `ResourceNotFoundException` | `404 Not Found` |
| `AppServiceException` (geral) | `400 Bad Request` |
| `AppServiceException` (delete com vídeos) | `409 Conflict` (explícito no Delete) |
| `Exception` (genérica) | `500 Internal Server Error` |
