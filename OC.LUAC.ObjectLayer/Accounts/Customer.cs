using OC.LUAC.ObjectLayer.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Accounts
{
    /// <summary>
    /// Represents a customer in the system.
    /// </summary>
    public class Customer
    {

        // Primary key

        public int Id { get; set; }


        public string Email { get; set; } // Email of the customer
        public string PasswordHash { get; set; } // Hashed password for security

        public string FirstName { get; set; } // First name of the customer
        public string LastName { get; set; } // Last name of the customer

        public string Language { get; set; } // Preferred language of the customer (e.g., "en", "de")

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp for when the customer was created
        public DateTime? LastLoginAt { get; set; } // Timestamp for the last login, nullable if never logged in

        // GDPR Soft delete support

        public bool IsDeleted { get; set; } = false; // Flag to indicate if the customer is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp for when the customer was deleted, nullable if not deleted

        // Navigation properties

        public ICollection<Order> Orders { get; set; } // Collection of orders associated with the customer
        public ICollection<Address>? Addresses { get; set; } // Collection of addresses associated with the customer, nullable if no addresses are present

    }
}
