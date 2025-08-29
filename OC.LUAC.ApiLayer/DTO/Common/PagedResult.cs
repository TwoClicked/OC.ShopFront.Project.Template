using System.Text.Json.Serialization;

namespace OC.LUAC.ApiLayer.DTO.Common
{
    public class PagedResult<T>
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("items")]
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
