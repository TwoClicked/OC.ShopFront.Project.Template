using OC.LUAC.ObjectLayer.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Entities
{

    // Represents the type of stock action performed on a product variant
    public enum StockActionType
    {
        Add,
        Reduce,
        Sale,
        Cancel
    }
    public class StockAction
    {

        // Primary key
        public int id { get; set; }

        // Foreign key to ProductVariant
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        //Type of action: "add", "reduce", "sale", "cancel"
        public StockActionType ActionType { get; set; }

        // Quantity of stock affected by this action
        public int Quantity { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now; // Timestamp for when the action was performed

        // Optional : link to an order if stock was changed by a purchase or cancellation
        public int? OrderId { get; set; } // Nullable in case the action is not related to an order
        public Order? Order { get; set; } // Navigation property to the related order, if applicable

        // Soft delete support (optional - mostly for manual corrections) 

        public bool IsDeleted { get; set; } = false; // Flag to indicate if the action is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp for when the action was deleted, if applicable
    }
}
