// See https://aka.ms/new-console-template for more information

using DotNetBrightener.Core.BackgroundTasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IServiceCollection serviceCollection = new ServiceCollection();
IConfiguration configuration = new ConfigurationBuilder().Build();

serviceCollection.EnableBackgroundTaskServices(configuration);

var methodInfo = typeof(BackgroundTask).GetMethod(nameof(BackgroundTask.Run));

serviceCollection.AddLogging((logging) =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddConsole();
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var backgroundScheduler = serviceProvider.GetRequiredService<IBackgroundTaskScheduler>();

while (true)
{
    Console.WriteLine("Press enter to add task, or Ctrl + C to exit");
    Console.ReadLine();

    backgroundScheduler.EnqueueTask(methodInfo);
    Console.WriteLine("Task scheduled");
    Thread.Sleep(TimeSpan.FromSeconds(2));
}


public class BackgroundTask
{
    public void Run()
    {
        Console.WriteLine("BackgroundTask.Run()");
    }
}