using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Moq;
using Ops.Health;
using Xunit;

public class StartupBackgroundServiceTests
{
    private readonly Mock<IStartupHealthCheck> _healthCheckMock;
    private readonly Mock<ILogger<StartupBackgroundService>> _loggerMock;
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly StartupBackgroundService _startupBackgroundService;

    public StartupBackgroundServiceTests()
    {
        _healthCheckMock = new Mock<IStartupHealthCheck>();
        _loggerMock = new Mock<ILogger<StartupBackgroundService>>();
        _daprClientMock = new Mock<DaprClient>();
        _startupBackgroundService = new StartupBackgroundService(_loggerMock.Object, _healthCheckMock.Object, _daprClientMock.Object);
    }

    [Fact]
    public async Task StartupAsync_WaitsForSidecar()
    {
        // Arrange
        _daprClientMock.Setup(x => x.WaitForSidecarAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _startupBackgroundService.StartupAsync(cancellationTokenSource.Token);

        // Assert
        _daprClientMock.Verify(x => x.WaitForSidecarAsync(cancellationTokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task StartupAsync_GetsTodoListFromStateStore()
    {
        // Arrange
        _daprClientMock.Setup(x => x.WaitForSidecarAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Todo>());
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _startupBackgroundService.StartupAsync(cancellationTokenSource.Token);

        // Assert
        _daprClientMock.Verify(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartupAsync_CreatesNewTodoListIfNotFound()
    {
        // Arrange
        _daprClientMock.Setup(x => x.WaitForSidecarAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<List<Todo>>());
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _startupBackgroundService.StartupAsync(cancellationTokenSource.Token);

        // Assert
        _daprClientMock.Verify(x => x.SaveStateAsync("todos", "todoList", It.IsAny<List<Todo>>(), null, default, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartupAsync_ReportsStartupCompleted()
    {
        // Arrange
        _daprClientMock.Setup(x => x.WaitForSidecarAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _daprClientMock.Setup(x => x.GetStateAsync<List<Todo>>("todos", "todoList", null, default, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Todo>());
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _startupBackgroundService.StartupAsync(cancellationTokenSource.Token);

        // Assert
        _healthCheckMock.VerifySet(x => x.StartupCompleted = true, Times.Once);
    }

    [Fact]
    public async Task StartupAsync_ReportsStartupNotHealthWhenException()
    {
        // Arrange
        _daprClientMock.Setup(x => x.WaitForSidecarAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception());
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _startupBackgroundService.StartupAsync(cancellationTokenSource.Token);

        // Assert
        _healthCheckMock.VerifySet(x => x.StartupHealthy = false, Times.Once);
    }
}