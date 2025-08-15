using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ServiceLayer.Services
{
    /// <summary>
    /// Service class for managing stock actions in the e-commerce system.
    /// </summary>
    public class StockActionService : IStockActionService
    {
        private readonly AppDbContext _context;

        public StockActionService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Records a stock change for a product variant.
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="quantity"></param>
        /// <param name="actionType"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<StockAction> RecordStockChangeAsync(int variantId, int quantity, StockActionType actionType, int? orderId = null)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null || variant.IsDeleted)
            {
                throw new Exception("Product variant not found");
            }

            // Always work with a positive amount; the action decides the sign
            var q = Math.Abs(quantity);

            // Decide the signed delta based on the action type
            int delta;
            switch (actionType)
            {
                case StockActionType.Increase:
                case StockActionType.CancelledRestock:
                    delta = q;      // stock goes up
                    break;

                case StockActionType.Decrease:
                case StockActionType.Sold:
                    delta = -q;     // stock goes down
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported action type: {actionType}");
            }

            // Prevent negative stock (optional, but recommended)
            var newStock = variant.Stock + delta;
            if (newStock < 0)
            {
                throw new InvalidOperationException("Insufficient stock for this operation.");
            }

            variant.Stock = newStock;

            // Log the action (store the absolute quantity; ActionType carries the meaning)
            var stockAction = new StockAction
            {
                ProductVariantId = variantId,
                Quantity = q,
                ActionType = actionType,
                OrderId = orderId,          // keep null if not provided
                Timestamp = DateTime.UtcNow // UTC is safer for logs
            };

            _context.StockActions.Add(stockAction);
            await _context.SaveChangesAsync();

            return stockAction;
        }


        /// <summary>
        /// Retrieves the stock log for a specific product variant.
        /// </summary>
        /// <param name="variantId"></param>
        /// <returns></returns>
        public async Task<List<StockAction>> GetStockLogByVariantAsync(int variantId)
        {
            return await _context.StockActions
                .Where(s => s.ProductVariantId == variantId)
                .OrderByDescending(s => s.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the most recent stock actions across all variants, limited to a specified count.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<StockAction>> GetRecentStockActionsAsync(int count = 20)
        {
            return await _context.StockActions
                .OrderByDescending(s => s.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
