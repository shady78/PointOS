using System.ComponentModel.DataAnnotations;

namespace PointOS.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Image")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Current Image")]
    public string? ExistingImage { get; set; }

    [Required(ErrorMessage = "Type is required")]
    [Display(Name = "Category Type")]
    public string Type { get; set; } = "Automated"; // Automated, Manual

    public IEnumerable<ProductViewModel>? Products { get; set; }

}