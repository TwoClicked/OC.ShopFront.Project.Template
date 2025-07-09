using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.DTO
{
    /// <summary>
    /// Represents an item in the shopping cart. DtO for CartItem.
    /// </summary> 
    public class CartItem
    {
        public string ProductName { get; set; } // Name of the product

        public int ProductVariantId { get; set; } // Foreign key to ProductVariant  

        public string Size { get; set; } // Size of the product variant
        public decimal UnitPrice { get; set; } // Price of the product variant
        public int Quantity { get; set; } // Quantity of this product variant in the cart
        public string ImageUrl { get; set; } // URL of the product image (Preview added to cart) 

    }
}
