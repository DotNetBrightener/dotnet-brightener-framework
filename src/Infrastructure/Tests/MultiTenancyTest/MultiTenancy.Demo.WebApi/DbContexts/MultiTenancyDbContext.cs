using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataAccess.EF.PostgreSQL;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.MultiTenancy.DbContexts;
using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultiTenancy.Demo.WebApi.DbContexts;

public class MultiTenancyDbContext(DbContextOptions<MultiTenancyDbContext> options) : PostgreSqlVersioningMigrationEnabledDbContext(options)
{
    protected override void ConfigureModelBuilder(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableMultiTenantSupport<Clinic>();

        modelBuilder.Entity<User>();
    }
}

public class Clinic : TenantBase
{
    [MaxLength(256)]
    public string? Address { get; set; }
    [MaxLength(64)]
    public string? City    { get; set; }
    [MaxLength(32)]
    public string? State   { get; set; }
    [MaxLength(10)]
    public string? ZipCode { get; set; }
}

public class User : GuidBaseEntityWithAuditInfo
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? UserName { get; set; }
}

internal class MultiTenancyDbContextMigration: PostgreSqlDbContextDesignTimeFactory<MultiTenancyDbContext>;