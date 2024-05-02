﻿// <auto-generated />
using System;
using DotNetBrightener.DataAccess.DataMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetBrightener.DataAccess.DataMigration.Migrations
{
    [DbContext(typeof(DataMigrationDbContext))]
    [Migration("20240502094152_Init_DataMigration_Schema")]
    partial class Init_DataMigration_Schema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DotNetBrightener.DataAccess.DataMigration.DataMigrationHistory", b =>
                {
                    b.Property<string>("MigrationId")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<DateTime?>("AppliedDateUtc")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.HasKey("MigrationId");

                    b.ToTable("__DataMigrationsHistory", "DataMigration");
                });
#pragma warning restore 612, 618
        }
    }
}
