using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.Domain.Entities;

/// <summary>
/// Representa uma tarefa. O estado só é alterado através dos métodos da própria entidade.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Exigido pelo EF Core
    private TaskItem() { }

    public TaskItem(string title, string? description, DateOnly? dueDate, TaskItemStatus status, DateTime createdAt)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
        CreatedAt = createdAt;
    }

    public void Update(string title, string? description, DateOnly? dueDate, TaskItemStatus status, DateTime updatedAt)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
        UpdatedAt = updatedAt;
    }
}
