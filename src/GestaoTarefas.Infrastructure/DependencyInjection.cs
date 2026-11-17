using GestaoTarefas.Application.Abstractions;
using GestaoTarefas.Infrastructure.Persistence;
using GestaoTarefas.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoTarefas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databaseName = "GestaoTarefasDb")
    {
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
