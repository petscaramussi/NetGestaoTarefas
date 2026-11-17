using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Application.Validators;
using GestaoTarefas.Domain.Enums;
using Microsoft.Extensions.Time.Testing;

namespace GestaoTarefas.UnitTests.Validators;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void Validate_WithOnlyTitle_IsValid()
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = "Tarefa" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithBlankTitle_IsInvalid(string title)
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = title });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Title));
    }

    [Fact]
    public void Validate_WithTitleTooLong_IsInvalid()
    {
        var request = new CreateTaskRequest { Title = new string('a', TaskValidationRules.TitleMaxLength + 1) };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Title));
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_IsInvalid()
    {
        var request = new CreateTaskRequest
        {
            Title = "Tarefa",
            Description = new string('a', TaskValidationRules.DescriptionMaxLength + 1)
        };

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Description));
    }

    [Fact]
    public void Validate_WithUndefinedStatus_IsInvalid()
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = "Tarefa", Status = (TaskItemStatus)99 });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Status));
    }

    [Theory]
    [InlineData(2026, 6, 15)]
    [InlineData(2026, 12, 31)]
    public void Validate_WithTodayOrFutureDueDate_IsValid(int year, int month, int day)
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = "Tarefa", DueDate = new DateOnly(year, month, day) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPastDueDate_IsInvalid()
    {
        var result = _validator.Validate(new CreateTaskRequest { Title = "Tarefa", DueDate = new DateOnly(2026, 6, 14) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTaskRequest.DueDate));
    }
}
