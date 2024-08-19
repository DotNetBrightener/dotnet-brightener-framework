using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;


public class AuditContext(DatabaseConfiguration dbConfiguration) : DbContext
{
    private readonly DatabaseConfiguration _dbConfiguration = dbConfiguration;
}

public class SavingChangesInterceptor(DatabaseConfiguration dbConfiguration) : SaveChangesInterceptor
{
    private readonly DatabaseConfiguration _dbConfiguration = dbConfiguration;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData      eventData,
                                                                                InterceptionResult<int> result,
                                                                                CancellationToken cancellationToken =
                                                                                    default)
    {
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                           int                           result,
                                                           CancellationToken             cancellationToken = default)
    {

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
                                                      CancellationToken       cancellationToken = default)
    {

        await base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}