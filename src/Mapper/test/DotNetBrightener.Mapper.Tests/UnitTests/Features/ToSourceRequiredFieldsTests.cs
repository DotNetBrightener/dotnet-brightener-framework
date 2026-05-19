using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Features;

public class ToSourceRequiredFieldsTests
{
    [Fact]
    public void ToSource_ShouldWork_WithExcludedRequiredFields()
    {
        // Arrange
        var eventLog = new EventLog
        {
            Id = "test-event",
            EventType = "TestEvent",
            Timestamp = DateTime.UtcNow,
            Message = "Test message",
            UserId = "user123",
            Source = "TestSource" // This required field will be excluded from the DTO
        };

        var target = eventLog.ToTarget<EventLog, EventLogDto>();

        // Act
        var result = target.ToSource<EventLog>();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe("test-event");
        result.EventType.ShouldBe("TestEvent");
        result.Timestamp.ShouldBe(eventLog.Timestamp);
        result.Message.ShouldBe("Test message");
        result.UserId.ShouldBe("user123");
        result.Source.ShouldBe(string.Empty);
    }

    [Fact]
    public void ToSource_ShouldProvideDefaultValues_ForExcludedRequiredFields()
    {
        // Arrange
        var originalEventLog = new EventLog
        {
            Id = "event-123",
            EventType = "UserLogin",
            Timestamp = DateTime.UtcNow,
            Message = "User logged in successfully",
            UserId = "user-456",
            Source = "WebApp" // This required field will be excluded in the DTO
        };

        var eventLogDto = originalEventLog.ToTarget<EventLog, EventLogDto>();

        // Act
        var mappedEventLog = eventLogDto.ToSource<EventLog>();

        // Assert
        mappedEventLog.ShouldNotBeNull();
        mappedEventLog.Id.ShouldBe("event-123");
        mappedEventLog.EventType.ShouldBe("UserLogin");
        mappedEventLog.Timestamp.ShouldBe(originalEventLog.Timestamp);
        mappedEventLog.Message.ShouldBe("User logged in successfully");
        mappedEventLog.UserId.ShouldBe("user-456");

        mappedEventLog.Source.ShouldBe(string.Empty); // String default value
    }
}
