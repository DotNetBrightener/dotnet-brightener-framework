using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

public interface IMigrationDefinitionDbContext;

/// <summary>
///     Marks the implementation of this interface as the DbContext
///     that provides migration definitions for its based <typeparamref name="TBaseDbContext"/>
/// </summary>
/// <typeparam name="TBaseDbContext">
///     The DbContext that defines all the models and entities
/// </typeparam>
public interface IMigrationDefinitionDbContext<TBaseDbContext> : IMigrationDefinitionDbContext
    where TBaseDbContext : DbContext;