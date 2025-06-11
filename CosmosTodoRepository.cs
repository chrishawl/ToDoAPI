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
        var query = _container.GetItemQueryIterator<Todo>(
            requestOptions: new QueryRequestOptions 
            { 
                MaxItemCount = -1
            });
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
        var query = _container.GetItemQueryIterator<Todo>(
            queryDefinition,
            requestOptions: new QueryRequestOptions 
            { 
                MaxItemCount = -1
            });
        var results = new List<Todo>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        
        return results;
    }

    public async Task<Todo?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Todo>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        // Generate GUID if not provided
        if (string.IsNullOrEmpty(todo.Id))
        {
            todo.Id = Guid.NewGuid().ToString();
        }
        
        var response = await _container.CreateItemAsync(todo, new PartitionKey(todo.Id));
        return response.Resource;
    }

    public async Task<Todo?> UpdateAsync(string id, Todo todo)
    {
        try
        {
            todo.Id = id; // Ensure the ID matches
            var response = await _container.ReplaceItemAsync(todo, id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<Todo>(id, new PartitionKey(id));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}