using System.ComponentModel.DataAnnotations;

namespace MapperDashboardDemo.Entities;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string InternalSku { get; set; } = string.Empty;
}
