using OC.LUAC.ObjectLayer.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Orders
{
    /// <summary>
    /// Represents the status of an order. Enum for OrderStatus.
    /// </summary>
    public enum OrderStatus
    {
        PendingPayment, // Waiting for bank transfer
        Processing, // Payment confirmed by admin, ready to process and ship
        Shipped, // Shipped after payment
        Cancelled // Cancelled by User or admin
    }
    /// <summary>
    /// Represents an order in the system.
    /// </summary>
    public class Order
    {

        // Primary key
        public int Id { get; set; }

        // OrderNumber is a unique identifier for the order
        public string OrderNumber { get; set; }

        // customer
        public int? CustomerId { get; set; } // It will be null if the user does not register/login and proceeds as Guest
        public Customer? Customer { get; set; }
        public string Language { get; set; } // Language preference for the order (e.g., "en", "de")

        // Shipping details
        public string ShippingStreet { get; set; } // Street address for shipping
        public string ShippingNumber { get; set; } // House number for shipping
        public string ShippingPostalCode { get; set; } // Postal code for shipping
        public string ShippingCity { get; set; } // City for shipping
        public string ShippingCountry { get; set; } // Country for shipping


        //Order Status
        public OrderStatus Status { get; set; } = OrderStatus.PendingPayment; // Default status is Pending payment

        // Tracking Info
        public string? TrackingNumber { get; set; } // Tracking number for the shipment
        public string? TrackingUrl { get; set; } // URL to track the shipment

        // creation timestamp
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Navigation properties

        public ICollection<OrderItem> Items { get; set; } // Collection of items in the order

        // Soft Delete (For admin cleanup)
        public bool IsDeleted { get; set; } = false; // Indicates if the order is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp when the order was deleted


        // DISCOUNT & VOUCHER

        public decimal? DiscountAmount { get; set; }      // how much discount was applied
        public decimal TotalBeforeDiscount { get; set; }  // original subtotal
        public decimal TotalAfterDiscount { get; set; }   // discounted total
        public string? VoucherCode { get; set; }          // applied voucher


        // SHIPPING COST

        public decimal ShippingCost { get; set; } // Final shipping fee
        public bool IsFreeShipping { get; set; } // True if treshold or voucher is applied

    }
}
