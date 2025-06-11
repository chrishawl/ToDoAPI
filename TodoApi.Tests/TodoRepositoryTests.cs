using Moq;
using TodoApi;
using Xunit;

namespace TodoApi.Tests;

public class TodoRepositoryTests
{
    private readonly Mock<IDataProvider> _mockDataProvider;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        _mockDataProvider = new Mock<IDataProvider>();
        _repository = new TodoRepository(_mockDataProvider.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        var expectedTodos = new List<Todo>
        {
            new Todo { Id = "1", Name = "Test Todo 1", IsComplete = false },
            new Todo { Id = "2", Name = "Test Todo 2", IsComplete = true }
        };
        _mockDataProvider.Setup(dp => dp.GetAllAsync()).ReturnsAsync(expectedTodos);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(expectedTodos, result);
        _mockDataProvider.Verify(dp => dp.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCompleteAsync_ShouldReturnCompleteTodos()
    {
        // Arrange
        var expectedTodos = new List<Todo>
        {
            new Todo { Id = "1", Name = "Completed Todo", IsComplete = true }
        };
        _mockDataProvider.Setup(dp => dp.GetCompleteAsync()).ReturnsAsync(expectedTodos);

        // Act
        var result = await _repository.GetCompleteAsync();

        // Assert
        Assert.Equal(expectedTodos, result);
        _mockDataProvider.Verify(dp => dp.GetCompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTodo_WhenExists()
    {
        // Arrange
        var expectedTodo = new Todo { Id = "1", Name = "Test Todo", IsComplete = false };
        _mockDataProvider.Setup(dp => dp.GetByIdAsync("1")).ReturnsAsync(expectedTodo);

        // Act
        var result = await _repository.GetByIdAsync("1");

        // Assert
        Assert.Equal(expectedTodo, result);
        _mockDataProvider.Verify(dp => dp.GetByIdAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockDataProvider.Setup(dp => dp.GetByIdAsync("nonexistent")).ReturnsAsync((Todo?)null);

        // Act
        var result = await _repository.GetByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
        _mockDataProvider.Verify(dp => dp.GetByIdAsync("nonexistent"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateGuid_AndIgnoreClientId()
    {
        // Arrange
        var inputTodo = new Todo { Id = "client-provided-id", Name = "Test Todo", IsComplete = false };
        var expectedTodo = new Todo { Id = "server-generated-id", Name = "Test Todo", IsComplete = false };
        
        _mockDataProvider.Setup(dp => dp.CreateAsync(It.IsAny<Todo>()))
                         .ReturnsAsync((Todo todo) => new Todo { Id = todo.Id, Name = todo.Name, IsComplete = todo.IsComplete });

        // Act
        var result = await _repository.CreateAsync(inputTodo);

        // Assert
        Assert.NotEqual("client-provided-id", result.Id);
        Assert.True(Guid.TryParse(result.Id, out _)); // Verify it's a valid GUID
        Assert.Equal("Test Todo", result.Name);
        Assert.False(result.IsComplete);
        _mockDataProvider.Verify(dp => dp.CreateAsync(It.Is<Todo>(t => 
            t.Id != "client-provided-id" && 
            t.Name == "Test Todo" &&
            t.IsComplete == false)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOnlyAllowedFields_WhenTodoExists()
    {
        // Arrange
        var existingTodo = new Todo { Id = "existing-id", Name = "Old Name", IsComplete = false };
        var updateTodo = new Todo { Id = "hacker-id", Name = "New Name", IsComplete = true };
        var expectedUpdatedTodo = new Todo { Id = "existing-id", Name = "New Name", IsComplete = true };

        _mockDataProvider.Setup(dp => dp.GetByIdAsync("existing-id")).ReturnsAsync(existingTodo);
        _mockDataProvider.Setup(dp => dp.UpdateAsync("existing-id", It.IsAny<Todo>()))
                         .ReturnsAsync(expectedUpdatedTodo);

        // Act
        var result = await _repository.UpdateAsync("existing-id", updateTodo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("existing-id", result.Id); // ID should not change
        Assert.Equal("New Name", result.Name);
        Assert.True(result.IsComplete);
        
        _mockDataProvider.Verify(dp => dp.GetByIdAsync("existing-id"), Times.Once);
        _mockDataProvider.Verify(dp => dp.UpdateAsync("existing-id", It.Is<Todo>(t => 
            t.Id == "existing-id" && 
            t.Name == "New Name" && 
            t.IsComplete == true)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenTodoNotExists()
    {
        // Arrange
        var updateTodo = new Todo { Id = "hacker-id", Name = "New Name", IsComplete = true };
        _mockDataProvider.Setup(dp => dp.GetByIdAsync("nonexistent")).ReturnsAsync((Todo?)null);

        // Act
        var result = await _repository.UpdateAsync("nonexistent", updateTodo);

        // Assert
        Assert.Null(result);
        _mockDataProvider.Verify(dp => dp.GetByIdAsync("nonexistent"), Times.Once);
        _mockDataProvider.Verify(dp => dp.UpdateAsync(It.IsAny<string>(), It.IsAny<Todo>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenTodoExists()
    {
        // Arrange
        _mockDataProvider.Setup(dp => dp.DeleteAsync("existing-id")).ReturnsAsync(true);

        // Act
        var result = await _repository.DeleteAsync("existing-id");

        // Assert
        Assert.True(result);
        _mockDataProvider.Verify(dp => dp.DeleteAsync("existing-id"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTodoNotExists()
    {
        // Arrange
        _mockDataProvider.Setup(dp => dp.DeleteAsync("nonexistent")).ReturnsAsync(false);

        // Act
        var result = await _repository.DeleteAsync("nonexistent");

        // Assert
        Assert.False(result);
        _mockDataProvider.Verify(dp => dp.DeleteAsync("nonexistent"), Times.Once);
    }
}