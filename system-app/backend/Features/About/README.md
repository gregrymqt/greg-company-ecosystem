# Feature: About

Gerencia o conteúdo da página "Sobre" da plataforma, composta por duas entidades independentes: **seções genéricas** (texto + imagem) e **membros da equipe**.

## Estrutura

```
About/
├── Controllers/
│   └── AboutController.cs       # Endpoints REST
├── DTOs/
│   └── AboutDTO.cs              # Contratos de entrada e saída
├── Interfaces/
│   ├── IAboutRepository.cs      # Contrato de acesso a dados
│   └── IAboutService.cs         # Contrato de negócio
├── Repositories/
│   └── AboutRepository.cs       # Implementação EF Core
└── Services/
    └── AboutService.cs          # Regras de negócio + cache + arquivo
```

## Endpoints

Base route: `/api/about`

### Leitura

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/about` | Anônimo | Retorna todo o conteúdo da página (seções + equipe) |

### Seções

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/about/sections` | Cria uma nova seção |
| `PUT` | `/api/about/sections/{id}` | Atualiza uma seção existente |
| `DELETE` | `/api/about/sections/{id}` | Remove uma seção |

### Equipe

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/about/team` | Cria um novo membro |
| `PUT` | `/api/about/team/{id}` | Atualiza um membro existente |
| `DELETE` | `/api/about/team/{id}` | Remove um membro |

> Todos os endpoints de escrita requerem autenticação JWT (`[Authorize]` herdado de `ApiControllerBase`).

## DTOs

### Saída (`GET /api/about`)

```json
{
  "sections": [
    {
      "id": 1,
      "contentType": "section1",
      "title": "string",
      "description": "string",
      "imageUrl": "string",
      "imageAlt": "string"
    }
  ],
  "teamSection": {
    "contentType": "section2",
    "title": "Nosso Time",
    "description": "string",
    "members": [
      {
        "id": 1,
        "name": "string",
        "role": "string",
        "photoUrl": "string",
        "linkedinUrl": "string",
        "githubUrl": "string"
      }
    ]
  }
}
```

### Entrada — Seção (`POST/PUT /api/about/sections`)

Enviado como `multipart/form-data` (herda `BaseUploadDto` para suporte a chunking).

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `title` | `string` | Sim | Título da seção |
| `description` | `string` | Sim | Texto da seção |
| `imageAlt` | `string` | Sim | Texto alternativo da imagem |
| `orderIndex` | `int` | Não | Posição de exibição (default: 0) |
| `file` | `IFormFile` | Não | Imagem da seção |
| `isChunk` | `bool` | Não | Indica upload em partes |
| `chunkIndex` | `int` | Não | Índice do chunk atual |
| `totalChunks` | `int` | Não | Total de chunks |
| `fileName` | `string` | Não | Nome do arquivo (obrigatório se `isChunk: true`) |

### Entrada — Membro (`POST/PUT /api/about/team`)

Enviado como `multipart/form-data`.

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `name` | `string` | Sim | Nome do membro |
| `role` | `string` | Sim | Cargo/função |
| `linkedinUrl` | `string` | Sim | URL do LinkedIn |
| `githubUrl` | `string` | Sim | URL do GitHub |
| `file` | `IFormFile` | Não | Foto do membro |

## Upload de Arquivos

Os endpoints de escrita suportam dois modos via `IFileService`:

- **Upload normal**: arquivo enviado diretamente em um único request.
- **Upload chunked**: arquivo enviado em partes (`isChunk: true`). O service acumula os chunks via `ProcessChunkAsync` e salva apenas no último. Enquanto aguarda chunks intermediários, o endpoint retorna `200 OK` com `{ message: "Chunk N recebido." }`.

O atributo `[AllowLargeFile(2048)]` nos endpoints de upload permite arquivos de até **2 GB**.

## Cache

O resultado do `GET /api/about` é cacheado com a chave `ABOUT_PAGE_CONTENT` via `ICacheService`. O cache é invalidado automaticamente em qualquer operação de escrita (create, update, delete).

## Tratamento de Erros

Os erros são tratados centralizadamente via `HandleException` herdado de `ApiControllerBase`:

| Exceção | HTTP |
|---------|------|
| `ResourceNotFoundException` | `404 Not Found` |
| `AppServiceException` / `InvalidOperationException` | `400 Bad Request` |
| `Exception` (genérica) | `500 Internal Server Error` |
