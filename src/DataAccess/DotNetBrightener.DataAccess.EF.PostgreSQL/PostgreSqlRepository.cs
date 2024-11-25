#nullable enable
using DotNetBrightener.DataAccess.EF.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

public class PostgreSqlRepository(
    DbContext        dbContext,
    IServiceProvider serviceProvider,
    ILoggerFactory   loggerFactory)
    : Repository(dbContext, serviceProvider, loggerFactory)
{
    public override IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                  DateTimeOffset?            from,
                                                  DateTimeOffset?            to)
    { 
        throw new NotSupportedException("History mechanism is not supported by default in PostgreSQL");
    }
}