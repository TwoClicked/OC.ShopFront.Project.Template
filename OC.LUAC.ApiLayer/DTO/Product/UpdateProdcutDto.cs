using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class UpdateProductDto
    {
        [Required, MaxLength(200)]
        public string Name_en { get; set; }

        [Required, MaxLength(200)]
        public string Name_de { get; set; }

        [MaxLength(4000)]
        public string Description_en { get; set; }

        [MaxLength(4000)]
        public string Description_de { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsFeatured { get; set; }
    }
}
