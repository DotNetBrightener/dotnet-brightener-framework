using ActivityLog.ActionFilters;
using ActivityLog.Configuration;
using ActivityLog.Interceptors;
using ActivityLog.Models;
using ActivityLog.Services;
using Castle.DynamicProxy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;
using Xunit;

namespace ActivityLog.Tests.Interceptors;

public class ActivityLogInterceptorTests
{
    private readonly Mock<IActivityLogService>             _mockActivityLogService;
    private readonly Mock<IActivityLogContextProvider>     _mockContextProvider;
    private readonly Mock<IActivityLogSerializer>          _mockSerializer;
    private readonly Mock<ILogger<ActivityLogInterceptor>> _mockLogger;
    private readonly ActivityLogConfiguration              _configuration;
    private readonly ActivityLogInterceptor                _interceptor;
    private readonly ProxyGenerator                        _proxyGenerator;

    public ActivityLogInterceptorTests()
    {
        _mockActivityLogService = new Mock<IActivityLogService>();
        _mockContextProvider    = new Mock<IActivityLogContextProvider>();
        _mockSerializer         = new Mock<IActivityLogSerializer>();
        _mockLogger             = new Mock<ILogger<ActivityLogInterceptor>>();

        _configuration = new ActivityLogConfiguration
        {
            IsEnabled       = true,
            MinimumLogLevel = ActivityLogLevel.Information
        };

        _interceptor = new ActivityLogInterceptor(
                                                  _mockActivityLogService.Object,
                                                  _mockContextProvider.Object,
                                                  _mockSerializer.Object,
                                                  Options.Create(_configuration),
                                                  _mockLogger.Object);

        _proxyGenerator = new ProxyGenerator();
    }

    [Fact]
    public void Intercept_ShouldNotLog_WhenLoggingDisabled()
    {
        // Arrange
        _configuration.IsEnabled = false;
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()),
                                       Times.Never);
    }

    [Fact]
    public void Intercept_ShouldNotLog_WhenMethodHasNoLogActivityAttribute()
    {
        // Arrange
        var testService = new TestServiceWithoutAttribute();
        var proxy =
            _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithoutAttribute>(testService, _interceptor);

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()),
                                       Times.Never);
    }

    [Fact]
    public void Intercept_ShouldLogMethodExecution_WhenMethodHasLogActivityAttribute()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .ReturnsAsync(LoggingResult.Success(TimeSpan.FromMilliseconds(1)));

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_ShouldLogAsyncMethodExecution_WhenMethodIsAsync()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .ReturnsAsync(LoggingResult.Success(TimeSpan.FromMilliseconds(1)));

        // Act
        var result = await proxy.GetValueAsync(42);

        // Assert
        result.Should().Be("Async Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public void Intercept_ShouldLogException_WhenMethodThrowsException()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()))
                               .ReturnsAsync(LoggingResult.Success(TimeSpan.FromMilliseconds(1)));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => proxy.ThrowException());
        exception.Message.Should().Be("Test exception");

        _mockActivityLogService.Verify(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_ShouldLogAsyncException_WhenAsyncMethodThrowsException()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()))
                               .ReturnsAsync(LoggingResult.Success(TimeSpan.FromMilliseconds(1)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.ThrowExceptionAsync());
        exception.Message.Should().Be("Test async exception");

        _mockActivityLogService.Verify(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public void Intercept_ShouldCaptureMethodContext_WhenLogging()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .ReturnsAsync(LoggingResult.Success(TimeSpan.FromMilliseconds(1)));

        _mockContextProvider.Setup(x => x.GetCorrelationId()).Returns(Guid.NewGuid());

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        capturedContext.Should().NotBeNull();
        capturedContext!.MethodInfo.Name.Should().Be("GetValue");
        capturedContext.Arguments.Should().HaveCount(1);
        capturedContext.Arguments[0].Should().Be(42);
        capturedContext.ReturnValue.Should().Be("Value: 42");
        capturedContext.ActivityName.Should().Be("GetValue");
    }

    [Fact]
    public void Intercept_ShouldFilterMethod_WhenMethodIsInExcludedList()
    {
        // Arrange
        _configuration.Filtering.ExcludedMethods.Add("GetValue");
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()),
                                       Times.Never);
    }

    [Fact]
    public void Intercept_ShouldFilterNamespace_WhenNamespaceIsExcluded()
    {
        // Arrange
        _configuration.Filtering.ExcludedNamespaces.Add("ActivityLogRecord.Tests");
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        // Act
        var result = proxy.GetValue(42);

        // Assert
        result.Should().Be("Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()),
                                       Times.Never);
    }
}

// Test interfaces and implementations
public interface ITestService
{
    string GetValue(int input);
    Task<string> GetValueAsync(int input);
    void ThrowException();
    Task ThrowExceptionAsync();
}

public interface ITestServiceWithoutAttribute
{
    string GetValue(int input);
}

public class TestService : ITestService
{
    [LogActivity("GetValue", "Getting value for input: {0}")]
    public string GetValue(int input)
    {
        return $"Value: {input}";
    }

    [LogActivity("GetValueAsync", "Getting async value for input: {0}")]
    public async Task<string> GetValueAsync(int input)
    {
        await Task.Delay(10);
        return $"Async Value: {input}";
    }

    [LogActivity("ThrowException", "This method will throw an exception")]
    public void ThrowException()
    {
        throw new InvalidOperationException("Test exception");
    }

    [LogActivity("ThrowExceptionAsync", "This async method will throw an exception")]
    public async Task ThrowExceptionAsync()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("Test async exception");
    }
}

public class TestServiceWithoutAttribute : ITestServiceWithoutAttribute
{
    public string GetValue(int input)
    {
        return $"Value: {input}";
    }
}
