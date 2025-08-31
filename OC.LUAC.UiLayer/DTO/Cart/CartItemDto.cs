namespace OC.LUAC.UiLayer.DTO.Cart
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int ProductVariantId { get; set; }   // ✅ NEW
        public string ProductName { get; set; } = "";
        public string Size { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int MaxStock { get; set; }
        public string? ThumbnailUrl { get; set; }

        public decimal Total => Price * Quantity;
    }
}
