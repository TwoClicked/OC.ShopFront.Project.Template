using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer
{
    public class EmaiLSettings
    {

        public string Host { get; set; } // SMTP server host
        public int Port { get; set; } // SMTP server port
        public string Username { get; set; } // SMTP server username
        public string Password { get; set; } // SMTP server password
        public string FromEmail { get; set; } // Email address from which emails are sent

    }
}
