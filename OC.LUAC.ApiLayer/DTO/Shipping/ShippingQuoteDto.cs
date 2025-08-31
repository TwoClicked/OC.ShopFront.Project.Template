namespace OC.LUAC.ApiLayer.DTO.Shipping
{
    public class ShippingQuoteDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = "";
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsFree { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Subtotal { get; set; }
    }
}
