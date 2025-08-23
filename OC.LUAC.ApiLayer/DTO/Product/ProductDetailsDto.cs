namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public string Name_en { get; set; }
        public string? Name_de { get; set; }
        public string? Description_en { get; set; }
        public string? Description_de { get; set; }
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName_en { get; set; }
        public string? CategoryName_de { get; set; }

        public List<ProductImageResponseDto> Images { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
        public bool IsFeatured { get; set; }
    }
}
