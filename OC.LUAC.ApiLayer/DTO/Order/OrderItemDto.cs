namespace OC.LUAC.ApiLayer.DTO.Order
{
    public class OrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
