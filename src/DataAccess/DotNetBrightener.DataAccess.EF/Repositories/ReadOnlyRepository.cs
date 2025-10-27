#nullable enable
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Exceptions;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.Repositories;

public class ReadOnlyRepository : IReadOnlyRepository
{
    protected readonly DbContext                     DbContext;
    protected readonly ICurrentLoggedInUserResolver? CurrentLoggedInUserResolver;
    protected readonly IEventPublisher?              EventPublisher;
    protected readonly IDateTimeProvider?            DateTimeProvider;
    protected readonly IHttpContextAccessor?         HttpContextAccessor;
    protected          ILogger                       Logger { get; init; }
    protected readonly Guid                          ScopeId = Guid.CreateVersion7();

    public ReadOnlyRepository(DbContext        dbContext,
                              IServiceProvider serviceProvider,
                              ILoggerFactory   loggerFactory)
    {
        DbContext                   = dbContext;
        CurrentLoggedInUserResolver = serviceProvider.TryGet<ICurrentLoggedInUserResolver>();
        EventPublisher              = serviceProvider.TryGet<IEventPublisher>();
        DateTimeProvider            = serviceProvider.TryGet<IDateTimeProvider>();
        HttpContextAccessor         = serviceProvider.TryGet<HttpContextAccessor>();
        Logger                      = loggerFactory.CreateLogger(GetType());
    }

    public virtual T? Get<T>(Expression<Func<T, bool>> expression)
        where T : class => GetAsync(expression).Result;

    public virtual async Task<T?> GetAsync<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await Fetch(expression).SingleOrDefaultAsync();
    }

    public virtual T? GetFirst<T>(Expression<Func<T, bool>> expression)
        where T : class => GetFirstAsync(expression).Result;

    public virtual async Task<T?> GetFirstAsync<T>(Expression<Func<T, bool>> expression) where T : class
    {
        return await Fetch(expression).FirstOrDefaultAsync();
    }

    public virtual TResult? Get<T, TResult>(Expression<Func<T, bool>>?   expression,
                                            Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).SingleOrDefault();
    }

    public virtual TResult? GetFirst<T, TResult>(Expression<Func<T, bool>>?   expression,
                                                 Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        return Fetch(expression, propertiesPickupExpression).FirstOrDefault();
    }

    public virtual IQueryable<T> Fetch<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        if (expression == null)
            return DbContext.Set<T>().AsQueryable();

        return DbContext.Set<T>().Where(expression);
    }

    public virtual IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                                 DateTimeOffset?            from,
                                                 DateTimeOffset?            to)
        where T : class, new()
    {
        if (!typeof(T).HasAttribute<HistoryEnabledAttribute>())
        {
            throw new VersioningNotSupportedException<T>();
        }

        var initialQuery = DbContext.Set<T>();

        var temporalQuery = initialQuery.TemporalAll();

        if (from is not null ||
            to is not null)
        {
            from ??= DateTimeOffset.UnixEpoch;

            to ??= DateTimeOffset.UtcNow;

            temporalQuery = initialQuery.TemporalFromTo(from.Value.UtcDateTime, to.Value.UtcDateTime)
                                        .OrderBy(entry =>
                                                     Microsoft.EntityFrameworkCore.EF.Property<DateTime>(entry,
                                                                                                         "PeriodStart"));
        }

        if (expression is not null)
        {
            temporalQuery = temporalQuery.Where(expression);
        }

        return temporalQuery;
    }

    public virtual IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>?   expression,
                                                         Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class
    {
        if (propertiesPickupExpression == null)
            throw new ArgumentNullException(nameof(propertiesPickupExpression));

        var query = Fetch(expression);

        return query.Select(propertiesPickupExpression);
    }

    public virtual int Count<T>(Expression<Func<T, bool>>? expression = null)
        where T : class => CountAsync(expression).Result;

    public virtual async Task<int> CountAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        return expression is null
                   ? await DbContext.Set<T>().CountAsync()
                   : await DbContext.Set<T>().CountAsync(expression);
    }

    public virtual async Task<int> CountNonDeletedAsync<T>(Expression<Func<T, bool>>? expression = null) where T : class
    {
        if (!typeof(T).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            throw
                new InvalidOperationException($"Entity of type {typeof(T).Name} does not have soft-delete capability");
        }

        var query = DbContext.Set<T>().Where($"{nameof(IAuditableEntity.IsDeleted)} != True");

        return expression is null
                   ? await query.CountAsync()
                   : await query.CountAsync(expression);
    }

    public bool Any<T>(Expression<Func<T, bool>>? expression = null)
        where T : class => AnyAsync(expression).Result;

    public virtual async Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class
    {
        return expression is null
                   ? await DbContext.Set<T>().AnyAsync()
                   : await DbContext.Set<T>().AnyAsync(expression);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }
}