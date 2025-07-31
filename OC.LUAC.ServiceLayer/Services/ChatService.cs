using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Chat;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ServiceLayer.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Starts a new chat session for a customer or guest.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="guestIdentifier"></param>
        /// <returns></returns>
        public async Task<ChatSession> StartChatSessionAsync(int? customerId, string? guestIdentifier)
        {
            var session = new ChatSession
            {
                CustomerId = customerId,
                GuestIdentifier = guestIdentifier,
                StartedAt = DateTime.Now
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        /// <summary>
        /// Sends a message in an existing chat session, indicating whether the message is from the customer or the support agent (Admin).
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="message"></param>
        /// <param name="isFromCustomer"></param>
        /// <returns></returns>
        public async Task<ChatMessage> SendMessageAsync(int sessionId, string message, bool isFromCustomer)
        {
            var chatMessage = new ChatMessage
            {
                ChatSessionId = sessionId,
                Message = message,
                IsFromCustomer = isFromCustomer,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        /// <summary>
        /// Retrieves all messages for a specific chat session by its ID.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(int sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        /// <summary>
        /// Closes a chat session by its ID, marking it as closed and preventing further messages from being sent in that session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<bool> CloseSessionAsync(int sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null || session.IsDeleted)
                return false;

            session.ClosedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a list of all active chat sessions, which are sessions that have not been closed yet.
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChatSession>> GetActiveSessionsAsync()
        {
            return await _context.ChatSessions
                .Where(s => s.ClosedAt == null && !s.IsDeleted)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();
        }
    }
}
