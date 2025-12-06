using Microsoft.Extensions.DependencyInjection;

namespace NotificationService.Extensions;

public class NotificationServiceBuilder
{
    public IServiceCollection Services { get; internal init; }
}