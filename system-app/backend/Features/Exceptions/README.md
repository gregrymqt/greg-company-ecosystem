# Feature: Exceptions

Define as exceções customizadas utilizadas em toda a aplicação para representar cenários de erro específicos, permitindo um tratamento padronizado e a conversão para códigos de status HTTP apropriados na camada de API.

## Estrutura de Pastas

```
Exceptions/
└── Exceptions.cs    # Definição das classes de exceção customizadas
```

## Tipos de Exceção

### `AppServiceException`
- **Herda de:** `System.Exception`
- **Propósito:** Representa um erro de regra de negócio ou uma falha de validação que impede a conclusão de uma operação. É uma exceção genérica para erros controlados que devem ser informados ao cliente.
- **Exemplo de uso:** `throw new AppServiceException("Já existe um curso com este nome.");`

### `ExternalApiException`
- **Herda de:** `AppServiceException`
- **Propósito:** Representa um erro ocorrido durante a comunicação com uma API externa (ex: Mercado Pago, SendGrid). Como herda de `AppServiceException`, é tratada da mesma forma.
- **Exemplo de uso:** `throw new ExternalApiException("Falha ao processar o pagamento na API externa.", ex);`

### `ResourceNotFoundException`
- **Herda de:** `System.Exception`
- **Propósito:** Indica que um recurso específico solicitado não foi encontrado no sistema (ex: um usuário, curso ou produto com um determinado ID).
- **Exemplo de uso:** `throw new ResourceNotFoundException($"Curso com ID {publicId} não encontrado.");`

## Tratamento de Erros e Mapeamento HTTP

As exceções são capturadas e tratadas centralizadamente pelo método `HandleException` na classe `ApiControllerBase`, que as mapeia para os seguintes códigos de status HTTP:

| Exceção | HTTP Status Code | Descrição |
|-----------------------------|------------------|-------------|
| `AppServiceException` | `400 Bad Request` | Erro de regra de negócio ou entrada inválida. A mensagem da exceção é retornada no corpo da resposta. |
| `ExternalApiException` | `400 Bad Request` | Como herda de `AppServiceException`, também resulta em um Bad Request. |
| `InvalidOperationException` | `400 Bad Request` | Erro de operação inválida, tratado como um erro de negócio. |
| `ResourceNotFoundException` | `404 Not Found` | O recurso solicitado não foi encontrado. |
| `Exception` (genérica) | `500 Internal Server Error` | Um erro inesperado e não tratado ocorreu no servidor. |

## Como Utilizar

As exceções devem ser lançadas a partir da camada de serviço (`Service`) quando uma regra de negócio é violada ou uma condição de erro é encontrada. O `Controller` deve capturar a exceção e repassá-la para o `HandleException`.

**Exemplo (em uma classe de serviço):**
```csharp
public async Task DeleteCourseAsync(Guid publicId)
{
    var course = await _repository.GetByPublicIdAsync(publicId);

    if (course == null)
    {
        // Lança a exceção para recurso não encontrado
        throw new ResourceNotFoundException($"Curso com ID {publicId} não encontrado.");
    }

    if (course.HasStudents)
    {
        // Lança a exceção para regra de negócio
        throw new AppServiceException("Não é possível deletar um curso que possui alunos matriculados.");
    }

    _repository.Delete(course);
    await _unitOfWork.CommitAsync();
}
```