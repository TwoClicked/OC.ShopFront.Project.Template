namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_de { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName_en { get; set; } = string.Empty;
        public string? CategoryName_de { get; set; }

        public string? ThumbnailUrl { get; set; }
        public bool IsFeatured { get; set; }
        public int VariantCount { get; set; }
        public int TotalStock { get; set; }
        public List<ProductImageResponseDto> Images { get; set; } = new();
    }

}
