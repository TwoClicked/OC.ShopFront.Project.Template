// DTO/Product/UploadMultipleProductImagesForm.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class UploadMultipleProductImagesForm
    {
        [Required] public List<IFormFile> Files { get; set; }
        public List<int> SortOrders { get; set; }
        public int BaseSortOrder { get; set; } = 0;
    }
}
