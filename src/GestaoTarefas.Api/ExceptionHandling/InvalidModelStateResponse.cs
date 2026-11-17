using GestaoTarefas.Api.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoTarefas.Api.ExceptionHandling;

/// <summary>
/// Substitui as mensagens padrão (em inglês) de erros de model binding/JSON por mensagens em português.
/// </summary>
public static class InvalidModelStateResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .GroupBy(entry => NormalizeKey(entry.Key))
            .ToDictionary(group => group.Key, group => new[] { BuildMessage(group.Key) });

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais erros de validação ocorreram.",
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    }

    // Chaves de erro do System.Text.Json vêm como "$.campo"; "request"/"$" indicam corpo ausente ou malformado
    private static string NormalizeKey(string key)
    {
        var field = key.TrimStart('$', '.');
        return field is "" or "request" ? "body" : field;
    }

    private static string BuildMessage(string field) => field switch
    {
        "body" => "O corpo da requisição é obrigatório e deve ser um JSON válido.",
        _ when field.Contains("date", StringComparison.OrdinalIgnoreCase) =>
            $"Data inválida para '{field}'. Use o formato {DateFormats.Date}.",
        _ => $"O valor informado para '{field}' é inválido ou está em formato incorreto."
    };
}
