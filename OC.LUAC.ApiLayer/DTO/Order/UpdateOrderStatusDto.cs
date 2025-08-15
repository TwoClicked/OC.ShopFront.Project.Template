using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Order
{
    public class UpdateOrderStatusDto
    {
        [Required] public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public string TrackingUrl { get; set; }
    }
}
