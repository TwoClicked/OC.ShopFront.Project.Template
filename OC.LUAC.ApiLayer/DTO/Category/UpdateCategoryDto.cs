using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Category
{
    public class UpdateCategoryDto
    {
        [Required, MaxLength(200)]
        public string Name_en { get; set; }

        [Required, MaxLength(200)]
        public string Name_de { get; set; }
    }
}
