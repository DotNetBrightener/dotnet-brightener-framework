using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System.Reflection;
using Xunit.Abstractions;

namespace DotNetBrightener.Plugins.EventPubSub.Tests;

public class TestEventMessage : IEventMessage;
public class TestData ;

public class GenericMessage<TSomething> : IEventMessage
    where TSomething : class, new()
{
    public TSomething Something { get; init; }
}

public class GenericMessage2<TSomething> : IEventMessage
    where TSomething : class, new()
{
    public TSomething Something { get; init; }
}

public class GenericEventHandler<TSomething>(IMockDataContainer mockDataContainer) : IEventHandler<GenericMessage<TSomething>>
    where TSomething : class, new()
{
    public virtual async Task<bool> HandleEvent(GenericMessage<TSomething> eventMessage)
    {
        mockDataContainer.ShouldGetCalledInEventHandler(eventMessage.Something);

        return true;
    }

    public int Priority => 1000;
}

public class ConcreteHandlerOfAGenericMessage(IServiceProvider serviceProvider) : IEventHandler<GenericMessage<TestData>>
{
    private readonly IMockDataContainer3? _mockDataContainer = serviceProvider.GetService<IMockDataContainer3>();

    public async Task<bool> HandleEvent(GenericMessage<TestData> eventMessage)
    {
        _mockDataContainer?.ShouldGetCalledInEventHandler(eventMessage.Something);

        return true;
    }

    public int Priority => 1000;
}

public class GenericEventHandler2<TSomething>(IMockDataContainer2 mockDataContainer) : IEventHandler<GenericMessage2<TSomething>>
    where TSomething : class, new()
{
    public virtual async Task<bool> HandleEvent(GenericMessage2<TSomething> eventMessage)
    {
        mockDataContainer.ShouldGetCalledInEventHandler(eventMessage.Something);

        return true;
    }

    public int Priority => 1000;
}

public interface IMockDataContainer
{
    string ShouldGetCalledInEventHandler<TData>(TData someData);
}

public interface IMockDataContainer2
{
    string ShouldGetCalledInEventHandler<TData>(TData someData);
}


public interface IMockDataContainer3
{
    string ShouldGetCalledInEventHandler<TData>(TData someData);
}

public class ConcreteEventHandlerTests
{
    private readonly IServiceCollection serviceCollection;

