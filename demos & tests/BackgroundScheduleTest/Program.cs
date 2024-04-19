using DotNetBrightener.Core.BackgroundTasks;

var builder = WebApplication.CreateBuilder(args);


builder.Services
       .EnableBackgroundTaskServices(builder.Configuration);

builder.Services.AddBackgroundTask<TestBackgroundTask>();
builder.Services.AddBackgroundTask<Test2BackgroundTask>();


var methodInfo = typeof(Test3BackgroundTask).GetMethod(nameof(Test3BackgroundTask.Run));

builder.Services.AddLogging((logging) =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddConsole();
});

var app = builder.Build();

var scheduler = app.Services.GetService<IScheduler>();

scheduler.ScheduleTask<TestBackgroundTask>()
         .EveryMinute(); 

app.Run();

public class Test3BackgroundTask
{
    private readonly ILogger _logger;

    public Test3BackgroundTask(ILogger<Test3BackgroundTask> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        _logger.LogInformation("BackgroundTask3.Run(). This should be executed with no dependencies registered");
    }
}

public class TestBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;

    public TestBackgroundTask(ILogger<TestBackgroundTask> logger)
    {
        _logger = logger;
    }

    public async Task Execute()
    {
        _logger.LogInformation("BackgroundTask.Run(), should run once, when scheduled by test2, and daily");

    }
}

public class Test2BackgroundTask : IBackgroundTask
{
    private readonly IScheduler _scheduler;
    private readonly ILogger    _logger;

    public Test2BackgroundTask(IScheduler scheduler, ILogger<Test2BackgroundTask> logger)
    {
        _scheduler = scheduler;
        _logger         = logger;
    }

    public async Task Execute()
    {
        _logger.LogInformation("This should delay 2s");

        await Task.Delay(TimeSpan.FromSeconds(2));

        _logger.LogInformation("BackgroundTask.Run() with {string}, {string2}", "hello world", 2);

        _scheduler.ScheduleTask<TestBackgroundTask>()
                  .Once();
    }
}