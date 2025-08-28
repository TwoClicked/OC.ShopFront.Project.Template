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

        public decimal Price { get; set; }

        [JsonPropertyName("thumbnailUrl")]
        public string? ThumbnailUrl { get; set; }

        [JsonPropertyName("images")]
        public List<ProductImageDto> Images { get; set; } = new();

        public string? FullThumbnailUrl(string apiBaseUrl) =>
            string.IsNullOrWhiteSpace(ThumbnailUrl)
                ? null
                : apiBaseUrl.TrimEnd('/') + ThumbnailUrl;
    }
}
