using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Application.Validators;
using GestaoTarefas.Domain.Enums;

namespace GestaoTarefas.UnitTests.Validators;

public class TaskFilterValidatorTests
{
    private readonly TaskFilterValidator _validator = new();

    [Fact]
    public void Validate_WithoutFilters_IsValid()
    {
        Assert.True(_validator.Validate(new TaskFilter()).IsValid);
    }

    [Fact]
    public void Validate_WithValidRange_IsValid()
    {
        var filter = new TaskFilter
        {
            Status = TaskItemStatus.InProgress,
            DueDateFrom = new DateOnly(2026, 1, 1),
            DueDateTo = new DateOnly(2026, 1, 1)
        };

        Assert.True(_validator.Validate(filter).IsValid);
    }

    [Fact]
    public void Validate_WithFromAfterTo_IsInvalid()
    {
        var filter = new TaskFilter { DueDateFrom = new DateOnly(2026, 2, 1), DueDateTo = new DateOnly(2026, 1, 1) };

        Assert.False(_validator.Validate(filter).IsValid);
    }

    [Fact]
    public void Validate_WithUndefinedStatus_IsInvalid()
    {
        Assert.False(_validator.Validate(new TaskFilter { Status = (TaskItemStatus)42 }).IsValid);
    }
}
