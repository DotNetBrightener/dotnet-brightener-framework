using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace DotNetBrightener.Plugins.EventPubSub.Tests;

public class TestEventMessage : IEventMessage;

public class ConcreteEventHandlerTests(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task EventHandler_Should_Be_Called()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEventPubSubService();

        var mockHandler = new Mock<IEventHandler<TestEventMessage>>();
        mockHandler.Setup(x => x.HandleEvent(It.IsAny<TestEventMessage>())).ReturnsAsync(true);

        serviceCollection.AddSingleton<IEventHandler>(mockHandler.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();


        using (var scope = serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            eventPublisher.Should().BeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);

            // Verify that the HandleEvent method was called once
            mockHandler.Verify(x => x.HandleEvent(message), Times.Once);
        }
    }

    [Fact]
    public async Task MultiEventHandlers_Should_Be_CalledInOrder()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEventPubSubService();

        var eventHandlers = new List<Mock<IEventHandler<TestEventMessage>>>();
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

            eventPublisher.Should().BeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);
            
            executionOrder.Should().BeInDescendingOrder();

            // Verify that each handler was called exactly once
            foreach (var handler in eventHandlers)
            {
                handler.Verify(x => x.HandleEvent(message), Times.Once);
            }
        }
    }

    [Fact]
    public async Task MultiEventHandlers_ShouldShortCircuit_When_A_Handler_Returns_False()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEventPubSubService();

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

            eventPublisher.Should().BeOfType<DefaultEventPublisher>();

            var message = new TestEventMessage();
            await eventPublisher.Publish(message);

            executionOrder.Should()
                          .BeInDescendingOrder();

            executionOrder.Count.Should().Be(3);

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