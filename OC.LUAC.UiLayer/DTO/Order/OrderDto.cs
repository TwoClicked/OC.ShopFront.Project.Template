namespace OC.LUAC.UiLayer.DTO.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        // optional: for displaying in account page
        public decimal Total { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
