using OC.LUAC.ObjectLayer.Chat;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Starts a new chat session for a customer or guest.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="guestIdentifier"></param>
        /// <returns></returns>
        Task<ChatSession> StartChatSessionAsync(int? customerId, string? guestIdentifier);

        /// <summary>
        /// Sends a message in an existing chat session, indicating whether the message is from the customer or the support agent (Admin).
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="message"></param>
        /// <param name="isFromCustomer"></param>
        /// <returns></returns>
        Task<ChatMessage> SendMessageAsync(int sessionId, string message, bool isFromCustomer);

        /// <summary>
        /// Retrieves all messages for a specific chat session by its ID.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<List<ChatMessage>> GetMessagesBySessionIdAsync(int sessionId);

        /// <summary>
        /// Closes a chat session by its ID, marking it as closed and preventing further messages from being sent in that session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<bool> CloseSessionAsync(int sessionId);

        /// <summary>
        /// Retrieves a list of all active chat sessions, which are sessions that have not been closed yet.
        /// </summary>
        /// <returns></returns>
        Task<List<ChatSession>> GetActiveSessionsAsync();
    }
}
