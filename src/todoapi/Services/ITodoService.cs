/// <summary>
/// Defines the interface for the todo service.
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// Gets the list of all todo items.
    /// </summary>
    /// <returns>A list of all todo items.</returns>
    Task<List<Todo>> GetListAsync();

    /// <summary>
    /// Saves the specified todo item list to the state store.
    /// </summary>
    /// <param name="todoList">The list of todo items to save.</param>
    Task SaveListAsync(List<Todo> todoList);

    /// <summary>
    /// Gets the todo item with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to retrieve.</param>
    /// <returns>The todo item with the specified ID.</returns>
    Task<Todo> GetTodoAsync(int id);

    /// <summary>
    /// Adds the specified todo item to the list and saves it to the state store.
    /// </summary>
    /// <param name="todo">The todo item to add.</param>
    /// <returns>The added todo item.</returns>
    Task<Todo> AddAsync(Todo todo);

    /// <summary>
    /// Updates the specified todo item in the list and saves it to the state store.
    /// </summary>
    /// <param name="todo">The todo item to update.</param>
    Task UpdateAsync(Todo todo);

    /// <summary>
    /// Deletes the todo item with the specified ID from the list and saves the list to the state store.
    /// </summary>
    /// <param name="id">The ID of the todo item to delete.</param>
    Task DeleteAsync(int id);

}