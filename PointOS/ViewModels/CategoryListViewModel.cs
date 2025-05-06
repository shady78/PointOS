namespace PointOS.ViewModels;

public class CategoryListViewModel
{
    public IEnumerable<CategoryViewModel>? Categories { get; set; }
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; } // Add this property
}