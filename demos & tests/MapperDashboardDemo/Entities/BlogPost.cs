using System.ComponentModel.DataAnnotations;

namespace MapperDashboardDemo.Entities;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
    public List<string> Tags { get; set; } = [];
}
