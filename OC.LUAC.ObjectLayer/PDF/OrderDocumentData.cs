using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.PDF
{
    /// <summary>
    /// Represents the data required to generate an order document, including order details, customer information, and line items.
    /// </summary>
    public class OrderDocumentData
    {

        public string OrderNumber { get; set; } // Unique identifier for the order
        public string CustomerName { get; set; } // Name of the customer
        public string CustomerEmail { get; set; } // Email address of the customer
        public string Language { get; set; } // Language preference for the document

        public List<OrderLineItem> Items { get; set; } // List of items in the order
        public decimal TotalAmount { get; set; } // Total amount for the order
        public DateTime OrderDate { get; set; } // Date when the order was placed

    }
}
