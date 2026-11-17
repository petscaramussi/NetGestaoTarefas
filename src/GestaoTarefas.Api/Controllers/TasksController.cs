using GestaoTarefas.Application.Abstractions;
using GestaoTarefas.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestaoTarefas.Api.Controllers;

/// <summary>
/// Gerenciamento de tarefas.
/// </summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    /// <summary>
    /// Lista as tarefas cadastradas, com filtros opcionais por status e/ou data de vencimento.
    /// </summary>
    /// <remarks>
    /// Exemplos:
    ///
    ///     GET /api/tasks
    ///     GET /api/tasks?status=Pending
    ///     GET /api/tasks?dueDate=31-12-2026
    ///     GET /api/tasks?status=InProgress&amp;dueDateFrom=01-12-2026&amp;dueDateTo=31-12-2026
    /// </remarks>
    /// <response code="200">Lista de tarefas (ordenada por data de vencimento; tarefas sem vencimento ao final).</response>
    /// <response code="400">Filtros inválidos.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List([FromQuery] TaskFilter filter, CancellationToken cancellationToken)
    {
        var tasks = await taskService.ListAsync(filter, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Obtém uma tarefa pelo seu código único.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    /// <remarks>
    /// Status aceitos: <c>Pending</c> (Pendente), <c>InProgress</c> (Em progresso), <c>Completed</c> (Concluída).
    /// Datas no formato <c>dd-MM-yyyy</c>.
    /// </remarks>
    /// <response code="201">Tarefa criada. O corpo contém o código único (<c>id</c>) gerado.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Atualiza título, descrição, status e data de vencimento de uma tarefa existente.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="request">Novos dados da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Remove uma tarefa.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Tarefa removida.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
