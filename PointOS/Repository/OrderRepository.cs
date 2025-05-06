using Microsoft.EntityFrameworkCore;
using PointOS.Data;
using PointOS.Models;

namespace PointOS.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync(string searchTerm = null, string status = null,
                                                           DateTime? startDate = null, DateTime? endDate = null,
                                                           int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(searchTerm) ||
                o.CustomerName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.DateAdded >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.DateAdded <= endDate.Value);
        }

        // Apply pagination
        return await query
            .OrderByDescending(o => o.DateAdded)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalOrdersCountAsync(string searchTerm = null, string status = null,
                                                   DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(searchTerm) ||
                o.CustomerName.Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.DateAdded >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.DateAdded <= endDate.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Order> GetOrderByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> GetOrderByNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}