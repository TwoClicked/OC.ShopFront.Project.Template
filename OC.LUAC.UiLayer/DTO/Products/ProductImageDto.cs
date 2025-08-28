using System.Text.Json.Serialization;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = "";

        public int SortOrder { get; set; }
    }

}
