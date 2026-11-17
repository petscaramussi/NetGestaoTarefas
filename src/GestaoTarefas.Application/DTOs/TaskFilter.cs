using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.Application.DTOs;

/// <summary>
/// Filtros opcionais para a listagem de tarefas. Todos os filtros informados são combinados (E).
/// </summary>
public sealed record TaskFilter
{
    /// <summary>Filtra pelo status da tarefa.</summary>
    public TaskItemStatus? Status { get; init; }

    /// <summary>Filtra tarefas com vencimento exatamente nesta data (dd-MM-yyyy).</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Filtra tarefas com vencimento a partir desta data, inclusive (dd-MM-yyyy).</summary>
    public DateOnly? DueDateFrom { get; init; }

    /// <summary>Filtra tarefas com vencimento até esta data, inclusive (dd-MM-yyyy).</summary>
    public DateOnly? DueDateTo { get; init; }
}
