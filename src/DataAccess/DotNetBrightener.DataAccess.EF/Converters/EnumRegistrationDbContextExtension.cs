#nullable enable
using DotNetBrightener.DataAccess.EF.Entities;
using DotNetBrightener.DataAccess.EF.EnumLookup;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

public static class EnumRegistrationDbContextExtension
{
    /// <summary>
    ///     Registers to the <see cref="ModelBuilder"/> the lookup table for <typeparamref name="TEnum"/> enum.
    /// </summary>
    /// <remarks>
    ///     If the application uses EF Migrations, the lookup table will be created and seeded with the values of the enum.
    /// </remarks>
    /// <typeparam name="TEnum">The type of the enum to generate the lookup table</typeparam>
    /// <param name="modelBuilder">
    ///     The <see cref="ModelBuilder"/>
    /// </param>
    public static void RegisterEnumLookupTable<TEnum>(this DbContext dbContext,
                                                      ModelBuilder   modelBuilder,
                                                      string?        schema = null)
        where TEnum : struct, Enum
    {
        var enumLookupTableName = typeof(TEnum).Name + "Lookup";

        modelBuilder.Entity<EnumLookupEntity<TEnum>>(lookupEntity =>
        {
            lookupEntity.HasKey(x => x.Id);

            lookupEntity.Property(x => x.Value)
                        .HasMaxLength(1024);

            lookupEntity.Property(x => x.Value).HasColumnName(typeof(TEnum).Name + "Value");

            lookupEntity.ToTable(enumLookupTableName, schema: schema)
                        .HasData(Enum.GetValues<TEnum>()
                                     .Select(x => new EnumLookupEntity<TEnum>
                                      {
                                          Id    = Convert.ToInt32(x),
                                          Value = x.ToString()
                                      }));
        });

        try
        {
            var lookupContainer = dbContext.GetService<ILookupEnumContainer>();

            if (lookupContainer is not null)
            {
                lookupContainer.RegisterEnum<TEnum>();
            }
        }
        catch (Exception)
        {
            // ignore
        }
    }
}