using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Voucher
{
    public class UpdateVoucherDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public int? MaxUsageCount { get; set; }

        public bool AppliesToShipping { get; set; }
    }
}
