using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Orders
{
    public class OrderItem
    {

        //primary key
        public int id { get; set; }

        //Foreign key to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }


        public int ProductId { get; set; } // Foreign key to Product
        public string ProductName { get; set; } // Snapshot of the product name at the time of order


        public int productVariantId { get; set; } // Foreign key to ProductVariant
        public string Size { get; set; } // Size of the product variant

        public int Quantity { get; set; } // Quantity of this product variant in the order
        public decimal UnitPrice { get; set; } // Price of the product variant at the time of order

        public decimal Total => UnitPrice * Quantity; // Total price for this order item (calculated property) Won't be stored in the database 

    }
}
