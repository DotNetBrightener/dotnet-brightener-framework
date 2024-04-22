using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public class PostgreSqlRepository : Repository
{
    public PostgreSqlRepository(DbContext                    dbContext,
                                ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                                IEventPublisher              eventPublisher,
                                ILoggerFactory               loggerFactory)
        : base(dbContext, currentLoggedInUserResolver, eventPublisher, loggerFactory)
    {
    }

    public override IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                  DateTimeOffset?            from,
                                                  DateTimeOffset?            to)
    {

        throw new NotSupportedException();
    }
}