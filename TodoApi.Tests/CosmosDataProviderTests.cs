using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using TodoApi;
using Xunit;

namespace TodoApi.Tests;

public class CosmosDataProviderTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly CosmosDataProvider _dataProvider;

    public CosmosDataProviderTests()
    {
        _mockContainer = new Mock<Container>();
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["CosmosDb:DatabaseName"]).Returns("TodoDB");
        _mockConfiguration.Setup(c => c["CosmosDb:ContainerName"]).Returns("Todos");
        _mockCosmosClient.Setup(c => c.GetContainer("TodoDB", "Todos")).Returns(_mockContainer.Object);

        _dataProvider = new CosmosDataProvider(_mockCosmosClient.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTodo_WhenExists()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "Test Todo", IsComplete = false };
        var mockResponse = new Mock<ItemResponse<Todo>>();
        mockResponse.Setup(r => r.Resource).Returns(todo);
        
        _mockContainer.Setup(c => c.ReadItemAsync<Todo>("1", new PartitionKey("1"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _dataProvider.GetByIdAsync("1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Test Todo", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var cosmosException = new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(c => c.ReadItemAsync<Todo>("nonexistent", new PartitionKey("nonexistent"), null, default))
                     .ThrowsAsync(cosmosException);

        // Act
        var result = await _dataProvider.GetByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTodo()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "New Todo", IsComplete = false };
        var mockResponse = new Mock<ItemResponse<Todo>>();
        mockResponse.Setup(r => r.Resource).Returns(todo);
        
        _mockContainer.Setup(c => c.CreateItemAsync(todo, new PartitionKey(todo.Id), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _dataProvider.CreateAsync(todo);

        // Assert
        Assert.Equal(todo, result);
        _mockContainer.Verify(c => c.CreateItemAsync(todo, new PartitionKey(todo.Id), null, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTodo_WhenExists()
    {
        // Arrange
        var todo = new Todo { Id = "1", Name = "Updated Todo", IsComplete = true };
        var mockResponse = new Mock<ItemResponse<Todo>>();
        mockResponse.Setup(r => r.Resource).Returns(todo);
        
        _mockContainer.Setup(c => c.ReplaceItemAsync(todo, "1", new PartitionKey("1"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _dataProvider.UpdateAsync("1", todo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo, result);
        _mockContainer.Verify(c => c.ReplaceItemAsync(todo, "1", new PartitionKey("1"), null, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenTodoNotExists()
    {
        // Arrange
        var todo = new Todo { Id = "nonexistent", Name = "Updated Todo", IsComplete = true };
        var cosmosException = new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(c => c.ReplaceItemAsync(todo, "nonexistent", new PartitionKey("nonexistent"), null, default))
                     .ThrowsAsync(cosmosException);

        // Act
        var result = await _dataProvider.UpdateAsync("nonexistent", todo);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenTodoExists()
    {
        // Arrange
        var mockResponse = new Mock<ItemResponse<Todo>>();
        _mockContainer.Setup(c => c.DeleteItemAsync<Todo>("1", new PartitionKey("1"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _dataProvider.DeleteAsync("1");

        // Assert
        Assert.True(result);
        _mockContainer.Verify(c => c.DeleteItemAsync<Todo>("1", new PartitionKey("1"), null, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTodoNotExists()
    {
        // Arrange
        var cosmosException = new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(c => c.DeleteItemAsync<Todo>("nonexistent", new PartitionKey("nonexistent"), null, default))
                     .ThrowsAsync(cosmosException);

        // Act
        var result = await _dataProvider.DeleteAsync("nonexistent");

        // Assert
        Assert.False(result);
    }
}