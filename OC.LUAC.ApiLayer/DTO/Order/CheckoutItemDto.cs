using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Order
{
    public class CheckoutItemDto
    {
        [Required] public int ProductId { get; set; }
        [Required] public int ProductVariantId { get; set; }
        [Range(1, 1000)] public int Quantity { get; set; }

        // Optional; server will snapshot if omitted
        public decimal? UnitPrice { get; set; }
        public string? ProductName { get; set; }
        public string? Size { get; set; }
    }
}