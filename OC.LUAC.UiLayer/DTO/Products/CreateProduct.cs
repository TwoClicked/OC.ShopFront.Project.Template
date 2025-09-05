using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.UiLayer.DTO.Products
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "English name is required")]
        [StringLength(100, ErrorMessage = "Name must be under 100 characters")]
        public string Name_en { get; set; } = "";

        [Required(ErrorMessage = "German name is required")]
        [StringLength(100)]
        public string Name_de { get; set; } = "";

        [Required(ErrorMessage = "English description is required")]
        [StringLength(1000)]
        public string Description_en { get; set; } = "";

        [Required(ErrorMessage = "German description is required")]
        [StringLength(1000)]
        public string Description_de { get; set; } = "";

        [Range(0.01, 99999, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
    }
}
