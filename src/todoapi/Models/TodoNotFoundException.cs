/// <summary>
/// An exception that is thrown when a Todo item is not found.
/// </summary>
public class TodoNotFoundException : Exception
{
    /// <summary>
    /// Creates a new TodoNotFoundException.
    /// </summary>
    public TodoNotFoundException() : base("The requested todo item was not found.")
    {
    }
}