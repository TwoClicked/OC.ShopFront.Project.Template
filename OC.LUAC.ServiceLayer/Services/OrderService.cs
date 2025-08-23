using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    public class OrderService : IOrderService
    {

        private readonly AppDbContext _context;
        private readonly IStockActionService _stockActionService;

        public OrderService(AppDbContext context, IStockActionService stockActionService)
        {
            _context = context;
            _stockActionService = stockActionService;
        }

        /// <summary>
        /// Create a full order with items + shipping
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.Status = OrderStatus.New;
            order.OrderNumber = $"ORD-{DateTime.Now.Ticks}"; // Generate a unique order number based on the current timestamp
            

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        /// <summary>
        /// List all orders placed by a customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && !o.IsDeleted)
                .Include(o => o.Items)
                .ToListAsync();
        }

        /// <summary>
        /// Admin/Customer fetch specific order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
        }

        /// <summary>
        /// Marks an order as shipped, updating the status and tracking information.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="trackingNumber"></param>
        /// <param name="trackingUrl"></param>
        /// <returns></returns>
        public async Task<bool> MarkOrderAsShippedAsync(int orderId, string trackingNumber, string trackingUrl)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                return false; // Order not found or already deleted
            }
            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = trackingNumber;
            order.TrackingUrl = trackingUrl;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cancels an order by its ID, marking it as deleted and updating the status.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items) // include items for restock
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.IsDeleted)
                return false;

            // Restock items
            foreach (var oi in order.Items)
            {
                await _stockActionService.RecordStockChangeAsync(
                    oi.ProductVariantId,
                    oi.Quantity,
                    StockActionType.CancelledRestock,
                    order.Id);
            }

            // Mark as cancelled
            order.Status = OrderStatus.Cancelled;
            order.IsDeleted = true;
            order.DeletedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)          // ✅ navigation property
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)   // ✅ navigation property
                .Include(o => o.Customer)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

    }
}
