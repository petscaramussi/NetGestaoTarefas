using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GestaoTarefas.Api.ExceptionHandling;
using GestaoTarefas.Api.Serialization;
using GestaoTarefas.Application;
using GestaoTarefas.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options => options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
    })
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = InvalidModelStateResponse.Create);

builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    // Erros sem exceção (rota inexistente, método não permitido etc.)
    if (context.Exception is not null)
        return;

    context.ProblemDetails.Title = context.ProblemDetails.Status switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida.",
        StatusCodes.Status404NotFound => "Recurso não encontrado.",
        StatusCodes.Status405MethodNotAllowed => "Método HTTP não permitido para este recurso.",
        StatusCodes.Status415UnsupportedMediaType => "Tipo de conteúdo não suportado. Utilize application/json.",
        StatusCodes.Status500InternalServerError => "Ocorreu um erro inesperado ao processar a requisição.",
        _ => context.ProblemDetails.Title
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration["Database:Name"] ?? "GestaoTarefasDb");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestão de Tarefas API",
        Version = "v1",
        Description = "API RESTful para cadastro, listagem (com filtros), edição e exclusão de tarefas. " +
                      $"Datas no formato {DateFormats.Date} e data/hora no formato {DateFormats.DateTime} (UTC)."
    });

    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Pattern = @"^\d{2}-\d{2}-\d{4}$",
        Description = $"Data no formato {DateFormats.Date}.",
        Example = JsonValue.Create("31-12-2026")
    });
    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Description = $"Data/hora (UTC) no formato {DateFormats.DateTime}.",
        Example = JsonValue.Create("17-11-2026 14:30:00")
    });

    foreach (var assembly in new[] { Assembly.GetExecutingAssembly(), typeof(GestaoTarefas.Application.DependencyInjection).Assembly })
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestão de Tarefas API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
