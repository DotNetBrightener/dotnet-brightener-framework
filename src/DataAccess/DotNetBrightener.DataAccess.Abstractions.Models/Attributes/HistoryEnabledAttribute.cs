namespace DotNetBrightener.DataAccess.Attributes;

/// <summary>
///     Marks the associated entity to be a history enabled entity.
///     A history enabled entity will have a temporal table created in the database.
///     This only works with Entity Framework.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HistoryEnabledAttribute : Attribute;