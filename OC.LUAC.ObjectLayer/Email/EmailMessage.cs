using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Email
{
    public class EmailMessage
    {

        public string To { get; set; } // Recipient email address
        public string Subject { get; set; } // Subject of the email
        public string HtmlBody { get; set; } // Body of the email

        public byte[]? Attachment { get; set; } // Optional attachment as byte array
        public string? AttachmentFileName { get; set; } // Name of the attachment file
    }
}
