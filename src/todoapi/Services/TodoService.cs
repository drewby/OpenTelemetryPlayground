
using Dapr.Client;

/// <summary>
/// Service class for managing todo items.
/// </summary>
public class TodoService
{
    private readonly ILogger<TodoService> _logger;
    private readonly DaprClient _daprClient;

     /// <summary>
    /// Initializes a new instance of the <see cref="TodoService"/> class with the specified logger and Dapr client.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="daprClient">The Dapr client instance to use for interacting with state store.</param>
    public TodoService(ILogger<TodoService> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;       
    }

    /// <summary>
    /// Gets the list of all todo items.
    /// </summary>
    /// <returns>A list of all todo items.</returns>
    public async Task<List<Todo>> GetList()
    {
        var todoList = await _daprClient.GetStateAsync<List<Todo>>("todos", "todoList");
        if (todoList == null)
        {
            throw new Exception("Unable to retrieve todo list");
        }
        
        return todoList;
    }

    /// <summary>
    /// Saves the specified todo item list to the state store.
    /// </summary>
    /// <param name="todoList">The list of todo items to save.</param>
    public async Task SaveList(List<Todo> todoList)
    {
        await _daprClient.SaveStateAsync("todos", "todoList", todoList);
    }

    /// <summary>
    /// Gets the todo item with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to retrieve.</param>
    /// <returns>The todo item with the specified ID.</returns>
    public async Task<Todo> GetTodo(int id)
    {
        var todoList = await GetList();
        var todo = GetTodo(todoList, id);
        return todo!;
    }

    private Todo GetTodo(List<Todo> todoList, int id)
    {
        var todo = todoList.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            throw new TodoNotFoundException();
        }

        return todo!;
    }

    /// <summary>
    /// Adds the specified todo item to the list and saves it to the state store.
    /// </summary>
    /// <param name="todo">The todo item to add.</param>
    /// <returns>The added todo item.</returns>
    public async Task<Todo> Add(Todo todo)
    {
        var todoList = await GetList();

        var id = todoList.Max(t => t.Id) + 1;
        todo.Id = id;

        todoList.Add(todo);

        await SaveList(todoList);

        return todo;
    }

    /// <summary>
    /// Updates the specified todo item in the list and saves it to the state store.
    /// </summary>
    /// <param name="todo">The todo item to update.</param>
    public async Task Update(Todo todo)
    {
        var todoList = await GetList();
        var todoToUpdate = GetTodo(todoList, todo.Id);

        todoToUpdate.Name = todo.Name;
        todoToUpdate.IsComplete = todo.IsComplete;

        await SaveList(todoList);
    }

    /// <summary>
    /// Deletes the todo item with the specified ID from the list and saves it to the state store.
    /// </summary>
    /// <param name="id">The ID of the todo item to delete.</param>
    public async Task Delete(int id)
    {
        var todoList = await GetList();

        var todoToDelete = GetTodo(todoList, id);
        todoList.Remove(todoToDelete);

        await SaveList(todoList);
    }
}