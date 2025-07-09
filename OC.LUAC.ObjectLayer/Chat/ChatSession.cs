using OC.LUAC.ObjectLayer.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Chat
{
    /// <summary>
    /// Represents a chat session in the system.
    /// </summary>
    public class ChatSession
    {

        //primary key
        public int Id { get; set; }

        //Foreign key to Customer

        public int? CustomerId { get; set; } // Nullable for guest users
        public Customer? Customer { get; set; } // Navigation property for Customer

        public string? GuestIdentifier { get; set; } // Unique identifier for guest users (Cookie or session ID)

        public DateTime StartedAt { get; set; } // Timestamp when the chat session started
        public DateTime? ClosedAt { get; set; } // Timestamp when the chat session was closed, nullable if still open

        // navigation properties

        public ICollection<ChatMessage> Messages { get; set; } // Collection of messages in the chat session

        // Soft Delete

        public bool IsDeleted { get; set; } = false; // Indicates if the chat session is deleted
        public DateTime? DeletedAt { get; set; } // Timestamp when the chat session was deleted, nullable if not deleted

    }
}
