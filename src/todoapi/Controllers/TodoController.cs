
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Controller class for managing todo items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodoController
{
    private readonly ILogger<TodoController> _logger;
    private readonly ITodoService _todos;
    private readonly bool _returnRandomException;
    private readonly bool _delayRandomRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoController"/> class with the specified logger and todo service.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <param name="todos">The todo service instance to use for managing todo items.</param>
    public TodoController(ILogger<TodoController> logger, ITodoService todos)
    {
        _logger = logger;
        _todos = todos;

        _returnRandomException = Environment.GetEnvironmentVariable("RETURN_RANDOM_EXCEPTION") != null;
        _delayRandomRequest = Environment.GetEnvironmentVariable("DELAY_RANDOM_REQUEST") != null;
    }

    /// <summary>
    /// Gets the list of all todo items.
    /// </summary>
    /// <returns>A list of all todo items.</returns>
    [ProducesResponseType(typeof(IEnumerable<Todo>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation($"TodoController.Get() called");
        var result = await _todos.GetListAsync();

        // Throw an exception randomly 10% of requests
        if (_returnRandomException && new Random().Next(10) == 0)
        {
            throw new Exception("Random exception");
        }

        // Sleep for 2 seconds randomly 10% of requests
        if (_delayRandomRequest && new Random().Next(10) == 0)
        {
            Thread.Sleep(2000);
        }

        return new ObjectResult(result);
    }

    /// <summary>
    /// Gets the todo item with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to retrieve.</param>
    /// <returns>The todo item with the specified ID.</returns>
    [ProducesResponseType(typeof(Todo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        _logger.LogInformation($"TodoController.Get({id}) called");
        try
        {
            return new ObjectResult(await _todos.GetTodoAsync(id));
        }
        catch (TodoNotFoundException)
        {
            return new NotFoundResult();
        }
    }

    /// <summary>
    /// Adds a new todo item to the list.
    /// </summary>
    /// <param name="todo">The todo item to add. Name field must contain content.</param>
    /// <returns>The newly added todo item.</returns>
    [ProducesResponseType(typeof(Todo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Todo todo)
    {
        _logger.LogInformation($"TodoController.Post() called with {todo.Name}");
        if (string.IsNullOrWhiteSpace(todo.Name))
        {
            return new BadRequestResult();
        }

        return new ObjectResult(await _todos.AddAsync(todo));
    }

    /// <summary>
    /// Updates the todo item with the specified ID.
    /// </summary>
    /// <param name="todo">The todo item to update. Id field must match id parameter in URL.</param>
    /// <param name="id">The ID of the todo item to update.</param>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromBody] Todo todo, int id)
    {
        _logger.LogInformation("TodoController.Put() called");
        if (id != todo.Id)
        {
            return new BadRequestResult();
        }

        try
        {
            await _todos.UpdateAsync(todo);
        }
        catch (TodoNotFoundException)
        {
            return new NotFoundResult();
        }

        return new OkResult();
    }

    /// <summary>
    /// Deletes the todo item with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the todo item to delete.</param>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation($"TodoController.Delete({id}) called");
        try
        {
            await _todos.DeleteAsync(id);
        }
        catch (TodoNotFoundException)
        {
            return new NotFoundResult();
        }
        return new OkResult();
    }
}