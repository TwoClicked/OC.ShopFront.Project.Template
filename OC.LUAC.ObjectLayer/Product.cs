using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer
{
    public class Product
    {
        //Primary key
        public int id { get; set; }

        //Multi-language support
        public string Name_en { get; set; }
        public string Name_de { get; set; }

        public string Description_en { get; set; }
        public string Description_de { get; set; }

        public decimal Price { get; set; }

        // Category relationship
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        // Featured on homepage
        public bool IsFeatured { get; set; }

        // Soft delete support 
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }


        // Timestamp for creation
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties

        public virtual ICollection<ProductImage>? Images { get; set; }
        public virtual ICollection<ProductVariant>? Variants { get; set; }
    }
}
