using System.Text.Json.Serialization;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class UpdateProductDto
    {
        [JsonPropertyName("name_en")]
        public string Name_en { get; set; } = "";

        [JsonPropertyName("name_de")]
        public string Name_de { get; set; } = "";

        [JsonPropertyName("description_en")]
        public string Description_en { get; set; } = "";

        [JsonPropertyName("description_de")]
        public string Description_de { get; set; } = "";

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }
    }
}
