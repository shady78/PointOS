using PointOS.Models;

namespace PointOS.Repository;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(string searchTerm = null, string type = null, int page = 1, int pageSize = 10);
    Task<Category> GetCategoryByIdAsync(int id);
    Task<int> GetTotalCategoriesCountAsync(string searchTerm = null, string type = null);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(int id);
}