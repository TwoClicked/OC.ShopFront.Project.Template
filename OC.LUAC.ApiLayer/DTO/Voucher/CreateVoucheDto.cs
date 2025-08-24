namespace OC.LUAC.ApiLayer.DTO.Voucher
{
    public class CreateVoucherDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int? MaxUsageCount { get; set; }
        public bool AppliesToShipping { get; set; } = false;
    }
}
