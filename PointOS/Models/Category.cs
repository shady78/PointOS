using System.ComponentModel.DataAnnotations;

namespace PointOS.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Image { get; set; }

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Automated"; // Automated, Manual

    public virtual ICollection<Product>? Products { get; set; }
}
