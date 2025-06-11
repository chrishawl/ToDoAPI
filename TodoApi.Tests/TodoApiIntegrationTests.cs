using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TodoApi;
using Xunit;

namespace TodoApi.Tests;

public class TodoApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TodoApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<TodoDb>));
                services.RemoveAll(typeof(TodoDb));
                
                // Add a test database
                services.AddDbContext<TodoDb>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
                });
                
                // Ensure we're using the EF data provider for integration tests
                services.RemoveAll(typeof(IDataProvider));
                services.AddScoped<IDataProvider, EFDataProvider>();
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTodos_ShouldReturnEmptyList_WhenNoTodos()
    {
        // Act
        var response = await _client.GetAsync("/todoitems");

        // Assert
        response.EnsureSuccessStatusCode();
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task CreateTodo_ShouldCreateTodoWithServerGeneratedId()
    {
        // Arrange
        var newTodo = new Todo { Id = "client-provided-id", Name = "Test Todo", IsComplete = false };

        // Act
        var response = await _client.PostAsJsonAsync("/todoitems", newTodo);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdTodo = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(createdTodo);
        Assert.NotEqual("client-provided-id", createdTodo.Id); // Server should ignore client ID
        Assert.True(Guid.TryParse(createdTodo.Id, out _)); // Should be a valid GUID
        Assert.Equal("Test Todo", createdTodo.Name);
        Assert.False(createdTodo.IsComplete);
        
        // Verify location header
        Assert.NotNull(response.Headers.Location);
        Assert.Contains(createdTodo.Id, response.Headers.Location.ToString());
    }

    [Fact]
    public async Task GetTodoById_ShouldReturnTodo_WhenExists()
    {
        // Arrange - Create a todo first
        var newTodo = new Todo { Name = "Test Todo", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/todoitems", newTodo);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();

        // Act
        var response = await _client.GetAsync($"/todoitems/{createdTodo!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrievedTodo = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(retrievedTodo);
        Assert.Equal(createdTodo.Id, retrievedTodo.Id);
        Assert.Equal("Test Todo", retrievedTodo.Name);
        Assert.False(retrievedTodo.IsComplete);
    }

    [Fact]
    public async Task GetTodoById_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/todoitems/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTodo_ShouldUpdateOnlyAllowedFields()
    {
        // Arrange - Create a todo first
        var newTodo = new Todo { Name = "Original Name", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/todoitems", newTodo);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();

        // Act - Try to update with a different ID (should be ignored)
        var updateTodo = new Todo { Id = "hacker-id", Name = "Updated Name", IsComplete = true };
        var response = await _client.PutAsJsonAsync($"/todoitems/{createdTodo!.Id}", updateTodo);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the update by getting the todo
        var getResponse = await _client.GetAsync($"/todoitems/{createdTodo.Id}");
        var updatedTodo = await getResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(updatedTodo);
        Assert.Equal(createdTodo.Id, updatedTodo.Id); // ID should not change
        Assert.Equal("Updated Name", updatedTodo.Name);
        Assert.True(updatedTodo.IsComplete);
    }

    [Fact]
    public async Task UpdateTodo_ShouldReturnNotFound_WhenTodoNotExists()
    {
        // Arrange
        var updateTodo = new Todo { Name = "Updated Name", IsComplete = true };

        // Act
        var response = await _client.PutAsJsonAsync("/todoitems/nonexistent-id", updateTodo);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_ShouldReturnNoContent_WhenTodoExists()
    {
        // Arrange - Create a todo first
        var newTodo = new Todo { Name = "To Delete", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/todoitems", newTodo);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();

        // Act
        var response = await _client.DeleteAsync($"/todoitems/{createdTodo!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the todo is deleted
        var getResponse = await _client.GetAsync($"/todoitems/{createdTodo.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_ShouldReturnNotFound_WhenTodoNotExists()
    {
        // Act
        var response = await _client.DeleteAsync("/todoitems/nonexistent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCompleteTodos_ShouldReturnOnlyCompleteTodos()
    {
        // Arrange - Create multiple todos
        var todo1 = new Todo { Name = "Incomplete Todo", IsComplete = false };
        var todo2 = new Todo { Name = "Complete Todo 1", IsComplete = true };
        var todo3 = new Todo { Name = "Complete Todo 2", IsComplete = true };

        await _client.PostAsJsonAsync("/todoitems", todo1);
        await _client.PostAsJsonAsync("/todoitems", todo2);
        await _client.PostAsJsonAsync("/todoitems", todo3);

        // Act
        var response = await _client.GetAsync("/todoitems/complete");

        // Assert
        response.EnsureSuccessStatusCode();
        var completeTodos = await response.Content.ReadFromJsonAsync<List<Todo>>();
        Assert.NotNull(completeTodos);
        Assert.Equal(2, completeTodos.Count);
        Assert.All(completeTodos, todo => Assert.True(todo.IsComplete));
    }

    [Fact]
    public async Task FullWorkflow_ShouldWorkEndToEnd()
    {
        // Create
        var newTodo = new Todo { Name = "Workflow Test", IsComplete = false };
        var createResponse = await _client.PostAsJsonAsync("/todoitems", newTodo);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(createdTodo);

        // Read
        var getResponse = await _client.GetAsync($"/todoitems/{createdTodo.Id}");
        getResponse.EnsureSuccessStatusCode();
        var retrievedTodo = await getResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.Equal(createdTodo.Id, retrievedTodo!.Id);

        // Update
        var updateTodo = new Todo { Name = "Updated Workflow Test", IsComplete = true };
        var updateResponse = await _client.PutAsJsonAsync($"/todoitems/{createdTodo.Id}", updateTodo);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($"/todoitems/{createdTodo.Id}");
        var updatedTodo = await getUpdatedResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.Equal("Updated Workflow Test", updatedTodo!.Name);
        Assert.True(updatedTodo.IsComplete);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/todoitems/{createdTodo.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($"/todoitems/{createdTodo.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }
}