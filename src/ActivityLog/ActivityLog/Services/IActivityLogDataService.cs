using ActivityLog.Entities;

namespace ActivityLog.Services;

public interface IActivityLogDataService
{
    IQueryable<ActivityLogRecordModel> RetrieveData();
}

public class ActivityLogDataService(IActivityLogReadOnlyRepository repository) : IActivityLogDataService
{
    public IQueryable<ActivityLogRecordModel> RetrieveData()
    {
        return repository.Fetch<ActivityLogRecord>()
                         .Select(ActivityLogRecordModel.FromEntity);
    }
}