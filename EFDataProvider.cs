using Microsoft.EntityFrameworkCore;

namespace TodoApi;

public class EFDataProvider : IDataProvider
{
    private readonly TodoDb _context;

    public EFDataProvider(TodoDb context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Todo>> GetAllAsync()
    {
        return await _context.Todos.ToListAsync();
    }

    public async Task<IEnumerable<Todo>> GetCompleteAsync()
    {
        return await _context.Todos.Where(t => t.IsComplete).ToListAsync();
    }

    public async Task<Todo?> GetByIdAsync(string id)
    {
        return await _context.Todos.FindAsync(id);
    }

    public async Task<Todo> CreateAsync(Todo todo)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateAsync(string id, Todo todo)
    {
        var existingTodo = await _context.Todos.FindAsync(id);
        if (existingTodo is null) 
            return null;

        existingTodo.Name = todo.Name;
        existingTodo.IsComplete = todo.IsComplete;
        await _context.SaveChangesAsync();
        
        return existingTodo;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo is null) 
            return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }
}