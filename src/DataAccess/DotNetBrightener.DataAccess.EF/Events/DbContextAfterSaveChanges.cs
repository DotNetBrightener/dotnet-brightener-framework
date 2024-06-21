using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DotNetBrightener.DataAccess.EF.Events;

/// <summary>
///     Represents the information of the event fired after
///     <see cref="DbContext.SaveChanges()"/> or
///     <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/> is executed
/// </summary>
public class DbContextAfterSaveChanges : IEventMessage
{
    public List<EntityEntry> InsertedEntityEntries { get; set; }
    public List<EntityEntry> UpdatedEntityEntries  { get; set; }
    public string            CurrentUserId         { get; set; }
    public string            CurrentUserName       { get; set; }
}