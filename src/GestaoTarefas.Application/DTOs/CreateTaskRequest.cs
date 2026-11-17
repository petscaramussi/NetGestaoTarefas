using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.Application.DTOs;

/// <summary>
/// Dados para criação de uma tarefa.
/// </summary>
public sealed record CreateTaskRequest
{
    /// <summary>Título da tarefa (obrigatório, até 200 caracteres).</summary>
    /// <example>Estudar Entity Framework Core</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>Descrição da tarefa (opcional, até 1000 caracteres).</summary>
    /// <example>Revisar o provider InMemory e migrations.</example>
    public string? Description { get; init; }

    /// <summary>Data de vencimento (opcional, formato dd-MM-yyyy). Não pode estar no passado.</summary>
    /// <example>31-12-2026</example>
    public DateOnly? DueDate { get; init; }

    /// <summary>Status: Pending (Pendente), InProgress (Em progresso) ou Completed (Concluída). Padrão: Pending.</summary>
    /// <example>Pending</example>
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Pending;
}
