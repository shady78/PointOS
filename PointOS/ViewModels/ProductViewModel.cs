    using System.ComponentModel.DataAnnotations;

    namespace PointOS.ViewModels;

    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = null!; // Stock Keeping Unit

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Current Image")]
        public string? ExistingImage { get; set; }

        [Display(Name = "Rating")]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string?  Status { get; set; }

        [Display(Name = "Stock Status")]
        public string? StockStatus { get; set; }

        // For dropdown lists
        public List<CategoryViewModel>? Categories { get; set; }



    }