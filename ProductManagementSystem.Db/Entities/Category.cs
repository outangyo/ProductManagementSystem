using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.Db.Entities;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Relationship: One Category has many Products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
