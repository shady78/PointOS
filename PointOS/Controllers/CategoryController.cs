using Microsoft.AspNetCore.Mvc;
using PointOS.Repository;
using PointOS.ViewModels;
using PointOS.Models;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace PointOS.Controllers;
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CategoryController(
        ICategoryRepository categoryRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _categoryRepository = categoryRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Category
    public async Task<IActionResult> Index(string searchTerm, string type, int page = 1, int pageSize = 10)
    {
        // Get categories with pagination
        var categories = await _categoryRepository.GetAllCategoriesAsync(searchTerm, type, page, pageSize);
        var totalCategories = await _categoryRepository.GetTotalCategoriesCountAsync(searchTerm, type);

        // Map categories to view models
        var categoryViewModels = categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Type = c.Type,
            ExistingImage = c.Image,
            // Include the Products property
            Products = c.Products?.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                // Map other essential properties if needed
                Price = p.Price,
                Quantity = p.Quantity,
                Status = p.Status
            })
        }).ToList();

        var model = new CategoryListViewModel
        {
            Categories = categoryViewModels,
            SearchTerm = searchTerm,
            Type = type,
            TotalPages = (int)Math.Ceiling(totalCategories / (double)pageSize),
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalCategories // Add this line
        };

        return View(model);
    }

    // GET: Category/Create
    public IActionResult Create()
    {
        // Initialize a new category with default values
        var model = new CategoryViewModel
        {
            Type = "Automated" // Default type
        };

        return View(model);
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Map view model to domain model
                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    Type = model.Type
                };

                // Handle image upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Create directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media", "categories");
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

                    // Set the image path in the category
                    category.Image = "/media/categories/" + uniqueFileName;
                }

                // Save to database
                await _categoryRepository.CreateCategoryAsync(category);

                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while creating category");

                ModelState.AddModelError("", "An error occurred while saving the category. Please try again.");
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    // GET: Category/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            // Get the category by id
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map domain model to view model
            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Type = category.Type,
                ExistingImage = category.Image,
                Products = category.Products?.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU
                })
            };

            // IMPORTANT: Return the "Create" view explicitly
            return View("Create", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while loading the category.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model, bool RemoveImage = false)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid category ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Get existing category
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update properties
                category.Name = model.Name;
                category.Description = model.Description;
                category.Type = model.Type;

                // Handle image changes
                if (RemoveImage)
                {
                    // Remove the existing image file if there is one
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    category.Image = null;
                }
                else if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Remove the old image first
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Create directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media", "categories");
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

                    // Set the image path in the category
                    category.Image = "/media/categories/" + uniqueFileName;
                }

                // Save changes
                await _categoryRepository.UpdateCategoryAsync(category);

                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while updating category");

                ModelState.AddModelError("", "An error occurred while updating the category. Please try again.");
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    // GET: Category/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Delete the category image if it exists
            if (!string.IsNullOrEmpty(category.Image))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete from database
            await _categoryRepository.DeleteCategoryAsync(id);

            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while deleting category");

            TempData["ErrorMessage"] = "An error occurred while deleting the category. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Category/BulkDelete
    [HttpPost]
    public async Task<IActionResult> BulkDelete(string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return Json(new { success = false, message = "No categories selected." });
        }

        try
        {
            var categoryIds = ids.Split(',').Select(int.Parse).ToList();

            // Handle each category
            foreach (var id in categoryIds)
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category != null)
                {
                    // Delete the category image if it exists
                    if (!string.IsNullOrEmpty(category.Image))
                    {
                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, category.Image.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Delete the category
                    await _categoryRepository.DeleteCategoryAsync(id);
                }
            }

            return Json(new { success = true, message = $"{categoryIds.Count} categories deleted successfully." });
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while bulk deleting categories");

            return Json(new { success = false, message = "An error occurred while deleting categories." });
        }
    }

    // POST: Category/BulkUpdateType
    [HttpPost]
    public async Task<IActionResult> BulkUpdateType(string ids, string type)
    {
        if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(type))
        {
            return Json(new { success = false, message = "Invalid parameters." });
        }

        if (type != "Automated" && type != "Manual")
        {
            return Json(new { success = false, message = "Invalid category type." });
        }

        try
        {
            var categoryIds = ids.Split(',').Select(int.Parse).ToList();
            var updatedCount = 0;

            // Update each category
            foreach (var id in categoryIds)
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category != null)
                {
                    category.Type = type;
                    await _categoryRepository.UpdateCategoryAsync(category);
                    updatedCount++;
                }
            }

            return Json(new { success = true, message = $"{updatedCount} categories updated to '{type}' type." });
        }
        catch (Exception ex)
        {
            // Log the error
            // _logger.LogError(ex, "Error occurred while bulk updating category types");

            return Json(new { success = false, message = "An error occurred while updating category types." });
        }
    }
}