namespace OC.LUAC.UiLayer.DTO.AdminDash
{
        public class AdminOrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Totals
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal TotalAfterDiscount { get; set; }

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Shipping Info
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;

        // Tracking
        public string? TrackingNumber { get; set; }
        public string? TrackingUrl { get; set; }

        public List<AdminOrderItemDto> Items { get; set; } = new();
         }
}
