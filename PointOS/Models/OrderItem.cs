using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PointOS.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    [Required]
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [NotMapped]
    public decimal Subtotal => Quantity * UnitPrice - Discount;

    public string? ProductImageUrl { get; set; }
    public string? SKU { get; set; }
    public DateTime DeliveryDate { get; set; }

    public string? ProductName { get; set; }
}