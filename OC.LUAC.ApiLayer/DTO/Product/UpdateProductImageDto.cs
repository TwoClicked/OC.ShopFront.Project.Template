using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class UpdateProductImageDto
    {

        [Required]
        public string ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }
}
