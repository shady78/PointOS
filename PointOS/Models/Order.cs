using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PointOS.Models;

public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = null!;

    //[Required]
    [StringLength(100)]
    public string? CustomerName { get; set; }

    // Optional foreign key to Customer if you have a Customer table
    //public int? CustomerId { get; set; }
    //[ForeignKey("CustomerId")]
    //public virtual Customer Customer { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [Required]
    [StringLength(20)]
    public string? Status { get; set; } // Completed, Processing, Delivering, Failed, Cancelled

    [Required]
    public DateTime DateAdded { get; set; }

    public DateTime? DateModified { get; set; }

    // Optional - customer profile image or initials color
    public string? CustomerImageUrl { get; set; }

    public string? CustomerInitials { get; set; }

    public string? CustomerInitialsBackgroundColor { get; set; }

    // Navigation properties for order items
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();






    // Additional properties
    public string? PaymentMethod { get; set; }
    public string? ShippingMethod { get; set; }
    public decimal ShippingRate { get; set; }
    public decimal VAT { get; set; }
    private decimal _grandTotal;
    private bool _hasCustomGrandTotal = false;

    public decimal GrandTotal
    {
        get
        {
            return _hasCustomGrandTotal ? _grandTotal : Total + ShippingRate;
        }
        set
        {
            _grandTotal = value;
            _hasCustomGrandTotal = true;
        }
    }
    // Customer details
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    // Address details
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }

    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }

    // Documents
    public string? InvoiceNumber { get; set; }
    public string? ShippingTrackingNumber { get; set; }
    public int? RewardPoints { get; set; }

}