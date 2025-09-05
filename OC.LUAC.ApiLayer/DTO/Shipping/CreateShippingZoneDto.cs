// DTO/ShippingZone/CreateShippingZoneDto.cs
using OC.LUAC.ApiLayer.DTO.Shipping;

namespace OC.LUAC.ApiLayer.DTO.ShippingZone
{
    public class CreateShippingZoneDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public bool IsDefault { get; set; }

        // FIX: allow passing both code + name
        public List<CountryDto> Countries { get; set; } = new();
    }
}
