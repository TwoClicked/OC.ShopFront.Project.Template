namespace OC.LUAC.ApiLayer.DTO.Order
{
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ProductName { get; set; }
        public string? Size { get; set; }
    }
}
