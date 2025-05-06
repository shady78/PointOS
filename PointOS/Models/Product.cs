using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PointOS.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = null!; // Stock Keeping Unit

    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }

    public int Quantity { get; set; }

    [StringLength(100)]
    public string? Image { get; set; }

    public int Rating { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Published"; // Published, Inactive, Scheduled

    [StringLength(50)]
    public string? StockStatus { get; set; } // Low stock, In stock, etc.
}