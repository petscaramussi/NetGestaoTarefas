using GestaoTarefas.Application.Validators;
using GestaoTarefas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestaoTarefas.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Title).IsRequired().HasMaxLength(TaskValidationRules.TitleMaxLength);
        builder.Property(t => t.Description).HasMaxLength(TaskValidationRules.DescriptionMaxLength);
        builder.Property(t => t.Status).IsRequired();
    }
}
