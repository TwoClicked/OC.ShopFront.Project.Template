using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.ShippingZone
{
    public class CreateShippingZoneDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Countries { get; set; } = new(); // ISO codes
    }
}
