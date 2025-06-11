using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// Configure database repository
builder.Services.AddTodoRepository(builder.Configuration);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/todoitems", async (ITodoRepository repository) =>
    await repository.GetAllAsync());

app.MapGet("/todoitems/complete", async (ITodoRepository repository) =>
    await repository.GetCompleteAsync());

app.MapGet("/todoitems/{id}", async (int id, ITodoRepository repository) =>
    await repository.GetByIdAsync(id) is Todo todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapPost("/todoitems", async (Todo todo, ITodoRepository repository) =>
{
    var createdTodo = await repository.CreateAsync(todo);
    return Results.Created($"/todoitems/{createdTodo.Id}", createdTodo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, ITodoRepository repository) =>
{
    var updatedTodo = await repository.UpdateAsync(id, inputTodo);
    return updatedTodo is not null ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/todoitems/{id}", async (int id, ITodoRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();