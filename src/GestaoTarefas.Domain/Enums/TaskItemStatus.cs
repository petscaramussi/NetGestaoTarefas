namespace GestaoTarefas.Domain.Enums;

/// <summary>
/// Status possíveis de uma tarefa.
/// </summary>
public enum TaskItemStatus
{
    /// <summary>Pendente.</summary>
    Pending = 1,

    /// <summary>Em progresso.</summary>
    InProgress = 2,

    /// <summary>Concluída.</summary>
    Completed = 3
}
