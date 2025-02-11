﻿using DotNetBrightener.DataAccess.EF.Migrations;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class TestDbContext : AdvancedDbContext
{
    public readonly Guid Id = Guid.NewGuid();

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>();
    }
}