using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GestaoTarefas.Api.Serialization;

/// <summary>
/// Faz o binding de <see cref="DateOnly"/> vindo da rota/query string no formato dd-MM-yyyy.
/// </summary>
public sealed class DateOnlyModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);
        var value = valueResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
            return Task.CompletedTask;

        if (DateFormats.TryParseDate(value, out var date))
            bindingContext.Result = ModelBindingResult.Success(date);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Data inválida. Use o formato {DateFormats.Date}.");

        return Task.CompletedTask;
    }
}

public sealed class DateOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(DateOnly) ? new DateOnlyModelBinder() : null;
    }
}
