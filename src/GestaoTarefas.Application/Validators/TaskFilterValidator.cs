using FluentValidation;
using GestaoTarefas.Application.DTOs;

namespace GestaoTarefas.Application.Validators;

public sealed class TaskFilterValidator : AbstractValidator<TaskFilter>
{
    public TaskFilterValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Status inválido. Valores aceitos: Pending, InProgress, Completed.");

        RuleFor(x => x)
            .Must(x => x.DueDateFrom <= x.DueDateTo)
            .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue)
            .WithName(nameof(TaskFilter.DueDateFrom))
            .WithMessage("DueDateFrom deve ser menor ou igual a DueDateTo.");
    }
}
