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

        public async Task<ChatSession> StartChatSessionAsync(int? customerId, string? guestIdentifier)
        {
            var session = new ChatSession
            {
                CustomerId = customerId,
                GuestIdentifier = guestIdentifier,
                StartedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatMessage> SendMessageAsync(int sessionId, string message, bool isFromCustomer)
        {
            var chatMessage = new ChatMessage
            {
                ChatSessionId = sessionId,
                Message = message,
                IsFromCustomer = isFromCustomer,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetMessagesBySessionIdAsync(int sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<bool> CloseSessionAsync(int sessionId)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session == null || session.IsDeleted)
                return false;

            session.ClosedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChatSession>> GetActiveSessionsAsync()
        {
            return await _context.ChatSessions
                .Include(s => s.Customer)
                .Include(s => s.Messages)
                .Where(s => s.ClosedAt == null && !s.IsDeleted)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get or create an active chat session for a specific customer.
        /// </summary>
        public async Task<ChatSession> GetOrCreateActiveSessionForCustomerAsync(int customerId)
        {
            // Check for an existing active session
            var existingSession = await _context.ChatSessions
                .Where(s => s.CustomerId == customerId && s.ClosedAt == null && !s.IsDeleted)
                .OrderByDescending(s => s.StartedAt)
                .FirstOrDefaultAsync();

            if (existingSession != null)
                return existingSession;

            // Otherwise, create new
            var newSession = new ChatSession
            {
                CustomerId = customerId,
                StartedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(newSession);
            await _context.SaveChangesAsync();
            return newSession;
        }
    }
}
