using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Dapr.Client;

namespace OpenTelemetry.Exporter.JsonConsole.Tests;

public class TodoServiceTests
{
    private readonly Mock<ILogger<TodoService>> _loggerMock = new Mock<ILogger<TodoService>>();
    private readonly Mock<DaprClient> _daprClientMock = new Mock<DaprClient>();

    private readonly TodoService _todoService;

    public TodoServiceTests()
    {
        _todoService = new TodoService(_loggerMock.Object, _daprClientMock.Object);
    }

    [Fact]
    public async Task GetList_ReturnsAllTodoItems()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };


        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        // Act
        var result = await _todoService.GetListAsync();

        // Assert
        Assert.Equal(todoList, result);
    }

    [Fact]
    public async Task GetList_ThrowsExceptionWhenTodoListIsNull()
    {
        // Arrange
        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(It.IsAny<List<Todo>>());

        // Act and assert
        await Assert.ThrowsAsync<Exception>(() => _todoService.GetListAsync());
    }

    [Fact]
    public async Task GetTodo_ReturnsTodoItemWithSpecifiedId()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        // Act
        var result = await _todoService.GetTodoAsync(2);

        // Assert
        Assert.Equal(todoList[1], result);
    }

    [Fact]
    public async Task GetTodo_ThrowsTodoNotFoundExceptionWhenTodoItemWithSpecifiedIdDoesNotExist()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        // Act and assert
        await Assert.ThrowsAsync<TodoNotFoundException>(() => _todoService.GetTodoAsync(4));
    }

    [Fact]
    public async Task Add_AddsTodoItemToListAndReturnsAddedTodoItem()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        var todo = new Todo { Id = 4, Name = "Todo 4", IsComplete = false };

        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        _daprClientMock.Setup(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        // Act
        var result = await _todoService.AddAsync(todo);

        // Assert
        Assert.Equal(todo, result);

        _daprClientMock.Verify(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_UpdatesTodoItemInListAndSavesItToStateStore()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        var todo = new Todo { Id = 2, Name = "Todo 2", IsComplete = false };

        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        _daprClientMock.Setup(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        await _todoService.UpdateAsync(todo);

        // Assert
        // Ensure that the values of todoList[1] are updated
        Assert.Equal(todo.Id, todoList[1].Id);
        Assert.Equal(todo.Name, todoList[1].Name);
        Assert.Equal(todo.IsComplete, todoList[1].IsComplete);

        _daprClientMock.Verify(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeletesTodoItemFromListAndSavesItToStateStore()
    {
        // Arrange
        var todoList = new List<Todo>
        {
            new Todo { Id = 1, Name = "Todo 1", IsComplete = false },
            new Todo { Id = 2, Name = "Todo 2", IsComplete = true },
            new Todo { Id = 3, Name = "Todo 3", IsComplete = false }
        };

        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(todoList);

        _daprClientMock.Setup(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _todoService.DeleteAsync(2);

        // Assert
        Assert.Equal(2, todoList.Count);
        Assert.Equal(1, todoList[0].Id);
        Assert.Equal(3, todoList[1].Id);

        _daprClientMock.Verify(x => x.SaveStateAsync("todos", "todoList", todoList, null, default, It.IsAny<CancellationToken>()), Times.Once);
    }

}