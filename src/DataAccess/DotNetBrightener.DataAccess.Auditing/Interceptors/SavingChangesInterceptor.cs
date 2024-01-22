using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;


public class AuditContext : DbContext
{
    private readonly DatabaseConfiguration _dbConfiguration;

    public AuditContext(DatabaseConfiguration dbConfiguration)
    {
        _dbConfiguration = dbConfiguration;
    }
}

public class SavingChangesInterceptor: ISaveChangesInterceptor
{
    private readonly DatabaseConfiguration _dbConfiguration;

    public SavingChangesInterceptor(DatabaseConfiguration dbConfiguration)
    {
        _dbConfiguration = dbConfiguration;
    }

    #region SavingChanges
    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        //_audit = CreateAudit(eventData.Context);

        //using var auditContext = new AuditContext(_dbConfiguration);

        //auditContext.Add(_audit);
        //await auditContext.SaveChangesAsync();

        //TODO Insert audit record

        return result;
    }

    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        return SavingChangesAsync(eventData, result).Result;
    }
    #endregion

    #region SavedChanges
    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        
        return SavedChangesAsync(eventData, result).Result;
    }

    public async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        //using var auditContext = new AuditContext(_connectionString);

        //auditContext.Attach(_audit);
        //_audit.Succeeded = true;
        //_audit.EndTime = DateTime.UtcNow;

        //await auditContext.SaveChangesAsync(cancellationToken);

        // TODO Update audit record

        return result;
    }
    #endregion

    #region SaveChangesFailed
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        SaveChangesFailedAsync(eventData).Wait();
    }

    public async Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        //using var auditContext = new AuditContext(_connectionString);

        //auditContext.Attach(_audit);
        //_audit.Succeeded = false;
        //_audit.EndTime = DateTime.UtcNow;
        //_audit.ErrorMessage = eventData.Exception.InnerException?.Message;

        //await auditContext.SaveChangesAsync(cancellationToken);


        // TODO Update audit record
    }
    #endregion

    #region CreateAudit
    //private static SaveChangesAudit CreateAudit(DbContext context)
    //{
    //    context.ChangeTracker.DetectChanges();

    //    var audit = new SaveChangesAudit { AuditId = Guid.NewGuid(), StartTime = DateTime.UtcNow };

    //    foreach (var entry in context.ChangeTracker.Entries())
    //    {
    //        var auditMessage = entry.State switch
    //        {
    //            EntityState.Deleted => CreateDeletedMessage(entry),
    //            EntityState.Modified => CreateModifiedMessage(entry),
    //            EntityState.Added => CreateAddedMessage(entry),
    //            _ => null
    //        };

    //        if (auditMessage != null)
    //        {
    //            audit.Entities.Add(new EntityAudit { State = entry.State, AuditMessage = auditMessage });
    //        }
    //    }

    //    return audit;

    //    string CreateAddedMessage(EntityEntry entry)
    //        => entry.Properties.Aggregate(
    //            $"Inserting {entry.Metadata.DisplayName()} with ",
    //            (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");

    //    string CreateModifiedMessage(EntityEntry entry)
    //        => entry.Properties.Where(property => property.IsModified || property.Metadata.IsPrimaryKey()).Aggregate(
    //            $"Updating {entry.Metadata.DisplayName()} with ",
    //            (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");

    //    string CreateDeletedMessage(EntityEntry entry)
    //        => entry.Properties.Where(property => property.Metadata.IsPrimaryKey()).Aggregate(
    //            $"Deleting {entry.Metadata.DisplayName()} with ",
    //            (auditString, property) => auditString + $"{property.Metadata.Name}: '{property.CurrentValue}' ");
    //}
    #endregion
}
