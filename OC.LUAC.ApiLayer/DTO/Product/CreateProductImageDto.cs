using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class CreateProductImageDto
    {

        [Required]
        public string ImageUrl { get; set; }
        public int SortOrder { get; set; }

    }
}
