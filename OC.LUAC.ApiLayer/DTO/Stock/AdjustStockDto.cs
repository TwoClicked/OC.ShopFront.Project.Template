using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Stock
{
    public class AdjustStockDto
    {
        [Required] public int VariantId { get; set; }
        // Use positive for add, negative for remove (service will validate)
        [Range(-100000, 100000)] public int QuantityChange { get; set; }

        // "Add", "Remove", "Reserve", etc. — parsed to your enum in controller
        [Required] public string ActionType { get; set; }

        // Optional: link a stock change to an order
        public int? OrderId { get; set; }
    }
}
