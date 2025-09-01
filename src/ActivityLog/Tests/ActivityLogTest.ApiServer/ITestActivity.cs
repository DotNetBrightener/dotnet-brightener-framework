using ActivityLog.ActionFilters;
using ActivityLog.Services;

namespace ActivityLogTest.ApiServer;

public interface ITestActivity
{
    Task DoSomething(long id);
}

public class TestActivity(ILogger<TestActivity> logger) : ITestActivity
{
    [LogActivity("DoSomething", 
                 descriptionFormat: "Do something with {id}",
                 TargetEntity = "Product")]
    public async Task DoSomething(long id)
    {
        logger.LogInformation("Just do nothing but logging");

        ActivityLogContext.AddMetadata(new Dictionary<string, object?>
        {
            {"Type", "Product"},
            {"ProductName", "Botox"}
        });
        ActivityLogContext.SetTargetEntityId(id);
        ActivityLogContext.SetDescriptionFormat("Doing something with {Metadata.Type} #{TargetEntityId} {Metadata.ProductName}");
    }
}