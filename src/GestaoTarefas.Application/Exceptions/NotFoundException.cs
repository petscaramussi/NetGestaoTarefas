namespace GestaoTarefas.Application.Exceptions;

public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} com identificador '{key}' não foi encontrado(a).");
