using FluentValidation;
using GestaoTarefas.Application.DTOs;

namespace GestaoTarefas.Application.Validators;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(TaskValidationRules.TitleMaxLength)
            .WithMessage($"O título deve ter no máximo {TaskValidationRules.TitleMaxLength} caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(TaskValidationRules.DescriptionMaxLength)
            .WithMessage($"A descrição deve ter no máximo {TaskValidationRules.DescriptionMaxLength} caracteres.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido. Valores aceitos: Pending, InProgress, Completed.");

        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate >= DateOnly.FromDateTime(timeProvider.GetLocalNow().Date))
            .When(x => x.DueDate.HasValue)
            .WithMessage("A data de vencimento não pode estar no passado.");
    }
}
