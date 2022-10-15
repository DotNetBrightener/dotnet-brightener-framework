using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess;
using DotNetBrightener.Core.DataAccess.Abstractions;
using DotNetBrightener.Core.DataAccess.EF.Repositories;
using DotNetBrightener.Core.Modular;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Integration.Modular.Database
{
    /// <summary>
    ///     The centralized db context which is used in the modular environment
    /// </summary>
    public class CentralizedDbContext : DotNetBrightenerDbContext
    {
        private readonly LoadedModuleEntries    _loadedModuleEntries;
        private readonly IEnumerable<DbContext> _otherModuleDbContexts;
        private readonly ILogger                _logger;

        public CentralizedDbContext(DbContextOptions<CentralizedDbContext> options,
                                    IDataWorkContext                       dataWorkContext,
                                    LoadedModuleEntries                    loadedModuleEntries,
                                    IEnumerable<DbContext>                 otherModuleDbContexts,
                                    ILogger<CentralizedDbContext>          logger)
            : base(options, dataWorkContext)
        {
            _loadedModuleEntries   = loadedModuleEntries;
            _otherModuleDbContexts = otherModuleDbContexts;
            _logger                = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var orderedDbContexts = _otherModuleDbContexts.OrderByModuleDependencies(_loadedModuleEntries);

            var onModelCreating = typeof(DbContext).GetMethod(nameof(OnModelCreating),
                                                              BindingFlags.Instance | BindingFlags.NonPublic,
                                                              Type.DefaultBinder,
                                                              new[] {typeof(ModelBuilder)},
                                                              null);
            if (onModelCreating != null)
            {
                foreach (var dbContext in orderedDbContexts)
                {
                    onModelCreating.Invoke(dbContext, new[] {modelBuilder});
                }
            }
        }

        public override int SaveChanges()
        {
            if (ChangeTracker.HasChanges())
            {
                SetModifiedInformation(ChangeTracker.Entries().ToArray());
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (ChangeTracker.HasChanges())
            {
                SetModifiedInformation(ChangeTracker.Entries().ToArray());
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected virtual void SetModifiedInformation(EntityEntry[] entityEntries)
        {
            var creatingEntries = entityEntries.Where(e => e.State == EntityState.Added);

            foreach (var entry in creatingEntries)
            {
                try
                {
                    var createdByPropName = nameof(BaseEntityWithAuditInfo.CreatedBy);

                    if (entry.Properties.Any(_ => _.Metadata.Name == createdByPropName) &&
                        entry.Property(createdByPropName) != null)
                    {
                        entry.Property(createdByPropName).CurrentValue =
                            CurrentLoggedInUser ?? CurrentLoggedInUserId?.ToString();
                    }

                    var createdDatePropName = nameof(BaseEntityWithAuditInfo.Created);
                    if (entry.Properties.Any(_ => _.Metadata.Name == createdDatePropName) &&
                        entry.Property(createdDatePropName) != null)
                    {
                        entry.Property(createdDatePropName).CurrentValue = DateTimeOffset.Now;
                    }
                }
                catch (Exception error)
                {
                    _logger.LogWarning(error, "Error while trying to set Audit information");
                }
            }

            var modifiedEntries = entityEntries.Where(e => e.State == EntityState.Added ||
                                                           e.State == EntityState.Modified ||
                                                           e.State == EntityState.Deleted);

            foreach (var entry in modifiedEntries)
            {
                try
                {
                    var lastUpdatedByPropName = nameof(BaseEntityWithAuditInfo.LastUpdatedBy);
                    if (entry.Properties.Any(_ => _.Metadata.Name == lastUpdatedByPropName) &&
                        entry.Property(lastUpdatedByPropName) != null)
                    {
                        entry.Property(lastUpdatedByPropName).CurrentValue =
                            CurrentLoggedInUser ?? CurrentLoggedInUserId?.ToString();
                    }

                    var lastUpdatedPropName = nameof(BaseEntityWithAuditInfo.LastUpdated);
                    if (entry.Properties.Any(_ => _.Metadata.Name == lastUpdatedPropName) &&
                        entry.Property(lastUpdatedPropName) != null)
                    {
                        entry.Property(lastUpdatedPropName).CurrentValue = DateTimeOffset.Now;
                    }
                }
                catch (Exception error)
                {
                    _logger.LogWarning(error, "Error while trying to set Audit information");
                }
            }
        }

        public override void Dispose()
        {
            SaveChanges();
        }
    }
}