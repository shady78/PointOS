using Microsoft.EntityFrameworkCore;
using PointOS.Data;
using PointOS.Models;

namespace PointOS.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync(string searchTerm = null, string type = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Categories.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                c.Description!.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(c => c.Type == type);
        }

        // Apply pagination
        return await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<int> GetTotalCategoriesCountAsync(string searchTerm = null, string type = null)
    {
        var query = _context.Categories.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                c.Description!.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(c => c.Type == type);
        }

        return await query.CountAsync();
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _context.Entry(category).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        // Check if any products are associated with this category
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            // Set category to null for all products with this category
            var products = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
            foreach (var product in products)
            {
                product.CategoryId = null;
            }
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}