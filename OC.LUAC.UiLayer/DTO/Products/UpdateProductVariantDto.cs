using System.Text.Json.Serialization;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class UpdateProductVariantDto
    {
        [JsonPropertyName("size")]
        public string Size { get; set; } = "";

        [JsonPropertyName("stock")]
        public int Stock { get; set; }
    }
}
