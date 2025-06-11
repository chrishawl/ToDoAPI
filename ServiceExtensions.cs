using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;

namespace TodoApi;

public static class ServiceExtensions
{
    public static IServiceCollection AddTodoRepository(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DatabaseProvider"] ?? "InMemory";

        if (databaseProvider.Equals("CosmosDb", StringComparison.OrdinalIgnoreCase))
        {
            services.AddCosmosDb(configuration);
        }
        else
        {
            services.AddEntityFramework();
        }

        return services;
    }

    private static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["CosmosDb:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("CosmosDb:ConnectionString is required when using CosmosDb provider");
        }

        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            return new CosmosClient(connectionString);
        });

        services.AddScoped<ITodoRepository, CosmosTodoRepository>();
        return services;
    }

    private static IServiceCollection AddEntityFramework(this IServiceCollection services)
    {
        services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
        services.AddScoped<ITodoRepository, TodoRepository>();
        return services;
    }
}