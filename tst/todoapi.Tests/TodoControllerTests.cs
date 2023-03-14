using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class TodoControllerTests
{
    private readonly Mock<ILogger<TodoController>> _loggerMock = new Mock<ILogger<TodoController>>();
    private readonly Mock<ITodoService> _todosMock = new Mock<ITodoService>();

    private readonly TodoController _todoController;

    public TodoControllerTests()
    {
        _todoController = new TodoController(_loggerMock.Object, _todosMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsListOfAllTodoItems()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        _todosMock.Setup(x => x.GetListAsync())
                  .ReturnsAsync(todoList);

        // Act
        var result = await _todoController.Get();

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Todo>>(okResult.Value);
        Assert.Equal(todoList.Count, model.Count());
        Assert.Equal(todoList, model);
    }

    [Fact]
    public async Task Get_Returns404NotFoundWhenTodoNotFoundExceptionThrown()
    {
        // Arrange
        _todosMock.Setup(x => x.GetTodoAsync(It.IsAny<int>()))
                  .ThrowsAsync(new TodoNotFoundException());

        // Act
        var result = await _todoController.Get(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_AddsNewTodoItemToListAndReturnsAddedTodoItem()
    {
        // Arrange
        var todo = new Todo { Id = 5, Name = "Todo 1", IsComplete = false };

        _todosMock.Setup(x => x.AddAsync(It.Is<Todo>(t => t.Name == todo.Name)))
                  .ReturnsAsync(todo);

        // Act
        var result = await _todoController.Post(todo);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        var model = Assert.IsAssignableFrom<Todo>(okResult.Value);
        Assert.Equal(todo, model);
        _todosMock.Verify(x => x.AddAsync(It.Is<Todo>(t => t.Name == todo.Name)), Times.Once);
    }

    [Fact]
    public async Task Post_Returns400BadRequestWhenTodoNameIsEmpty()
    {
        // Arrange
        var todo = new Todo { Id = 1, Name = string.Empty, IsComplete = false };

        // Act
        var result = await _todoController.Post(todo);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Put_UpdatesTodoItemWithSpecifiedId()
    {
        // Arrange
        var todo = new Todo { Id = 1, Name = "Todo 1", IsComplete = false };

        _todosMock.Setup(x => x.UpdateAsync(It.IsAny<Todo>()));

        // Act
        var result = await _todoController.Put(todo, todo.Id);

        // Assert
        Assert.IsType<OkResult>(result);
        _todosMock.Verify(x => x.UpdateAsync(It.Is<Todo>(t => t.Id == todo.Id)), Times.Once);
    }

    [Fact]
    public async Task Put_Returns400BadRequestWhenTodoIdDoesNotMatchIdInRoute()
    {
        // Arrange
        var todo = new Todo { Id = 1, Name = "Todo 1", IsComplete = false };

        // Act
        var result = await _todoController.Put(todo, 2);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Put_Returns404NotFoundWhenTodoNotFoundExceptionThrown()
    {
        // Arrange
        var todo = new Todo { Id = 1, Name = "Todo 1", IsComplete = false };

        _todosMock.Setup(x => x.UpdateAsync(It.Is<Todo>(t => t.Id == todo.Id)))
                  .ThrowsAsync(new TodoNotFoundException());

        // Act
        var result = await _todoController.Put(todo, todo.Id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_DeletesTodoItemWithSpecifiedId()
    {
        // Arrange
        var todo = new Todo { Id = 1, Name = "Todo 1", IsComplete = false };

        _todosMock.Setup(x => x.DeleteAsync(It.IsAny<int>()));

        // Act
        var result = await _todoController.Delete(todo.Id);

        // Assert
        Assert.IsType<OkResult>(result);

        _todosMock.Verify(x => x.DeleteAsync(It.Is<int>(id => id == todo.Id)), Times.Once);
    }

    [Fact]
    public async Task Delete_Returns404NotFoundWhenTodoNotFoundExceptionThrown()
    {
        // Arrange
        _todosMock.Setup(x => x.DeleteAsync(It.IsAny<int>()))
                  .ThrowsAsync(new TodoNotFoundException());

        // Act
        var result = await _todoController.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }


}
