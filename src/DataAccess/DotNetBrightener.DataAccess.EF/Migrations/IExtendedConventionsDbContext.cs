#nullable enable
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.Migrations;

/// <summary>
///     Represents an extended <see cref="DbContext"/> that extends the conventions configuration
/// </summary>
public interface IExtendedConventionsDbContext
{
    List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; }
}