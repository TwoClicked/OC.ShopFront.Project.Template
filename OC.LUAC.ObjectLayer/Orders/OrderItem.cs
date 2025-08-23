using OC.LUAC.ObjectLayer.Entities; // for Product, ProductVariant

namespace OC.LUAC.ObjectLayer.Orders
{
    public class OrderItem
    {
        public int Id { get; set; }

        // Foreign key to Order
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }   // ✅ Navigation

        // Snapshot
        public string ProductName { get; set; }

        // Foreign key to ProductVariant
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }  // ✅ Navigation

        // Snapshot
        public string Size { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }
}
