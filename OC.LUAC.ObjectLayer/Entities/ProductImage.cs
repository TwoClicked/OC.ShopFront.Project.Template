using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Entities
{
    /// <summary>
    /// Represents an image associated with a product in the e-commerce platform.
    /// </summary>
    public class ProductImage
    {
        //Primary key
        public int Id { get; set; }

        //Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ImageUrl { get; set; }

        // To order images in gallery 
        public int SortOrder { get; set; } = 0;

        //Soft delete support  
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