    public ConcreteEventHandlerTests(ITestOutputHelper testOutput)
    {
        serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(serviceCollection);
        serviceCollection.AddLogging();
        serviceCollection.AddEventPubSubService();
        serviceCollection.AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());
        serviceCollection.AddEventMessagesFromAssemblies(Assembly.GetExecutingAssembly());
    }

    [Fact]
    public async Task GenericEventHandler_Should_Be_Called()
    {
        var mockHandler = new Mock<IMockDataContainer>();
        mockHandler.Setup(x => x.ShouldGetCalledInEventHandler(It.IsAny<object>())).Returns("called");

        var mockHandler2 = new Mock<IMockDataContainer2>();
        mockHandler2.Setup(x => x.ShouldGetCalledInEventHandler(It.IsAny<object>())).Returns("called");
        
        var mockHandler3 = new Mock<IMockDataContainer3>();
        mockHandler3.Setup(x => x.ShouldGetCalledInEventHandler(It.IsAny<object>())).Returns("called");

        serviceCollection.AddSingleton<IMockDataContainer>(mockHandler.Object);
        serviceCollection.AddSingleton<IMockDataContainer2>(mockHandler2.Object);
        serviceCollection.AddSingleton<IMockDataContainer3>(mockHandler3.Object);


        var serviceProvider      = serviceCollection.BuildServiceProvider();
        var testEventMessageData = new TestEventMessage();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var message = new GenericMessage<TestEventMessage>
            {
                Something = testEventMessageData
            };

            await eventPublisher.Publish(message);

            // verify that the irrelevant handler still be detected
            var genericEventHandlersContainer = scope.ServiceProvider.GetService<GenericEventHandlersContainer>();

            genericEventHandlersContainer.ShouldNotBeNull();

            genericEventHandlersContainer!.GenericEventHandlerTypes.ShouldContain(typeof(GenericEventHandler2<>));

            // Verify that the HandleEvent method was called once
            mockHandler.Verify(x => x.ShouldGetCalledInEventHandler(testEventMessageData),
                               Times.Exactly(1),
                               "Relevant handler should be called");
            
            // irrelevant handler should not be called
            mockHandler2.Verify(x => x.ShouldGetCalledInEventHandler(It.IsAny<GenericMessage2<TestEventMessage>>()),
                                Times.Never,
                                "Irrelevant handler should not be called");
        }
        
        mockHandler.Invocations.Clear();
        mockHandler2.Invocations.Clear();
        mockHandler3.Invocations.Clear();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var message = new GenericMessage2<TestEventMessage>
            {
                Something = testEventMessageData
            };

            await eventPublisher.Publish(message);

            // irrelevant HandleEvent method should not be called
            mockHandler.Verify(x => x.ShouldGetCalledInEventHandler(testEventMessageData),
                               Times.Never,
                               "Irrelevant handler should not be called");

            //  handler should be called
            mockHandler2.Verify(x => x.ShouldGetCalledInEventHandler(testEventMessageData),
                                Times.Once,
                                "Relevant handler should be called");
        }

        mockHandler.Invocations.Clear();
        mockHandler2.Invocations.Clear();
        mockHandler3.Invocations.Clear();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var testData = new TestData();
            var message = new GenericMessage<TestData>
            {
                Something = testData
            };

            await eventPublisher.Publish(message);

            // irrelevant HandleEvent method should not be called
            mockHandler2.Verify(x => x.ShouldGetCalledInEventHandler(testData),
                               Times.Never,
                               "Irrelevant handler should not be called");

            //  handler should be called
            mockHandler.Verify(x => x.ShouldGetCalledInEventHandler(testData),
                               Times.Once,
                               "Relevant handler should be called");

            mockHandler3.Verify(x => x.ShouldGetCalledInEventHandler(testData),
                                Times.Once,
                                "Relevant handler should be called");
        }
    }

    [Fact]
    public async Task EventHandler_Should_Be_Called()
    {
        var mockHandler = new Mock<IEventHandler<TestEventMessage>>();
        mockHandler.Setup(x => x.HandleEvent(It.IsAny<TestEventMessage>())).ReturnsAsync(true);

        serviceCollection.AddSingleton<IEventHandler>(mockHandler.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);

            // Verify that the HandleEvent method was called once
            mockHandler.Verify(x => x.HandleEvent(message), Times.Once);
        }
    }

    [Fact]
    public async Task MultiEventHandlers_Should_Be_CalledInOrder()
    {
        var eventHandlers  = new List<Mock<IEventHandler<TestEventMessage>>>();
        var executionOrder = new List<int>();


        for (var i = 0; i < 5; i++)
        {
            Mock<IEventHandler<TestEventMessage>> mockHandler = new();
            var                                   priority    = Random.Shared.Next(1000);
            mockHandler.Setup(x => x.Priority).Returns(priority);
            mockHandler.Setup(x => x.HandleEvent(It.IsAny<TestEventMessage>()))
                       .Callback(() => executionOrder.Add(priority))
                       .ReturnsAsync(true);

            serviceCollection.AddSingleton<IEventHandler>(mockHandler.Object);

            eventHandlers.Add(mockHandler);
        }

        var serviceProvider = serviceCollection.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);

            // Verify that each handler was called exactly once
            foreach (var handler in eventHandlers)
            {
                handler.Verify(x => x.HandleEvent(message), Times.Once);
            }

            executionOrder.ShouldBeInOrder(SortDirection.Descending);
        }
    }

    [Fact]
    public async Task MultiEventHandlers_ShouldShortCircuit_When_A_Handler_Returns_False()
    {
        var eventHandlers  = new List<Mock<IEventHandler<TestEventMessage>>>();
        var executionOrder = new List<int>();

        for (var i = 0; i < 5; i++)
        {
            Mock<IEventHandler<TestEventMessage>> mockHandler = new();
            var                                   priority    = 50 - i * 10;

            mockHandler.Setup(x => x.Priority).Returns(priority);
            mockHandler.Setup(x => x.HandleEvent(It.IsAny<TestEventMessage>()))
                       .Callback(() => executionOrder.Add(priority))

                       .ReturnsAsync((eventHandlers.Count < 2));

            serviceCollection.AddSingleton<IEventHandler>(mockHandler.Object);

            eventHandlers.Add(mockHandler);
        }

        var serviceProvider = serviceCollection.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.ShouldBeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);

            executionOrder.ShouldBeInOrder(SortDirection.Descending);

            executionOrder.Count.ShouldBe(3);

            // Verify that the HandleEvent method was called once for each handler and short-circuited after the third handler
            for (var i = 0; i < eventHandlers.Count; i++)
            {
                if (i < 3)
                {
                    eventHandlers[i].Verify(x => x.HandleEvent(message), Times.Once);
                }
                else
                {
                    eventHandlers[i].Verify(x => x.HandleEvent(message), Times.Never);
                }
            }
        }
    }
}