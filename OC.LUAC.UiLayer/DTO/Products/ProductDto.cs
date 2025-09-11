using System.Text.Json.Serialization;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class ProductDto
    {
        public int Id { get; set; }

        [JsonPropertyName("name_en")]
        public string Name_En { get; set; } = "";

        [JsonPropertyName("name_de")]
        public string Name_De { get; set; } = "";

        [JsonPropertyName("description_en")]
        public string Description_En { get; set; } = "";

        [JsonPropertyName("description_de")]
        public string Description_De { get; set; } = "";

        public decimal Price { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName_en")]
        public string CategoryName_En { get; set; } = "";

        [JsonPropertyName("categoryName_de")]
        public string CategoryName_De { get; set; } = "";

        [JsonPropertyName("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonPropertyName("variantCount")]
        public int VariantCount { get; set; }

        [JsonPropertyName("totalStock")]
        public int TotalStock { get; set; }

        [JsonPropertyName("images")]
        public List<ProductImageDto> Images { get; set; } = new();

        public string? FullThumbnailUrl(string baseUrl) =>
            string.IsNullOrWhiteSpace(ThumbnailUrl)
                ? null
                : $"{baseUrl.TrimEnd('/')}/{ThumbnailUrl.TrimStart('/')}";
    }
}
