namespace PointOS.ViewModels;

public class OrderViewModel
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    //public int? CustomerId { get; set; }
    public decimal Total { get; set; }
    public string? Status { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? DateModified { get; set; }
    public string? CustomerImageUrl { get; set; }
    public string? CustomerInitials { get; set; }
    public string? CustomerInitialsBackgroundColor { get; set; }
    public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();



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

    // Full address helpers
    public string FullBillingAddress => string.Join(", ", new[] {
        BillingAddress,
        BillingCity,
        BillingState,
        BillingPostalCode,
        BillingCountry
    }.Where(x => !string.IsNullOrEmpty(x)));

    public string FullShippingAddress => string.Join(", ", new[] {
        ShippingAddress,
        ShippingCity,
        ShippingState,
        ShippingPostalCode,
        ShippingCountry
    }.Where(x => !string.IsNullOrEmpty(x)));

    // Documents
    public string? InvoiceNumber { get; set; }
    public string? ShippingTrackingNumber { get; set; }
    public int? RewardPoints { get; set; }
}
