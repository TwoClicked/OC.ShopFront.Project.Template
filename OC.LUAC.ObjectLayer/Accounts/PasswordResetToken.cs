using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Accounts
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;

        // Navigation
        public Customer Customer { get; set; }
    }
}
