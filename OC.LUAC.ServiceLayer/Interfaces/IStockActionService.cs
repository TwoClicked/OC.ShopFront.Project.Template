using OC.LUAC.ObjectLayer.Entities;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IStockActionService
    {
        /// <summary>
        /// Records a stock change for a product variant.
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="quantity"></param>
        /// <param name="actionType"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<StockAction> RecordStockChangeAsync(int variantId, int quantity, StockActionType actionType, int? orderId = null);

        /// <summary>
        /// Retrieves the stock log for a specific product variant.
        /// </summary>
        /// <param name="variantId"></param>
        /// <returns></returns>
        Task<List<StockAction>> GetStockLogByVariantAsync(int variantId);

        /// <summary>
        /// Retrieves the most recent stock actions across all product variants.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<List<StockAction>> GetRecentStockActionsAsync(int count = 20);
    }
}
