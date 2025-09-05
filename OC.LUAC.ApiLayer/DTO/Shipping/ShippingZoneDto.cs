// DTO/ShippingZone/ShippingZoneDto.cs
using OC.LUAC.ApiLayer.DTO.Shipping;

namespace OC.LUAC.ApiLayer.DTO.ShippingZone
{
    public class ShippingZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsDefault { get; set; }

        // FIX: now a list of CountryDto instead of string
        public List<CountryDto> Countries { get; set; } = new();
    }
}
