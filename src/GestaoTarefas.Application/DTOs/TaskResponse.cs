using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.Application.DTOs;

/// <summary>
/// Representação de uma tarefa retornada pela API.
/// </summary>
/// <param name="Id">Código único da tarefa.</param>
/// <param name="Title">Título da tarefa.</param>
/// <param name="Description">Descrição da tarefa.</param>
/// <param name="DueDate">Data de vencimento (dd-MM-yyyy).</param>
/// <param name="Status">Status: Pending (Pendente), InProgress (Em progresso) ou Completed (Concluída).</param>
/// <param name="CreatedAt">Data/hora de criação (UTC, dd-MM-yyyy HH:mm:ss).</param>
/// <param name="UpdatedAt">Data/hora da última atualização (UTC, dd-MM-yyyy HH:mm:ss).</param>
public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    DateOnly? DueDate,
    TaskItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
