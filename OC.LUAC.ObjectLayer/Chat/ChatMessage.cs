using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Chat
{
    public class ChatMessage
    {
        // Primary key
        public int id { get; set; }


        public int ChatSessionid { get; set; } // Foreign key to ChatSession
        public ChatSession ChatSession { get; set; } // Navigation property to ChatSession

        public bool IsFromCustomer { get; set; } // Indicates if the message is from the customer (true) or the support agent(Admin) (false)

        public string Message { get; set; } // The content of the chat message
        public DateTime SentAt { get; set; } = DateTime.Now; // Timestamp when the message was sent
    }
}
