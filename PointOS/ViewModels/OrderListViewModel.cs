namespace PointOS.ViewModels;

public class OrderListViewModel
{
    public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
    public string SearchTerm { get; set; }
    public string Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}