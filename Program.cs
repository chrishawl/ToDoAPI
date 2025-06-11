using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.Cosmos;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// Configure database repository
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "InMemory";

if (databaseProvider.Equals("CosmosDb", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = builder.Configuration["CosmosDb:ConnectionString"];
    if (!string.IsNullOrEmpty(connectionString))
    {
        builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            return new CosmosClient(connectionString);
        });
        builder.Services.AddScoped<ITodoRepository, CosmosTodoRepository>();
    }
    else
    {
        // Fallback to InMemory if CosmosDb connection string is not provided
        builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
        builder.Services.AddScoped<ITodoRepository, TodoRepository>();
    }
}
else
{
    builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
    builder.Services.AddScoped<ITodoRepository, TodoRepository>();
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/todoitems", async (ITodoRepository repository) =>
    await repository.GetAllAsync());

app.MapGet("/todoitems/complete", async (ITodoRepository repository) =>
    await repository.GetCompleteAsync());

app.MapGet("/todoitems/{id}", async (string id, ITodoRepository repository) =>
    await repository.GetByIdAsync(id) is Todo todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, ITodoRepository repository) =>
{
    var createdTodo = await repository.CreateAsync(todo);
    return Results.Created($"/todoitems/{createdTodo.Id}", createdTodo);
});

app.MapPut("/todoitems/{id}", async (string id, Todo inputTodo, ITodoRepository repository) =>
{
    var updatedTodo = await repository.UpdateAsync(id, inputTodo);
    return updatedTodo is not null ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/todoitems/{id}", async (string id, ITodoRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();