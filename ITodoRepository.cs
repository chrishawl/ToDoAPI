namespace TodoApi;

public interface ITodoRepository
{
    Task<IEnumerable<Todo>> GetAllAsync();
    Task<IEnumerable<Todo>> GetCompleteAsync();
    Task<Todo?> GetByIdAsync(string id);
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo?> UpdateAsync(string id, Todo todo);
    Task<bool> DeleteAsync(string id);
}