namespace OC.LUAC.ApiLayer.DTO.Shipping
{
    public class CountryDto
    {
        public string Code { get; set; } = string.Empty;  // ISO Alpha-2 (e.g. "BE")
        public string Name { get; set; } = string.Empty;  // Human readable (e.g. "Belgium")
    }
}
