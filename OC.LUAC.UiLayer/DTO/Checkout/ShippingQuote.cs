namespace OC.LUAC.UiLayer.DTO.Checkout
{
    public class ShippingQuote
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsFree { get; set; }
        public decimal ShippingCost { get; set; }  
        public decimal Subtotal { get; set; }
    }
}
