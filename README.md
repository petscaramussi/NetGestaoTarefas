# Gestão de Tarefas API

API RESTful para gestão de tarefas (criar, listar com filtros, editar e excluir), desenvolvida em **.NET 10 (LTS)** com **Entity Framework Core InMemory**, documentada com **Swagger** e coberta por testes **xUnit**.

## Sumário

- [Tecnologias](#tecnologias)
- [Como executar](#como-executar)
- [Como testar](#como-testar)
- [Endpoints](#endpoints)
- [Arquitetura](#arquitetura)
- [Decisões técnicas](#decisões-técnicas)

## Tecnologias

| Item | Uso |
| --- | --- |
| .NET 10 / ASP.NET Core | API (controllers) |
| Entity Framework Core InMemory | Persistência sem banco externo |
| FluentValidation | Validação de entrada |
| Swashbuckle (Swagger) | Documentação interativa da API |
| xUnit + NSubstitute | Testes unitários |
| Microsoft.AspNetCore.Mvc.Testing | Testes de integração (API end-to-end em memória) |

## Como executar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
git clone <url-do-repositorio>
cd NetGestaoTarefas
dotnet run --project src/GestaoTarefas.Api
```

Acesse o Swagger em **http://localhost:5001/swagger**.

> Os dados ficam em memória: ao reiniciar a aplicação, as tarefas são perdidas.

## Como testar

### Testes automatizados

```bash
dotnet test
```

- **GestaoTarefas.UnitTests**: regras de negócio do `TaskService` (com repositório mockado), validadores e repositório (EF Core InMemory).
- **GestaoTarefas.IntegrationTests**: sobe a API em memória (`WebApplicationFactory`) e valida os endpoints e os códigos HTTP retornados.

Cobertura (opcional):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Testes manuais

Pelo Swagger (`/swagger`) é possível executar todas as operações. Exemplos com `curl`:

```bash
# Criar
curl -X POST http://localhost:5001/api/tasks -H "Content-Type: application/json" \
  -d '{"title":"Estudar EF Core","description":"Provider InMemory","dueDate":"31-12-2026","status":"Pending"}'

# Listar todas / filtrar
curl http://localhost:5001/api/tasks
curl "http://localhost:5001/api/tasks?status=Pending"
curl "http://localhost:5001/api/tasks?dueDate=31-12-2026"
curl "http://localhost:5001/api/tasks?status=InProgress&dueDateFrom=01-12-2026&dueDateTo=31-12-2026"

# Obter, editar e excluir
curl http://localhost:5001/api/tasks/{id}
curl -X PUT http://localhost:5001/api/tasks/{id} -H "Content-Type: application/json" \
  -d '{"title":"Estudar EF Core","status":"Completed","dueDate":"31-12-2026"}'
curl -X DELETE http://localhost:5001/api/tasks/{id}
```

## Endpoints

| Método | Rota | Descrição | Respostas |
| --- | --- | --- | --- |
| `GET` | `/api/tasks` | Lista tarefas (filtros opcionais) | 200, 400 |
| `GET` | `/api/tasks/{id}` | Obtém uma tarefa | 200, 404 |
| `POST` | `/api/tasks` | Cria uma tarefa e retorna seu código único (`id`) | 201, 400 |
| `PUT` | `/api/tasks/{id}` | Atualiza título, descrição, status e vencimento | 200, 400, 404 |
| `DELETE` | `/api/tasks/{id}` | Exclui uma tarefa | 204, 404 |

### Modelo da tarefa

| Campo | Tipo | Regras |
| --- | --- | --- |
| `id` | GUID | Gerado pelo sistema (código único) |
| `title` | string | Obrigatório, até 200 caracteres |
| `description` | string | Opcional, até 1000 caracteres |
| `dueDate` | data (`dd-MM-yyyy`) | Opcional; na criação não pode estar no passado |
| `status` | enum | `Pending` (Pendente), `InProgress` (Em progresso), `Completed` (Concluída). Padrão na criação: `Pending` |
| `createdAt` / `updatedAt` | data/hora (`dd-MM-yyyy HH:mm:ss`, UTC) | Preenchidos pelo sistema |

> Todas as datas usam o formato brasileiro `dd-MM-yyyy` (corpo JSON e query string). Outros formatos, como `2026-12-31`, são rejeitados com 400.

### Filtros da listagem

Todos opcionais e combináveis (lógica **E**):

| Parâmetro | Descrição |
| --- | --- |
| `status` | Status da tarefa |
| `dueDate` | Vencimento exatamente nesta data |
| `dueDateFrom` / `dueDateTo` | Intervalo de vencimento (inclusivo). `dueDateFrom` deve ser ≤ `dueDateTo` |

A listagem é ordenada por data de vencimento (tarefas sem vencimento ao final).

### Erros

Todos os erros seguem o padrão **ProblemDetails (RFC 7807)**, por exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Um ou mais erros de validação ocorreram.",
  "status": 400,
  "errors": { "Title": ["O título é obrigatório."] }
}
```

## Arquitetura

Foi adotada uma **arquitetura em camadas inspirada na Clean Architecture**, em que as dependências apontam sempre para o centro (domínio):

```
Api ──► Application ──► Domain
 │           ▲
 └──► Infrastructure
```

```
src/
├── GestaoTarefas.Domain          # Entidade TaskItem e enum TaskItemStatus (sem dependências)
├── GestaoTarefas.Application     # DTOs, interfaces (ITaskService, ITaskRepository), serviço, validadores, exceções
├── GestaoTarefas.Infrastructure  # EF Core (AppDbContext, configurações) e implementação do repositório
└── GestaoTarefas.Api             # Controllers, tratamento global de erros, Swagger, composição (DI)
tests/
├── GestaoTarefas.UnitTests
└── GestaoTarefas.IntegrationTests
```

| Camada | Responsabilidade |
| --- | --- |
| **Domain** | Modelo de negócio. A entidade expõe setters privados e só muda de estado por métodos próprios (`Update`). |
| **Application** | Casos de uso (`TaskService`), validação e contratos. Define a interface do repositório, mas não conhece o EF Core. |
| **Infrastructure** | Detalhes de persistência. Implementa `ITaskRepository` com EF Core InMemory. |
| **Api** | Camada HTTP: recebe DTOs, delega ao serviço e traduz resultados/exceções em códigos HTTP. |

**Por que este modelo?** Ele separa claramente as responsabilidades pedidas no desafio (controlador, serviço, repositório), mantém as regras de negócio independentes de framework/banco e torna o código facilmente testável. Trocar o InMemory por SQL Server, por exemplo, exige mudanças apenas na Infrastructure. Para o tamanho do problema, evitou-se complexidade extra (CQRS, MediatR, Unit of Work), mantendo a solução simples.

### SOLID na prática

- **S** – Cada classe tem uma responsabilidade: controller (HTTP), serviço (regras), validadores (entrada), repositório (persistência), `GlobalExceptionHandler` (erros).
- **O** – Novas regras de validação ou tratamentos de erro são adicionados sem alterar o serviço/controller.
- **L** – Qualquer implementação de `ITaskRepository`/`ITaskService` pode substituir a atual (ex.: mocks nos testes).
- **I** – Interfaces pequenas e específicas (`ITaskService`, `ITaskRepository`).
- **D** – Controller e serviço dependem de abstrações, resolvidas via injeção de dependência. Até o relógio é injetado (`TimeProvider`), tornando as regras de data testáveis.

## Decisões técnicas

- **DTOs** separam o contrato da API da entidade de domínio (`CreateTaskRequest`, `UpdateTaskRequest`, `TaskFilter`, `TaskResponse`).
- **Validação** com FluentValidation executada na camada de aplicação, garantindo as regras independentemente de quem chama o serviço. Erros de formato no JSON (ex.: status inexistente) são rejeitados pelo próprio ASP.NET Core com 400.
- **Tratamento de erros** centralizado via `IExceptionHandler`: `ValidationException` → 400, `NotFoundException` → 404, demais → 500 (sem expor detalhes internos).
- **Logging** com `ILogger` nativo: criação/edição/exclusão em nível `Information`, recursos não encontrados em `Warning` e falhas inesperadas em `Error`.
- **Status como string** no JSON (`JsonStringEnumConverter`) para uma API mais legível.
- **Swagger** enriquecido com comentários XML (resumos, exemplos e códigos de resposta), habilitado no ambiente de desenvolvimento.
- **Código único** da tarefa é um `Guid`, gerado no domínio.
