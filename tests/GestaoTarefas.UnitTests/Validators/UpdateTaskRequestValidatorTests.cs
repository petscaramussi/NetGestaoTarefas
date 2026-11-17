using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Application.Validators;
using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.UnitTests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_IsValid()
    {
        var request = new UpdateTaskRequest { Title = "Tarefa", Status = TaskItemStatus.Completed };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_WithPastDueDate_IsValid()
    {
        var request = new UpdateTaskRequest { Title = "Tarefa", Status = TaskItemStatus.Pending, DueDate = new DateOnly(2000, 1, 1) };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyTitle_IsInvalid()
    {
        var result = _validator.Validate(new UpdateTaskRequest { Title = "", Status = TaskItemStatus.Pending });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskRequest.Title));
    }

    [Fact]
    public void Validate_WithDefaultStatus_IsInvalid()
    {
        var result = _validator.Validate(new UpdateTaskRequest { Title = "Tarefa" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTaskRequest.Status));
    }
}
