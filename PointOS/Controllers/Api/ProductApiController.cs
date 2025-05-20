using Microsoft.AspNetCore.Mvc;
using PointOS.Models;
using PointOS.Repository;

namespace PointOS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductApiController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/ProductApi/Search?searchTerm=keyword
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string searchTerm, string status = "Published")
        {
            try
            {
                // Get products matching search term (use the first page with larger page size)
                var products = await _productRepository.GetAllProductsAsync(searchTerm, status, null, 1, 50);

                if (products == null)
                {
                    return Ok(new { success = true, data = new List<object>() });
                }

                // Map to simplified objects for the order form
                var result = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sku = p.SKU,
                    price = p.Price,
                    imageUrl = string.IsNullOrEmpty(p.Image) ? "/media/products/default.png" : p.Image,
                    stock = p.Quantity,
                    lowStock = p.Quantity <= 10
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { success = false, message = "An error occurred while searching products." });
            }
        }

        // GET: api/ProductApi/GetById/5
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found." });
                }

                var result = new
                {
                    id = product.Id,
                    name = product.Name,
                    sku = product.SKU,
                    price = product.Price,
                    imageUrl = string.IsNullOrEmpty(product.Image) ? "/media/products/default.png" : product.Image,
                    stock = product.Quantity,
                    lowStock = product.Quantity <= 10
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the product." });
            }
        }

        // GET: api/ProductApi/GetByIds?ids=1,2,3
        [HttpGet("GetByIds")]
        public async Task<IActionResult> GetByIds([FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    return Ok(new { success = true, data = new List<object>() });
                }

                // Parse comma-separated ids
                var idList = ids.Split(',')
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .ToList();

                if (idList.Count == 0)
                {
                    return Ok(new { success = true, data = new List<object>() });
                }

                // Get products by id
                var products = new List<Product>();
                foreach (var id in idList)
                {
                    var product = await _productRepository.GetProductByIdAsync(id);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }

                // Map to simplified objects
                var result = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sku = p.SKU,
                    price = p.Price,
                    imageUrl = string.IsNullOrEmpty(p.Image) ? "/media/products/default.png" : p.Image,
                    stock = p.Quantity,
                    lowStock = p.Quantity <= 10
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving products." });
            }
        }
    }
}