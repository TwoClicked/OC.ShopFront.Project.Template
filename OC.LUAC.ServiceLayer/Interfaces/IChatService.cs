using OC.LUAC.ObjectLayer.Chat;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IChatService
    {
        Task<ChatSession> StartChatSessionAsync(int? customerId, string? guestIdentifier);
        Task<ChatMessage> SendMessageAsync(int sessionId, string message, bool isFromCustomer);
        Task<List<ChatMessage>> GetMessagesBySessionIdAsync(int sessionId);
        Task<bool> CloseSessionAsync(int sessionId);
        Task<List<ChatSession>> GetActiveSessionsAsync();

        /// <summary>
        /// Get or create an active chat session for a specific customer.
        /// </summary>
        Task<ChatSession> GetOrCreateActiveSessionForCustomerAsync(int customerId);
    }
}
