using DotNetBrightener.DataAccess.Services;

namespace DotNetBrightener.Core.Logging;

public interface IEventLogDataService : IBaseDataService<EventLog>
{
}

public class EventLogDataService : BaseDataService<EventLog>, IEventLogDataService
{
    public EventLogDataService(IRepository repository)
        : base(repository)
    {
    }
}