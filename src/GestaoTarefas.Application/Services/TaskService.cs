using FluentValidation;
using GestaoTarefas.Application.Abstractions;
using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Application.Exceptions;
using GestaoTarefas.Application.Mappings;
using GestaoTarefas.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GestaoTarefas.Application.Services;

public sealed class TaskService(
    ITaskRepository repository,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator,
    IValidator<TaskFilter> filterValidator,
    TimeProvider timeProvider,
    ILogger<TaskService> logger) : ITaskService
{
    private const string ResourceName = "Tarefa";

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = new TaskItem(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.DueDate,
            request.Status,
            timeProvider.GetUtcNow().UtcDateTime);

        await repository.AddAsync(task, cancellationToken);

        logger.LogInformation("Tarefa {TaskId} criada com status {Status}.", task.Id, task.Status);
        return task.ToResponse();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingAsync(id, cancellationToken);
        return task.ToResponse();
    }

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var tasks = await repository.ListAsync(filter, cancellationToken);
        return tasks.Select(t => t.ToResponse()).ToList();
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await GetExistingAsync(id, cancellationToken);

        task.Update(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.DueDate,
            request.Status,
            timeProvider.GetUtcNow().UtcDateTime);

        await repository.UpdateAsync(task, cancellationToken);

        logger.LogInformation("Tarefa {TaskId} atualizada.", task.Id);
        return task.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetExistingAsync(id, cancellationToken);

        await repository.DeleteAsync(task, cancellationToken);

        logger.LogInformation("Tarefa {TaskId} removida.", id);
    }

    private async Task<TaskItem> GetExistingAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            logger.LogWarning("Tarefa {TaskId} não encontrada.", id);
            throw new NotFoundException(ResourceName, id);
        }

        return task;
    }
}
