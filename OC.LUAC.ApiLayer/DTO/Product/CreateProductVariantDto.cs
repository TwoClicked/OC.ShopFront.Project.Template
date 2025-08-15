using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class CreateProductVariantDto
    {

        [Required] public string Size { get; set; }
        [Range(0, 100000)] public int Stock { get; set; }

    }
}
