using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DotNetBrightener.DataAccess.EF.Events;

/// <summary>
///     Represents the information of the event fired before
///     <see cref="DbContext.SaveChanges()"/> or
///     <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/> is executed
/// </summary>
public class DbContextBeforeSaveChanges: IEventMessage
{
    public EntityEntry [ ] InsertedEntityEntries { get; set; }
    public EntityEntry [ ] UpdatedEntityEntries  { get; set; }
    public string          CurrentUserId         { get; set; }
    public string          CurrentUserName       { get; set; }
}