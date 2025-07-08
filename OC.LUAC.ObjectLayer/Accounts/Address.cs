using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Accounts
{
    public class Address
    {

        // primary key
        public int id { get; set; }

        // Foreign key to Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public string Label { get; set; } // e.g., "Home", "Work", etc.

        public string Street { get; set; } // Street address
        public string Number { get; set; } // House number
        public string PostalCode { get; set; } // Postal code
        public string City { get; set; } // City
        public string Country { get; set; } // Country

        public bool IsDefault { get; set; } = false; // Indicates if this is the default address for the customer

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp when the address was created

        //soft delete support
        public bool IsDeleted { get; set; } = false; // Indicates if the address is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp when the address was deleted, nullable if not deleted
    }
}
