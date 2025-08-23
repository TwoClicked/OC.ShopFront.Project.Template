namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class BrowseQuery
    {
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public bool? Featured { get; set; }
        public string? Sort { get; set; } = "newest";   // price|name|newest
        public string? Dir { get; set; } = "desc";      // asc|desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 24;
    }
}
