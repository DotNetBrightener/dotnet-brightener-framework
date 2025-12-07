using DotNetBrightener.DataAccess.Services;
using NotificationService.Entity;
using NotificationService.Repository;

namespace NotificationService.Services;

public interface INotificationMessageQueueDataService : IBaseDataService<NotificationMessageQueue>;

public class NotificationMessageQueueDataService(INotificationMessageQueueRepository repository)
    : BaseDataService<NotificationMessageQueue>(repository),
      INotificationMessageQueueDataService;