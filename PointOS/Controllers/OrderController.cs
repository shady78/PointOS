using Microsoft.AspNetCore.Mvc;
using PointOS.Models;
using PointOS.Repository;
using PointOS.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PointOS.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderController(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        // GET: Order
        public async Task<IActionResult> Index(string searchTerm, string status,
                                              DateTime? startDate, DateTime? endDate,
                                              int page = 1, int pageSize = 10)
        {
            try
            {
                // Get orders with pagination
                var orders = await _orderRepository.GetAllOrdersAsync(
                    searchTerm, status, startDate, endDate, page, pageSize);

                var totalOrders = await _orderRepository.GetTotalOrdersCountAsync(
                    searchTerm, status, startDate, endDate);

                // Map orders to view models
                var orderViewModels = orders.Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.CustomerName,
                    Total = o.Total,
                    Status = o.Status,
                    DateAdded = o.DateAdded,
                    DateModified = o.DateModified,
                    CustomerImageUrl = o.CustomerImageUrl,
                    CustomerInitials = o.CustomerInitials ?? GetInitials(o.CustomerName!),
                    CustomerInitialsBackgroundColor = o.CustomerInitialsBackgroundColor ?? GetRandomColor(o.CustomerName!),
                    OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name,
                        SKU = oi.SKU ?? oi.Product?.SKU,
                        ProductImageUrl = oi.ProductImageUrl ?? oi.Product?.Image,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Discount = oi.Discount
                    }).ToList()
                }).ToList();

                var model = new OrderListViewModel
                {
                    Orders = orderViewModels,
                    SearchTerm = searchTerm,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                    TotalItems = totalOrders
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while retrieving orders");

                TempData["ErrorMessage"] = "An error occurred while retrieving orders. Please try again later.";

                return View(new OrderListViewModel
                {
                    Orders = new List<OrderViewModel>(),
                    SearchTerm = searchTerm,
                    Status = status,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = 1
                });
            }
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            try
            {
                // Generate a new order number
                var orderNumber = GenerateNewOrderNumber();

                // Set it in ViewBag to be used in the view
                ViewBag.NextOrderNumber = orderNumber;

                // Create an empty OrderViewModel with default values
                var model = new OrderViewModel
                {
                    DateAdded = DateTime.Now,
                    Status = "Processing",
                    VAT = 5, // Default VAT rate
                    OrderNumber = orderNumber
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while preparing create order form");

                TempData["ErrorMessage"] = "An error occurred while preparing the create order form. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Map the view model to domain model
                var order = new Order
                {
                    OrderNumber = model.OrderNumber,
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerImageUrl = model.CustomerImageUrl,
                    CustomerInitials = GetInitials(model.CustomerName),
                    CustomerInitialsBackgroundColor = GetRandomColor(model.CustomerName),
                    Total = model.Total,
                    Status = model.Status,
                    DateAdded = model.DateAdded,
                    PaymentMethod = model.PaymentMethod,
                    ShippingMethod = model.ShippingMethod,
                    ShippingRate = model.ShippingRate,
                    VAT = model.VAT,
                    GrandTotal = model.GrandTotal,
                    BillingAddress = model.BillingAddress,
                    BillingCity = model.BillingCity,
                    BillingState = model.BillingState,
                    BillingPostalCode = model.BillingPostalCode,
                    BillingCountry = model.BillingCountry,
                    ShippingAddress = model.ShippingAddress,
                    ShippingCity = model.ShippingCity,
                    ShippingState = model.ShippingState,
                    ShippingPostalCode = model.ShippingPostalCode,
                    ShippingCountry = model.ShippingCountry,
                    InvoiceNumber = GenerateInvoiceNumber(),
                    RewardPoints = model.RewardPoints,
                    OrderItems = model.OrderItems.Select(oi => new OrderItem
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        SKU = oi.SKU,
                        ProductImageUrl = oi.ProductImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Discount = oi.Discount,
                        DeliveryDate = oi.DeliveryDate
                    }).ToList()
                };

                // Save the order
                var orderId = await _orderRepository.CreateOrderAsync(order);

                TempData["SuccessMessage"] = $"Order #{model.OrderNumber} has been created successfully.";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while creating order");

                TempData["ErrorMessage"] = "An error occurred while creating the order. Please try again.";
                return View(model);
            }
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Map order to view model
                var orderViewModel = new OrderViewModel
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    Total = order.Total,
                    Status = order.Status,
                    DateAdded = order.DateAdded,
                    DateModified = order.DateModified,
                    CustomerImageUrl = order.CustomerImageUrl,
                    CustomerInitials = order.CustomerInitials ?? GetInitials(order.CustomerName!),
                    CustomerInitialsBackgroundColor = order.CustomerInitialsBackgroundColor ?? GetRandomColor(order.CustomerName),
                    // Map additional fields
                    PaymentMethod = order.PaymentMethod,
                    ShippingMethod = order.ShippingMethod,
                    ShippingRate = order.ShippingRate,
                    VAT = order.VAT,
                    GrandTotal = order.GrandTotal,
                    CustomerEmail = order.CustomerEmail,
                    CustomerPhone = order.CustomerPhone,
                    BillingAddress = order.BillingAddress,
                    BillingCity = order.BillingCity,
                    BillingState = order.BillingState,
                    BillingPostalCode = order.BillingPostalCode,
                    BillingCountry = order.BillingCountry,
                    ShippingAddress = order.ShippingAddress,
                    ShippingCity = order.ShippingCity,
                    ShippingState = order.ShippingState,
                    ShippingPostalCode = order.ShippingPostalCode,
                    ShippingCountry = order.ShippingCountry,
                    InvoiceNumber = order.InvoiceNumber,
                    ShippingTrackingNumber = order.ShippingTrackingNumber,
                    RewardPoints = order.RewardPoints,
                    // Map order items
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? oi.ProductName,
                        SKU = oi.SKU ?? oi.Product?.SKU,
                        ProductImageUrl = oi.ProductImageUrl ?? oi.Product?.Image,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Discount = oi.Discount,
                        DeliveryDate = oi.DeliveryDate
                    }).ToList()
                };

                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while retrieving order for editing");

                TempData["ErrorMessage"] = "An error occurred while loading the order. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get the existing order
                var existingOrder = await _orderRepository.GetOrderByIdAsync(model.Id);
                if (existingOrder == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update order properties
                existingOrder.CustomerName = model.CustomerName;
                existingOrder.CustomerEmail = model.CustomerEmail;
                existingOrder.CustomerPhone = model.CustomerPhone;
                existingOrder.CustomerInitials = GetInitials(model.CustomerName);
                existingOrder.CustomerInitialsBackgroundColor = GetRandomColor(model.CustomerName);
                existingOrder.Total = model.Total;
                existingOrder.Status = model.Status;
                existingOrder.PaymentMethod = model.PaymentMethod;
                existingOrder.ShippingMethod = model.ShippingMethod;
                existingOrder.ShippingRate = model.ShippingRate;
                existingOrder.VAT = model.VAT;
                existingOrder.GrandTotal = model.GrandTotal;
                existingOrder.BillingAddress = model.BillingAddress;
                existingOrder.BillingCity = model.BillingCity;
                existingOrder.BillingState = model.BillingState;
                existingOrder.BillingPostalCode = model.BillingPostalCode;
                existingOrder.BillingCountry = model.BillingCountry;
                existingOrder.ShippingAddress = model.ShippingAddress;
                existingOrder.ShippingCity = model.ShippingCity;
                existingOrder.ShippingState = model.ShippingState;
                existingOrder.ShippingPostalCode = model.ShippingPostalCode;
                existingOrder.ShippingCountry = model.ShippingCountry;
                existingOrder.ShippingTrackingNumber = model.ShippingTrackingNumber;
                existingOrder.RewardPoints = model.RewardPoints;
                existingOrder.DateModified = DateTime.Now;

                // Handle order items
                existingOrder.OrderItems.Clear(); // Remove existing items
                foreach (var itemVM in model.OrderItems)
                {
                    var orderItem = new OrderItem
                    {
                        Id = itemVM.Id > 0 ? itemVM.Id : 0, // If new item, set Id to 0
                        OrderId = model.Id,
                        ProductId = itemVM.ProductId,
                        ProductName = itemVM.ProductName,
                        SKU = itemVM.SKU,
                        ProductImageUrl = itemVM.ProductImageUrl,
                        Quantity = itemVM.Quantity,
                        UnitPrice = itemVM.UnitPrice,
                        Discount = itemVM.Discount,
                        DeliveryDate = itemVM.DeliveryDate
                    };
                    existingOrder.OrderItems.Add(orderItem);
                }

                // Update the order
                await _orderRepository.UpdateOrderAsync(existingOrder);

                TempData["SuccessMessage"] = $"Order #{model.OrderNumber} has been updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while updating order");

                TempData["ErrorMessage"] = "An error occurred while updating the order. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Map order to view model
                var orderViewModel = new OrderViewModel
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    CustomerName = order.CustomerName,
                    Total = order.Total,
                    Status = order.Status,
                    DateAdded = order.DateAdded,
                    DateModified = order.DateModified,
                    CustomerImageUrl = order.CustomerImageUrl,
                    CustomerInitials = order.CustomerInitials ?? GetInitials(order.CustomerName!),
                    CustomerInitialsBackgroundColor = order.CustomerInitialsBackgroundColor ?? GetRandomColor(order.CustomerName),
                    // Map additional fields
                    PaymentMethod = order.PaymentMethod,
                    ShippingMethod = order.ShippingMethod,
                    ShippingRate = order.ShippingRate,
                    VAT = order.VAT,
                    GrandTotal = order.GrandTotal,
                    CustomerEmail = order.CustomerEmail,
                    CustomerPhone = order.CustomerPhone,
                    BillingAddress = order.BillingAddress,
                    BillingCity = order.BillingCity,
                    BillingState = order.BillingState,
                    BillingPostalCode = order.BillingPostalCode,
                    BillingCountry = order.BillingCountry,
                    ShippingAddress = order.ShippingAddress,
                    ShippingCity = order.ShippingCity,
                    ShippingState = order.ShippingState,
                    ShippingPostalCode = order.ShippingPostalCode,
                    ShippingCountry = order.ShippingCountry,
                    InvoiceNumber = order.InvoiceNumber,
                    ShippingTrackingNumber = order.ShippingTrackingNumber,
                    RewardPoints = order.RewardPoints,
                    // Map order items
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name,
                        SKU = oi.SKU ?? oi.Product?.SKU,
                        ProductImageUrl = oi.ProductImageUrl ?? oi.Product?.Image,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Discount = oi.Discount,
                        DeliveryDate = oi.DeliveryDate
                    }).ToList()
                };

                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while retrieving order details");

                TempData["ErrorMessage"] = "An error occurred while loading the order details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Add this method to your OrderController
        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // In a real application, you would generate a PDF here
                // For now, we'll redirect to the details page with a message
                TempData["SuccessMessage"] = "Invoice download initiated.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "An error occurred while generating the invoice.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Helper methods for customer initials and colors
        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "?";

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();

            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private string GetRandomColor(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "#5D87FF"; // Default blue

            // Generate a consistent color based on the customer name
            var colors = new[]
            {
                "#FF6B6B", // Red
                "#4ECDC4", // Teal
                "#FFD166", // Yellow
                "#6A0572", // Purple
                "#1A936F", // Green
                "#FF9A76", // Orange
                "#3A86FF", // Blue
                "#8338EC", // Violet
                "#3D5A80", // Navy
                "#FF5A5F"  // Pink
            };

            int hash = input.GetHashCode();
            int index = Math.Abs(hash) % colors.Length;
            return colors[index];
        }

        // Helper method to generate an order number
        private string GenerateNewOrderNumber()
        {
            // In a real application, this would be more sophisticated,
            // possibly calling the repository to get the next sequential number

            // For demo purposes, we'll use the current timestamp
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            return timestamp;
        }

        // Helper method to generate an invoice number
        private string GenerateInvoiceNumber()
        {
            // In a real application, this would be more sophisticated

            // For demo purposes, we'll use "INV-" plus the current timestamp
            string timestamp = DateTime.Now.ToString("yyMMddHHmmss");
            return "INV-" + timestamp;
        }

        // Helper method to delete an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete the order
                await _orderRepository.DeleteOrderAsync(id);

                TempData["SuccessMessage"] = $"Order #{order.OrderNumber} has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error
                // _logger.LogError(ex, "Error occurred while deleting order");

                TempData["ErrorMessage"] = "An error occurred while deleting the order. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}