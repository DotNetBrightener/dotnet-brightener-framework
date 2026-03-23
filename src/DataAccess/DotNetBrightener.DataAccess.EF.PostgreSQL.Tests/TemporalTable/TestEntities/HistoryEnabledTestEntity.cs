using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL.Tests.TemporalTable.TestEntities;

/// <summary>
/// 	Test entity with history tracking enabled
/// </summary>
[HistoryEnabled]
public class HistoryEnabledTestEntity : BaseEntityWithAuditInfo
{
	/// <summary>
	/// 	Gets or sets the name of the entity
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// 	Gets or sets the quantity
	/// </summary>
	public int Quantity { get; set; }

	/// <summary>
	/// 	Gets or sets the price
	/// </summary>
	public decimal Price { get; set; }

	/// <summary>
	/// 	Gets or sets whether the entity is active
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// 	Gets or sets the expiry date
	/// </summary>
	public DateTime? ExpiryDate { get; set; }
}
