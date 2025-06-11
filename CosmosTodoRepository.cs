using Microsoft.Azure.Cosmos;

namespace TodoApi;

public class CosmosTodoRepository : ITodoRepository
{
    private readonly Container _container;

    public CosmosTodoRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "TodoDB";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "Todos";
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Todo>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Todo>();
        var results = new List<Todo>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        
        return results;
    }

    public async Task<IEnumerable<Todo>> GetCompleteAsync()
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.IsComplete = true");
        var query = _container.GetItemQueryIterator<Todo>(queryDefinition);
        var results = new List<Todo>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        
        return results;
    }

    public async Task<Todo?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Todo>(id.ToString(), new PartitionKey(id.ToString()));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        // Generate ID if not provided (for Cosmos DB, we'll use a unique identifier)
        if (todo.Id == 0)
        {
            todo.Id = new Random().Next(1, int.MaxValue);
        }
        
        var response = await _container.CreateItemAsync(todo, new PartitionKey(todo.Id.ToString()));
        return response.Resource;
    }

    public async Task<Todo?> UpdateAsync(int id, Todo todo)
    {
        try
        {
            todo.Id = id; // Ensure the ID matches
            var response = await _container.ReplaceItemAsync(todo, id.ToString(), new PartitionKey(id.ToString()));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _container.DeleteItemAsync<Todo>(id.ToString(), new PartitionKey(id.ToString()));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}