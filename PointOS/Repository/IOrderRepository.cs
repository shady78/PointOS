using PointOS.Models;

namespace PointOS.Repository;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllOrdersAsync(string searchTerm = null, string status = null,
                                              DateTime? startDate = null, DateTime? endDate = null,
                                              int page = 1, int pageSize = 10);
    Task<int> GetTotalOrdersCountAsync(string searchTerm = null, string status = null,
                                      DateTime? startDate = null, DateTime? endDate = null);
    Task<Order> GetOrderByIdAsync(int id);
    Task<Order> GetOrderByNumberAsync(string orderNumber);
    Task<Order> CreateOrderAsync(Order order);
    Task<Order> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(int id);
}