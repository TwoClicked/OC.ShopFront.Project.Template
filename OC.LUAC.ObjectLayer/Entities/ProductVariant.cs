using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Entities
{
    /// <summary>
    /// Represents a product variant, which includes details such as size, stock quantity, and a reference to the parent product.
    /// </summary>
    public class ProductVariant
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string Size { get; set; } // Example: "S", "M", "L", "XL"

        public int Stock { get; set; } // Stock quantity for this variant (current available stock for this size)

        //Soft delete support

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Optional : Stock audit trail
        public virtual ICollection<StockAction>? StockActions { get; set; }

    }
}
