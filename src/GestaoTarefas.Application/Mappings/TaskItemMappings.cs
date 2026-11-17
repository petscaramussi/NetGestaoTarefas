using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Domain.Entities;

namespace GestaoTarefas.Application.Mappings;

public static class TaskItemMappings
{
    public static TaskResponse ToResponse(this TaskItem task) =>
        new(task.Id, task.Title, task.Description, task.DueDate, task.Status, task.CreatedAt, task.UpdatedAt);
}
