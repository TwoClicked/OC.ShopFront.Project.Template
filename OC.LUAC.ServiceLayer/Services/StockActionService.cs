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
                throw new Exception("Product variant not found");

            variant.Stock += quantity; // can be positive or negative

            var stockAction = new StockAction
            {
                ProductVariantId = variantId,
                Quantity = quantity,
                ActionType = actionType,
                OrderId = orderId,
                Timestamp = DateTime.Now
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
