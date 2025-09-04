namespace OC.LUAC.UiLayer.DTO.AdminDash
{
    public class AdminOrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
