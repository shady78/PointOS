using PointOS.Models;

namespace PointOS.Repository;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync(string searchTerm = null, string status = null, int? categoryId = null, int page = 1, int pageSize = 10);
    Task<Product> GetProductByIdAsync(int id);
    Task<int> GetTotalProductsCountAsync(string searchTerm = null, string status = null, int? categoryId = null);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
}   