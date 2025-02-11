﻿// <auto-generated />
using System;
using DotNetBrightener.Core.Logging.DbStorage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetBrightener.Core.Logging.DbStorage.Migrations
{
    [DbContext(typeof(LoggingDbContext))]
    partial class LoggingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DotNetBrightener.Core.Logging.EventLog", b =>
                {
                    b.Property<string>("FormattedMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Level")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoggerName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PropertiesDictionary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RemoteIpAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RequestId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RequestUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StackTrace")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantIds")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserAgent")
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("Level");

                    b.HasIndex("LoggerName");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("LoggerName"), new[] { "Level", "TimeStamp" });

                    b.HasIndex("RequestId");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("RequestId"), new[] { "TimeStamp", "Level" });

                    b.HasIndex("TimeStamp");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("TimeStamp"), new[] { "Level", "LoggerName" });

                    b.ToTable("EventLog", "Log");
                });
#pragma warning restore 612, 618
        }
    }
}
