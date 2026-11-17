using System.Text.Json;
using System.Text.Json.Serialization;
using GestaoTarefas.Api.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestaoTarefas.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(), new DateOnlyJsonConverter(), new DateTimeJsonConverter() }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Banco em memória isolado por instância da factory
        builder.UseSetting("Database:Name", $"GestaoTarefasTests-{Guid.NewGuid()}");
    }
}
