namespace TodoApi;

public class TodoRepository : ITodoRepository
{
    private readonly IDataProvider _dataProvider;

    public TodoRepository(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<IEnumerable<Todo>> GetAllAsync()
    {
        return await _dataProvider.GetAllAsync();
    }

    public async Task<IEnumerable<Todo>> GetCompleteAsync()
    {
        return await _dataProvider.GetCompleteAsync();
    }

    public async Task<Todo?> GetByIdAsync(string id)
    {
        return await _dataProvider.GetByIdAsync(id);
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        // Always generate a new GUID on server - never trust client-provided IDs
        todo.Id = Guid.NewGuid().ToString();
        
        return await _dataProvider.CreateAsync(todo);
    }

    public async Task<Todo?> UpdateAsync(string id, Todo todo)
    {
        // Fetch existing todo to update only allowed fields - never trust client IDs
        var existingTodo = await _dataProvider.GetByIdAsync(id);
        if (existingTodo is null) 
            return null;

        // Update only the allowed fields, preserve server-controlled ID
        existingTodo.Name = todo.Name;
        existingTodo.IsComplete = todo.IsComplete;
        
        return await _dataProvider.UpdateAsync(id, existingTodo);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _dataProvider.DeleteAsync(id);
    }
}