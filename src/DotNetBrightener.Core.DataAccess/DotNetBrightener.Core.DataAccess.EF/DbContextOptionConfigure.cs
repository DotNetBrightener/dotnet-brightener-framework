using System;
using System.Collections.Concurrent;
using DotNetBrightener.Core.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.DataAccess.EF
{
    public class DbContextOptionConfigure
    {
        private static readonly object LockObject = new object();

        private static DbContextOptionConfigure _singleInstance;

        public static DbContextOptionConfigure Instance
        {
            get
            {
                lock (LockObject)
                {
                    if (_singleInstance != null)
                        return _singleInstance;

                    _singleInstance = new DbContextOptionConfigure();
                    return _singleInstance;
                }
            }
        }

        private readonly ConcurrentDictionary<DatabaseProvider, Action<DbContextOptionsBuilder, DatabaseConfiguration>>
            _configurationActions =
                new ConcurrentDictionary<DatabaseProvider, Action<DbContextOptionsBuilder, DatabaseConfiguration>>();

        internal void RegisterDbContextOptionConfigure(
            DatabaseProvider                                       databaseProvider,
            Action<DbContextOptionsBuilder, DatabaseConfiguration> configure)
        {
            _configurationActions.TryAdd(databaseProvider, configure);
        }

        internal void ConfigureDbProvider(DbContextOptionsBuilder builder,
                                          DatabaseConfiguration   dbConfiguration)
        {
            if (_configurationActions.TryGetValue(dbConfiguration.DatabaseProvider, out var action))
            {
                action?.Invoke(builder, dbConfiguration);
                return;
            }

            throw new InvalidOperationException($"Cannot find the supported Database Provider. Make sure to register it before configuring the DbContexts");
        }
    }
}