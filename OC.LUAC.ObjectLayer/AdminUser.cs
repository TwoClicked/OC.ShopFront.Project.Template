using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer
{
    public class AdminUser
    {

        // Primary key
        public int id { get; set; }

        public string Email { get; set; } // Email of the admin user
        public string PasswordHash { get; set; } // Hashed password for security

        public string Role { get; set; } // Role of the admin user (e.g., "SuperAdmin", "Admin", etc.)

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp for when the admin user was created
        public DateTime? LastLoginAt { get; set; } // Timestamp for the last login, nullable if never logged in

        public bool IsActive { get; set; } = true; // Indicates if the admin user is active

        //Soft delete support
        public bool IsDeleted { get; set; } = false; // Indicates if the admin user is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp when the admin user was deleted, nullable if not deleted

    }
}
