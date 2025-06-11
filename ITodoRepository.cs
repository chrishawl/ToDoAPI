namespace TodoApi;

public interface ITodoRepository
{
    Task<IEnumerable<Todo>> GetAllAsync();
    Task<IEnumerable<Todo>> GetCompleteAsync();
    Task<Todo?> GetByIdAsync(int id);
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo?> UpdateAsync(int id, Todo todo);
    Task<bool> DeleteAsync(int id);
}