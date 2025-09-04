namespace OC.LUAC.UiLayer.DTO.AdminDash
{
    public class OrdersPagedResponse
    {
        public int Total { get; set; }
        public List<OrderCountDto> Items { get; set; } = new();
    }
}
