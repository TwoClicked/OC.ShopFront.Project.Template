namespace OC.LUAC.UiLayer.DTO.Order
{
    public class CustomerOrderView
    {
        public int CustomerId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
    }
}
