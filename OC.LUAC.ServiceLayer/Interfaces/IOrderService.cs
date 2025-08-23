using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order in the system.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<Order> CreateOrderAsync(Order order);

        /// <summary>
        /// Retrieves a list of orders for a specific customer by their ID.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId);

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<Order?> GetOrderByIdAsync(int orderId);

        /// <summary>
        /// Marks an order as shipped.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="trackingNumber"></param>
        /// <param name="trackingUrl"></param>
        /// <returns></returns>
        Task<bool> MarkOrderAsShippedAsync(int orderId, string trackingNumber, string trackingUrl);

        /// <summary>
        /// Cancels an order by its ID.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> CancelOrderAsync(int orderId);

        Task<List<Order>> GetAllOrdersAsync();

    }
}
