using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public class PostgreSqlRepository : Repository
{
    public PostgreSqlRepository(DbContext                    dbContext,
                                ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                                IServiceProvider             serviceProvider,
                                ILoggerFactory               loggerFactory)
        : base(dbContext, serviceProvider, loggerFactory)
    {
    }

    public override IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                  DateTimeOffset?            from,
                                                  DateTimeOffset?            to)
    {

        throw new NotSupportedException();
    }
}