﻿// <auto-generated />
using System;
using DotNetBrightener.TemplateEngine.Data.Mssql.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DotNetBrightener.TemplateEngine.Data.Mssql.Migrations
{
    [DbContext(typeof(TemplateEngineDbContext))]
    partial class TemplateEngineDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DotNetBrightener.TemplateEngine.Data.Entity.TemplateRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTimeOffset?>("CreatedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DeletedBy")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTimeOffset?>("DeletedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DeletionReason")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("FieldsString")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Fields");

                    b.Property<string>("FromAssemblyName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTimeOffset?>("ModifiedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("TemplateContent")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TemplateTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TemplateType")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.HasKey("Id");

                    b.HasIndex("TemplateType");

                    b.ToTable("TemplateRecord", "TemplateEngine");
                });
#pragma warning restore 612, 618
        }
    }
}
