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
            MinimumLogLevel = ActivityLogLevel.Information,
            Filtering = new FilteringConfiguration
            {
                ExcludedNamespaces = [], // Clear default excluded namespaces for tests
                ExcludedMethods    = [], // Clear default excluded methods for tests
                UseWhitelistMode   = false
            }
        };

        // Initialize the static ActivityLogContext accessor for tests
        var contextAccessor = new ActivityLogContextAccessor(_mockSerializer.Object);
        ActivityLogContext.SetAccessor(contextAccessor);

        _interceptor = new ActivityLogInterceptor(
                                                  _mockActivityLogService.Object,
                                                  _mockContextProvider.Object,
                                                  Options.Create(_configuration));

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
    public async Task Intercept_ShouldLogMethodExecution_WhenMethodHasLogActivityAttribute()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Returns(Task.CompletedTask);

        // Act
        var result = proxy.GetValue(42);

        // Wait a bit for the async logging to complete (fire-and-forget Task.Run)
        await Task.Delay(100);

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
                               .Returns(Task.CompletedTask);

        // Act
        var result = await proxy.GetValueAsync(42);

        // Assert
        result.Should().Be("Async Value: 42");
        _mockActivityLogService.Verify(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_ShouldLogException_WhenMethodThrowsException()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()))
                               .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => proxy.ThrowException());
        exception.Message.Should().Be("Test exception");

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        _mockActivityLogService.Verify(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_ShouldLogAsyncException_WhenAsyncMethodThrowsException()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        _mockActivityLogService.Setup(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()))
                               .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.ThrowExceptionAsync());
        exception.Message.Should().Be("Test async exception");

        _mockActivityLogService.Verify(x => x.LogMethodFailureAsync(It.IsAny<MethodExecutionContext>()), Times.Once);
    }

    [Fact]
    public async Task Intercept_ShouldCaptureMethodContext_WhenLogging()
    {
        // Arrange
        var testService = new TestService();
        var proxy       = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestService>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .Returns(Task.CompletedTask);

        _mockContextProvider.Setup(x => x.GetCorrelationId()).Returns(Guid.NewGuid());

        // Act
        var result = proxy.GetValue(42);

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        // Assert
        result.Should().Be("Value: 42");
        capturedContext.Should().NotBeNull();
        capturedContext!.MethodInfo.Name.Should().Be("GetValue");

        // Verify named arguments structure
        capturedContext.Arguments.Should().BeOfType<Dictionary<string, object?>>();
        var namedArguments = (Dictionary<string, object?>)capturedContext.Arguments;
        namedArguments.Should().HaveCount(1);
        namedArguments.Should().ContainKey("input");
        namedArguments["input"].Should().Be(42);

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
        _configuration.Filtering.ExcludedNamespaces.Add("ActivityLog.Tests.Interceptors");
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
    public async Task Intercept_ShouldCaptureMetadataAddedDuringExecution_WhenMethodAddsMetadata()
    {
        // Arrange
        var testService = new TestServiceWithMetadata();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithMetadata>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .Returns(Task.CompletedTask);

        // Act
        var result = proxy.ProcessWithMetadata(42);

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        // Assert
        result.Should().Be("Processed: 42");
        capturedContext.Should().NotBeNull();

        // Verify original arguments are captured
        capturedContext!.Arguments.Should().BeOfType<Dictionary<string, object?>>();
        var namedArguments = (Dictionary<string, object?>)capturedContext.Arguments;
        namedArguments.Should().ContainKey("value");
        namedArguments["value"].Should().Be(42);

        // Verify metadata added during execution is captured
        capturedContext.Metadata.Should().ContainKey("processingTime");
        capturedContext.Metadata.Should().ContainKey("itemCount");
        capturedContext.Metadata.Should().ContainKey("status");
        capturedContext.Metadata["itemCount"].Should().Be(5);
        capturedContext.Metadata["status"].Should().Be("completed");
    }

    [Fact]
    public async Task Intercept_ShouldIsolateContexts_WhenMultipleMethodsExecuteConcurrently()
    {
        // Arrange
        var testService = new TestServiceWithMetadata();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithMetadata>(testService, _interceptor);

        var capturedContexts = new List<MethodExecutionContext>();
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx =>
                               {
                                   lock (capturedContexts)
                                   {
                                       capturedContexts.Add(ctx);
                                   }
                               })
                               .Returns(Task.CompletedTask);

        // Act - Execute multiple methods concurrently
        var tasks = new[]
        {
            Task.Run(() => proxy.ProcessWithMetadata(100)),
            Task.Run(() => proxy.ProcessWithMetadata(200)),
            Task.Run(() => proxy.ProcessWithMetadata(300))
        };

        await Task.WhenAll(tasks);

        // Wait for async logging to complete
        await Task.Delay(200);

        // Assert
        capturedContexts.Should().HaveCount(3);

        // Each context should have its own isolated metadata
        foreach (var context in capturedContexts)
        {
            context.Metadata.Should().ContainKey("processingTime");
            context.Metadata.Should().ContainKey("itemCount");
            context.Metadata.Should().ContainKey("valueReturned");
            context.Metadata.Should().ContainKey("status");
            context.Metadata["itemCount"].Should().Be(5);
            context.Metadata["status"].Should().Be("completed");
        }

        // Verify arguments are different for each call
        var argumentValues = capturedContexts
            .Select(ctx => ((Dictionary<string, object?>)ctx.Arguments)["value"])
            .OrderBy(v => v)
            .ToArray();

        argumentValues.Should().BeEquivalentTo(new object[] { 100, 200, 300 });
    }

    [Fact]
    public async Task Intercept_ShouldDemonstrateContextFlow_WithinSameExecutionFlow()
    {
        // Arrange
        var testService = new TestServiceWithNestedCalls();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithNestedCalls>(testService, _interceptor);

        var capturedContexts = new List<MethodExecutionContext>();
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx =>
                               {
                                   lock (capturedContexts)
                                   {
                                       capturedContexts.Add(ctx);
                                   }
                               })
                               .Returns(Task.CompletedTask);

        // Act - Execute method that calls other methods (direct calls, not through proxy)
        var result = proxy.OuterMethod(42);

        // Wait for async logging to complete
        await Task.Delay(200);

        // Assert
        result.Should().Be("Outer: Inner: 42");
        capturedContexts.Should().HaveCount(1); // Only OuterMethod is intercepted (InnerMethod is direct call)

        var outerContext = capturedContexts.First();
        outerContext.MethodInfo.Name.Should().Be("OuterMethod");

        // Verify that metadata from both outer and inner methods is captured
        // This demonstrates that context flows properly within the same execution
        outerContext.Metadata.Should().ContainKey("outerStart");
        outerContext.Metadata.Should().ContainKey("outerEnd");
        outerContext.Metadata.Should().ContainKey("innerProcessed"); // Inner metadata flows to outer context
    }

    [Fact]
    public async Task Intercept_ShouldIsolateContexts_BetweenSeparateMethodCalls()
    {
        // Arrange
        var testService = new TestServiceWithNestedCalls();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithNestedCalls>(testService, _interceptor);

        var capturedContexts = new List<MethodExecutionContext>();
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx =>
                               {
                                   lock (capturedContexts)
                                   {
                                       capturedContexts.Add(ctx);
                                   }
                               })
                               .Returns(Task.CompletedTask);

        // Act - Execute separate method calls (each gets its own context)
        var result1 = proxy.InnerMethod(100);
        var result2 = proxy.InnerMethod(200);

        // Wait for async logging to complete
        await Task.Delay(200);

        // Assert
        result1.Should().Be("Inner: 100");
        result2.Should().Be("Inner: 200");
        capturedContexts.Should().HaveCount(2); // Two separate contexts

        // Each context should have its own isolated metadata
        foreach (var context in capturedContexts)
        {
            context.MethodInfo.Name.Should().Be("InnerMethod");
            context.Metadata.Should().ContainKey("innerProcessed");
            context.Metadata.Should().HaveCount(1); // Only its own metadata
        }

        // Verify arguments are different for each call
        var argumentValues = capturedContexts
            .Select(ctx => ((Dictionary<string, object?>)ctx.Arguments)["value"])
            .OrderBy(v => v)
            .ToArray();

        argumentValues.Should().BeEquivalentTo(new object[] { 100, 200 });
    }

    [Fact]
    public async Task Intercept_ShouldCaptureBatchMetadata_WhenMethodAddsBatchMetadata()
    {
        // Arrange
        var testService = new TestServiceWithBatchMetadata();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithBatchMetadata>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .Returns(Task.CompletedTask);

        // Act
        var result = proxy.ProcessWithBatchMetadata(42);

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        // Assert
        result.Should().Be("Batch Processed: 42");
        capturedContext.Should().NotBeNull();

        // Verify original arguments are captured
        capturedContext!.Arguments.Should().BeOfType<Dictionary<string, object?>>();
        var namedArguments = (Dictionary<string, object?>)capturedContext.Arguments;
        namedArguments.Should().ContainKey("value");
        namedArguments["value"].Should().Be(42);

        // Verify single metadata is captured
        capturedContext.Metadata.Should().ContainKey("startTime");

        // Verify batch metadata is captured
        capturedContext.Metadata.Should().ContainKey("itemCount");
        capturedContext.Metadata.Should().ContainKey("totalAmount");
        capturedContext.Metadata.Should().ContainKey("customerType");
        capturedContext.Metadata.Should().ContainKey("processingMode");
        capturedContext.Metadata.Should().ContainKey("endTime");

        // Verify batch metadata values
        capturedContext.Metadata["itemCount"].Should().Be(5);
        capturedContext.Metadata["totalAmount"].Should().Be(99.99m);
        capturedContext.Metadata["customerType"].Should().Be("premium");
        capturedContext.Metadata["processingMode"].Should().Be("batch");
    }

    [Fact]
    public async Task Intercept_ShouldHandleEdgeCases_WhenBatchMetadataHasInvalidEntries()
    {
        // Arrange
        var testService = new TestServiceWithBatchMetadata();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithBatchMetadata>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .Returns(Task.CompletedTask);

        // Act
        var result = proxy.ProcessWithEdgeCases(42);

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        // Assert
        result.Should().Be("Edge Cases Processed: 42");
        capturedContext.Should().NotBeNull();

        // Verify that valid entries are captured and invalid ones are skipped
        capturedContext!.Metadata.Should().ContainKey("validKey1");
        capturedContext.Metadata.Should().ContainKey("validKey2");
        capturedContext.Metadata["validKey1"].Should().Be("validValue1");
        capturedContext.Metadata["validKey2"].Should().Be("validValue2");

        // Verify that null/empty keys are not present
        capturedContext.Metadata.Should().NotContainKey("");
        capturedContext.Metadata.Should().NotContainKey(" ");
        capturedContext.Metadata.Keys.Should().NotContain(k => string.IsNullOrWhiteSpace(k));
    }

    [Fact]
    public async Task Intercept_ShouldAllowContextModification_WhenMethodModifiesContextProperties()
    {
        // Arrange
        var testService = new TestServiceWithContextModification();
        var proxy = _proxyGenerator.CreateInterfaceProxyWithTarget<ITestServiceWithContextModification>(testService, _interceptor);

        MethodExecutionContext? capturedContext = null;
        _mockActivityLogService.Setup(x => x.LogMethodCompletionAsync(It.IsAny<MethodExecutionContext>()))
                               .Callback<MethodExecutionContext>(ctx => capturedContext = ctx)
                               .Returns(Task.CompletedTask);

        // Act
        var result = proxy.ProcessWithContextModification(42);

        // Wait a bit for the async logging to complete
        await Task.Delay(100);

        // Assert
        result.Should().Be("Context Modified: 42");
        capturedContext.Should().NotBeNull();

        // Verify original arguments are captured
        capturedContext!.Arguments.Should().BeOfType<Dictionary<string, object?>>();
        var namedArguments = (Dictionary<string, object?>)capturedContext.Arguments;
        namedArguments.Should().ContainKey("value");
        namedArguments["value"].Should().Be(42);

        // Verify context properties were modified during execution
        capturedContext.ActivityName.Should().Be("ModifiedActivityName");
        capturedContext.ActivityDescription.Should().Be("This activity was modified during execution");
        capturedContext.DescriptionFormat.Should().Be("Modified format: {value}");
        capturedContext.TargetEntity.Should().Be("ModifiedEntity");

        // Verify metadata was also added
        capturedContext.Metadata.Should().ContainKey("modificationTime");
        capturedContext.Metadata.Should().ContainKey("originalName");
        capturedContext.Metadata.Should().ContainKey("modificationCount");
        capturedContext.Metadata["originalName"].Should().Be("ProcessWithContextModification");
        capturedContext.Metadata["modificationCount"].Should().Be(4);
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

public interface ITestServiceWithMetadata
{
    string ProcessWithMetadata(int value);
}

public interface ITestServiceWithNestedCalls
{
    string OuterMethod(int value);
    string InnerMethod(int value);
}

public interface ITestServiceWithBatchMetadata
{
    string ProcessWithBatchMetadata(int value);
    string ProcessWithEdgeCases(int value);
}

public interface ITestServiceWithContextModification
{
    string ProcessWithContextModification(int value);
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

public class TestServiceWithMetadata : ITestServiceWithMetadata
{
    [LogActivity("ProcessWithMetadata", "Processing value: {value}")]
    public string ProcessWithMetadata(int value)
    {
        // Add metadata during method execution
        ActivityLogContext.AddMetadata("processingTime", DateTime.UtcNow);
        ActivityLogContext.AddMetadata("itemCount", 5);
        ActivityLogContext.AddMetadata("status", "completed");
        ActivityLogContext.AddMetadata("valueReturned", value);

        return $"Processed: {value}";
    }
}

public class TestServiceWithNestedCalls : ITestServiceWithNestedCalls
{
    [LogActivity("OuterMethod", "Outer method processing: {value}")]
    public string OuterMethod(int value)
    {
        // Add metadata for outer method
        ActivityLogContext.AddMetadata("outerStart", DateTime.UtcNow);

        // Call inner method (which will have its own context)
        var innerResult = InnerMethod(value);

        // Add more metadata for outer method
        ActivityLogContext.AddMetadata("outerEnd", DateTime.UtcNow);

        return $"Outer: {innerResult}";
    }

    [LogActivity("InnerMethod", "Inner method processing: {value}")]
    public string InnerMethod(int value)
    {
        // Add metadata for inner method (isolated from outer)
        ActivityLogContext.AddMetadata("innerProcessed", DateTime.UtcNow);

        return $"Inner: {value}";
    }
}

public class TestServiceWithBatchMetadata : ITestServiceWithBatchMetadata
{
    [LogActivity("ProcessWithBatchMetadata", "Processing value with batch metadata: {value}")]
    public string ProcessWithBatchMetadata(int value)
    {
        // Add single metadata entry
        ActivityLogContext.AddMetadata("startTime", DateTime.UtcNow);

        // Add batch metadata
        var batchMetadata = new Dictionary<string, object?>
        {
            ["itemCount"] = 5,
            ["totalAmount"] = 99.99m,
            ["customerType"] = "premium",
            ["processingMode"] = "batch"
        };

        ActivityLogContext.AddMetadata(batchMetadata);

        // Add another single metadata entry
        ActivityLogContext.AddMetadata("endTime", DateTime.UtcNow);

        return $"Batch Processed: {value}";
    }

    [LogActivity("ProcessWithEdgeCases", "Processing with edge cases: {value}")]
    public string ProcessWithEdgeCases(int value)
    {
        // Test edge cases with batch metadata
        var edgeCaseMetadata = new Dictionary<string, object?>
        {
            ["validKey1"] = "validValue1",
            [""] = "emptyKey",           // Should be skipped
            [" "] = "whitespaceKey",     // Should be skipped
            ["validKey2"] = "validValue2"
        };

        ActivityLogContext.AddMetadata(edgeCaseMetadata);

        // Test null dictionary (should be handled gracefully)
        ActivityLogContext.AddMetadata((Dictionary<string, object?>)null!);

        // Test empty dictionary (should be handled gracefully)
        ActivityLogContext.AddMetadata(new Dictionary<string, object?>());

        return $"Edge Cases Processed: {value}";
    }
}

public class TestServiceWithContextModification : ITestServiceWithContextModification
{
    [LogActivity("ProcessWithContextModification", "Processing value with context modification: {value}")]
    public string ProcessWithContextModification(int value)
    {
        // Store original activity name in metadata
        var currentContext = ActivityLogContext.GetCurrentContext();
        ActivityLogContext.AddMetadata("originalName", currentContext?.ActivityName ?? "Unknown");

        // Modify context properties during execution
        ActivityLogContext.SetActivityName("ModifiedActivityName");
        ActivityLogContext.SetActivityDescription("This activity was modified during execution");
        ActivityLogContext.SetDescriptionFormat("Modified format: {value}");
        ActivityLogContext.SetTargetEntity("ModifiedEntity");

        // Add metadata about the modifications
        ActivityLogContext.AddMetadata("modificationTime", DateTime.UtcNow);
        ActivityLogContext.AddMetadata("modificationCount", 4);

        return $"Context Modified: {value}";
    }
}
