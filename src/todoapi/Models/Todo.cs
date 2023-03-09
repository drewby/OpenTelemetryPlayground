/// <summary>
/// A simple Todo item.
/// </summary>
public class Todo {
    /// <summary>
    /// The unique identifier for the Todo item.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The name/description of the Todo item.
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// Whether the Todo item is complete.
    /// </summary>
    public bool IsComplete { get; set; }
}