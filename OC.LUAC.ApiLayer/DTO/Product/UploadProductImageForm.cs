// DTO/Product/UploadProductImageForm.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class UploadProductImageForm
    {
        [Required] public IFormFile File { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
