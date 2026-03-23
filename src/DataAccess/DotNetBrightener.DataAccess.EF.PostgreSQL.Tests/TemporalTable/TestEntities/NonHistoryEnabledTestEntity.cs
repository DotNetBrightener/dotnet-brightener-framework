using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;

/// <summary>
/// 	Test entity without history tracking (for negative tests)
/// </summary>
public class NonHistoryEnabledTestEntity : BaseEntityWithAuditInfo
{
	/// <summary>
	/// 	Gets or sets the value
	/// </summary>
	public string Value { get; set; } = string.Empty;
}
