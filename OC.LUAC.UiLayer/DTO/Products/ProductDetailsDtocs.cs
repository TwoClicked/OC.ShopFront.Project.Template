using System.Text.Json.Serialization;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class ProductDetailsDto
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

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [JsonPropertyName("images")]
        public List<ProductImageDto> Images { get; set; } = new();

        [JsonPropertyName("variants")]
        public List<ProductVariantDto> Variants { get; set; } = new();
    }
}
