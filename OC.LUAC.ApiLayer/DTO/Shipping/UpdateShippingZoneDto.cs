using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.ShippingZone
{
    public class UpdateShippingZoneDto
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        [Range(0, 999999)] public decimal BaseCost { get; set; }
        [Range(0, 999999)] public decimal FreeShippingThreshold { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Countries { get; set; } = new(); // replaces mapping
    }
}
