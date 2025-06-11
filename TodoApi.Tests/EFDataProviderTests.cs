using Microsoft.EntityFrameworkCore;
using TodoApi;
using Xunit;

namespace TodoApi.Tests;

public class EFDataProviderTests : IDisposable
{
    private readonly TodoDb _context;
    private readonly EFDataProvider _dataProvider;

    public EFDataProviderTests()
    {
        var options = new DbContextOptionsBuilder<TodoDb>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new TodoDb(options);
        _dataProvider = new EFDataProvider(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        var todos = new List<Todo>
        {
            new Todo { Id = "1", Name = "Todo 1", IsComplete = false },
            new Todo { Id = "2", Name = "Todo 2", IsComplete = true }
        };
        
        _context.Todos.AddRange(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataProvider.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Id == "1" && t.Name == "Todo 1");
        Assert.Contains(result, t => t.Id == "2" && t.Name == "Todo 2");
    }

    [Fact]
    public async Task GetCompleteAsync_ShouldReturnOnlyCompleteTodos()
    {
        // Arrange
        var todos = new List<Todo>
        {
            new Todo { Id = "1", Name = "Incomplete Todo", IsComplete = false },
            new Todo { Id = "2", Name = "Complete Todo 1", IsComplete = true },
            new Todo { Id = "3", Name = "Complete Todo 2", IsComplete = true }
        };
        
        _context.Todos.AddRange(todos);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataProvider.GetCompleteAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, todo => Assert.True(todo.IsComplete));
        Assert.Contains(result, t => t.Id == "2");
        Assert.Contains(result, t => t.Id == "3");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTodo_WhenExists()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "Test Todo", IsComplete = false };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataProvider.GetByIdAsync("1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Test Todo", result.Name);
        Assert.False(result.IsComplete);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _dataProvider.GetByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTodoToDatabase()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "New Todo", IsComplete = false };

        // Act
        var result = await _dataProvider.CreateAsync(todo);

        // Assert
        Assert.Equal(todo, result);
        
        var savedTodo = await _context.Todos.FindAsync("1");
        Assert.NotNull(savedTodo);
        Assert.Equal("New Todo", savedTodo.Name);
        Assert.False(savedTodo.IsComplete);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingTodo()
    {
        // Arrange
        var originalTodo = new Todo { Id = "1", Name = "Original Name", IsComplete = false };
        _context.Todos.Add(originalTodo);
        await _context.SaveChangesAsync();

        var updatedTodo = new Todo { Id = "1", Name = "Updated Name", IsComplete = true };

        // Act
        var result = await _dataProvider.UpdateAsync("1", updatedTodo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.True(result.IsComplete);

        var savedTodo = await _context.Todos.FindAsync("1");
        Assert.NotNull(savedTodo);
        Assert.Equal("Updated Name", savedTodo.Name);
        Assert.True(savedTodo.IsComplete);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenTodoNotExists()
    {
        // Arrange
        var updatedTodo = new Todo { Id = "nonexistent", Name = "Updated Name", IsComplete = true };

        // Act
        var result = await _dataProvider.UpdateAsync("nonexistent", updatedTodo);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTodo_WhenExists()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "To Delete", IsComplete = false };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dataProvider.DeleteAsync("1");

        // Assert
        Assert.True(result);
        
        var deletedTodo = await _context.Todos.FindAsync("1");
        Assert.Null(deletedTodo);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTodoNotExists()
    {
        // Act
        var result = await _dataProvider.DeleteAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}