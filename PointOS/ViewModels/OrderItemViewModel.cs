namespace PointOS.ViewModels;

public class OrderItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal => Quantity * UnitPrice - Discount;

    public string? SKU { get; set; }
    public DateTime DeliveryDate { get; set; }
}