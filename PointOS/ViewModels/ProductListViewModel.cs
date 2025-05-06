namespace PointOS.ViewModels;

public class ProductListViewModel
{
    public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? CategoryId { get; set; }
    public List<CategoryViewModel>? Categories { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; } // Add this property

}