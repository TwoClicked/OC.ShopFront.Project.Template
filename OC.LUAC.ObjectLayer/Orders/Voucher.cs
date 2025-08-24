namespace OC.LUAC.ObjectLayer.Orders
{
    public class Voucher
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        public decimal? Percentage { get; set; }
        public decimal? FixedAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int? MaxUsageCount { get; set; }
        public int CurrentUsageCount { get; set; } = 0;

        public bool AppliesToShipping { get; set; } = false;
    }
}
