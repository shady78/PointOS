using Microsoft.AspNetCore.Mvc;
using PointOS.Models;
using PointOS.Repository;
using PointOS.ViewModels;

namespace PointOS.Controllers;
public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Product
    // GET: Product
    public async Task<IActionResult> Index(string searchTerm, string status, int? categoryId, int page = 1, int pageSize = 10)
    {
        try
        {
            //_logger.LogInformation("Retrieving products with filters. SearchTerm: {SearchTerm}, Status: {Status}, CategoryId: {CategoryId}", searchTerm, status, categoryId);

            // Initialize empty model in case of error
            var model = new ProductListViewModel
            {
                Products = new List<ProductViewModel>(),
                SearchTerm = searchTerm,
                Status = status,
                CategoryId = categoryId,
                CurrentPage = page,
                PageSize = pageSize,

                TotalPages = 1,
                Categories = new List<CategoryViewModel>()
            };

            // Get products with pagination
            var products = await _productRepository.GetAllProductsAsync(searchTerm, status, categoryId, page, pageSize);
            if (products == null)
            {
                //_logger.LogWarning("No products returned from repository");
                return View(model);
            }

            var totalProducts = await _productRepository.GetTotalProductsCountAsync(searchTerm, status, categoryId);

            // Get all categories for the filter dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var categoryViewModels = categories?.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            })?.ToList() ?? new List<CategoryViewModel>();

            // Map products to view models
            var productViewModels = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Status = p.Status,
                StockStatus = p.StockStatus ?? (p.Quantity <= 0 ? "Out of stock" : (p.Quantity <= 10 ? "Low stock" : "In stock")),
                Rating = p.Rating,
                ExistingImage = p.Image
            }).ToList();

            model = new ProductListViewModel
            {
                Products = productViewModels,
                SearchTerm = searchTerm,
                Status = status,
                CategoryId = categoryId,
                Categories = categoryViewModels,
                TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalProducts // Add this line
            };

            return View(model);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error occurred while retrieving products");
            // Return a basic model with error message
            TempData["ErrorMessage"] = "An error occurred while retrieving products. Please try again later.";

            return View(new ProductListViewModel
            {
                Products = new List<ProductViewModel>(),
                SearchTerm = searchTerm,
                Status = status,
                CategoryId = categoryId,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = 1,
                Categories = new List<CategoryViewModel>()
            });
        }
    }



    // GET: Product/Create
    public async Task<IActionResult> Create()
    {
        try
        {
            // Get all categories for the dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            // Initialize a new product with default values
            var model = new ProductViewModel
            {
                Status = "Published", // Default status
                Quantity = 0,
                Price = 0,
                Rating = 0
            };

            return View(model);
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while loading the Create product form");

            TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Product/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                // Map view model to domain model
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    SKU = model.SKU,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    CategoryId = model.CategoryId,
                    Status = model.Status!,
                    Rating = model.Rating,
                    // Set StockStatus based on quantity
                    StockStatus = model.Quantity <= 0 ? "Out of stock" : (model.Quantity <= 10 ? "Low stock" : "In stock")
                };

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Create directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the image path in the product
                    product.Image = "/media/products/" + uniqueFileName;
                }

                // Save to database
                await _productRepository.CreateProductAsync(product);

                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, redisplay the form with validation errors
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            return View(model);
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while creating product");

            ModelState.AddModelError("", "An error occurred while saving the product. Please try again.");

            // Re-populate categories for the dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            return View(model);
        }
    }

    // GET: Product/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            // Get the product by id
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Get all categories for the dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            // Map domain model to view model
            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Status = product.Status,
                Rating = product.Rating,
                ExistingImage = product.Image,
                StockStatus = product.StockStatus
            };

            return View("Create", model); // Reuse the Create view
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while loading product for edit");

            TempData["ErrorMessage"] = "An error occurred while loading the product. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            // Get the product by id
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map domain model to view model
            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Status = product.Status,
                Rating = product.Rating,
                ExistingImage = product.Image,
                StockStatus = product.StockStatus ?? (product.Quantity <= 0 ? "Out of stock" : (product.Quantity <= 10 ? "Low stock" : "In stock"))
            };

            return View(model);
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while retrieving product details");

            TempData["ErrorMessage"] = "An error occurred while loading the product details. Please try again later.";
            return RedirectToAction(nameof(Index));
        }
    }
    // POST: Product/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model, bool RemoveImage = false)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid product ID.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            if (ModelState.IsValid)
            {
                // Get existing product
                var product = await _productRepository.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update properties
                product.Name = model.Name;
                product.Description = model.Description;
                product.SKU = model.SKU;
                product.Price = model.Price;
                product.Quantity = model.Quantity;
                product.CategoryId = model.CategoryId;
                product.Status = model.Status!;
                product.Rating = model.Rating;
                product.StockStatus = model.Quantity <= 0 ? "Out of stock" : (model.Quantity <= 10 ? "Low stock" : "In stock");

                // Handle image changes
                if (RemoveImage)
                {
                    // Remove the existing image file if there is one
                    if (!string.IsNullOrEmpty(product.Image) && product.Image != "/media/products/default.png")
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    product.Image = null;
                }
                else if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Remove the old image first
                    if (!string.IsNullOrEmpty(product.Image) && product.Image != "/media/products/default.png")
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Create directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the image path in the product
                    product.Image = "/media/products/" + uniqueFileName;
                }

                // Save changes
                await _productRepository.UpdateProductAsync(product);

                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, redisplay the form with validation errors
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            return View("Create", model);
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while updating product");

            ModelState.AddModelError("", "An error occurred while updating the product. Please try again.");

            // Re-populate categories for the dropdown
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories?.ToList() ?? new List<Category>();

            return View("Create", model);
        }
    }

    // GET: Product/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Delete the product image if it exists and is not the default image
            if (!string.IsNullOrEmpty(product.Image) && product.Image != "/media/products/default.png")
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete from database
            await _productRepository.DeleteProductAsync(id);

            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while deleting product");

            TempData["ErrorMessage"] = "An error occurred while deleting the product. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}