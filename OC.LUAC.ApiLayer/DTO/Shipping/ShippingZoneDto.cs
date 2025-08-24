namespace OC.LUAC.ApiLayer.DTO.Shipping
{
    public class ShippingZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Countries { get; set; } = new();
    }
}
