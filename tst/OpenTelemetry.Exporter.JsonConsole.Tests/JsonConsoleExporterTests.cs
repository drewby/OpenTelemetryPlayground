using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace OpenTelemetry.Exporter.JsonConsole.Tests;

public class JsonConsoleExporterTests
{

    [Fact]
    public void Export_EmptyBatch_ShouldReturnSuccess()
    {
        // Arrange
        var exporter = new JsonConsoleLogRecordExporter();
        var batch = new Batch<LogRecord>();

        // Act
        var result = exporter.Export(batch);

        // Assert
        Assert.Equal(ExportResult.Success, result);
    }

    [Fact]
    public void Export_OneLog_ShouldSerializeOneLogRecordToConsole()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, "Test Message");

        // Assert
        mockWriteFunction.Verify(x => x(It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public void Export_TwoLogs_ShouldSerializeTwoLogRecordsToConsole()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, "Test Message");
        logger.Log(LogLevel.Information, "Test Message 2");

        // Assert
        mockWriteFunction.Verify(x => x(It.IsAny<string>()), Times.Exactly(2));
    }

    public void Export_OneLog_ShouldSerializeToCorrectJson()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, "Test Message");

        // Assert
        var consoleText = mockWriteFunction.Invocations[0].Arguments[0].ToString();
        var json = JsonSerializer.Deserialize<JsonElement>(consoleText!);
        Assert.NotNull(json.GetProperty("Timestamp").GetString());
        Assert.Equal("Information", json.GetProperty("LogLevel").GetString());
        Assert.Equal("Test Message", json.GetProperty("Message").GetString());
    }

    [Fact]
    public void Export_OneLogWithTrace_ShouldHaveTraceIdAndSpanId()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        var activity = new System.Diagnostics.Activity("TestActivity");
        activity.Start();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, "Test Message");

        // Assert
        var consoleText = mockWriteFunction.Invocations[0].Arguments[0].ToString();
        var json = JsonSerializer.Deserialize<JsonElement>(consoleText!);
        Assert.NotNull(json.GetProperty("TraceId").GetString());
        Assert.NotNull(json.GetProperty("SpanId").GetString());
    }

    [Fact]
    public void Export_LogWithStateObject_ShouldHaveStateInJson()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, 0, new { Test = "Test" }, null, (s, e) => s.ToString());

        // Assert
        var consoleText = mockWriteFunction.Invocations[0].Arguments[0].ToString();
        var json = JsonSerializer.Deserialize<JsonElement>(consoleText!);
        Assert.Equal("Test", json.GetProperty("State").GetProperty("Test").GetString());
    }

    [Fact]
    public void Export_LogWithStateKeyValuePairs_ShouldHaveKeyValuesInJson()
    {
        // Arrange
        var mockWriteFunction = new Mock<Action<string>>();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.AddJsonConsoleExporter(mockWriteFunction.Object);
            });
        });
        var logger = loggerFactory.CreateLogger("TestLogger");

        // Act
        logger.Log(LogLevel.Information, 0, new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("Test", "Test") }, null, (s, e) => s.ToString());

        // Assert
        var consoleText = mockWriteFunction.Invocations[0].Arguments[0].ToString();
        var json = JsonSerializer.Deserialize<JsonElement>(consoleText!);
        Assert.Equal("Test", json.GetProperty("Test").GetString());
    }
}