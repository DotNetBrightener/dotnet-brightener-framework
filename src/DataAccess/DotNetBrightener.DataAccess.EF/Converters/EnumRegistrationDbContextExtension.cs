#nullable enable
using DotNetBrightener.DataAccess.EF.Converters;
using DotNetBrightener.DataAccess.EF.Entities;
using DotNetBrightener.DataAccess.EF.Migrations;

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
    public static void RegisterEnumLookupTable<TEnum>(this IExtendedConventionsDbContext dbContext,
                                                      ModelBuilder                       modelBuilder,
                                                      string?                            schema = null)
        where TEnum : struct, Enum
    {
        var enumLookupTableName = typeof(TEnum).Name + "Lookup";

        var lookupEntity = modelBuilder.Entity<EnumLookupEntity<TEnum>>();

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


        dbContext.ConventionConfigureActions.Add(builder =>
        {
            builder.Properties<TEnum>()
                   .HaveConversion<EnumLookupConverter<TEnum>>();
        });
    }

    /// <summary>
    ///     Call this method to extend the defaults and configure conventions before they run. This method must be called within the
    ///     <see cref="DbContext.ConfigureConventions" />.
    /// </summary>
    /// <param name="builder">
    ///     The builder being used to set defaults and configure conventions that will be used to build the model for this context.
    /// </param>
    public static void ExtendConfigureConventions(this IExtendedConventionsDbContext dbContext,
                                                  ModelConfigurationBuilder          builder)
    {
        foreach (var action in dbContext.ConventionConfigureActions)
        {
            action(builder);
        }
    }
}