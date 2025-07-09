using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.PDF
{
    /// <summary>
    /// Represents a line item in an order, including product details such as name, size, quantity, and unit price.
    /// </summary>
    public class OrderLineItem
    {

        public string ProductName { get; set; } // Name of the product
        public string Size { get; set; } // Size of the product variant
        public int Quantity { get; set; } // Quantity of this product variant in the order
        public decimal UnitPrice { get; set; } // Price of the product variant at the time of order

    }
}
