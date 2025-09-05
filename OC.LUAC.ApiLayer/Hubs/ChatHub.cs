using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OC.LUAC.ObjectLayer.Chat;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Hubs
{
    [Authorize] // only authenticated users
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Customer starts a chat session
        [Authorize(Roles = "Customer")]
        public async Task<int> StartSession()
        {
            var customerId = int.Parse(Context.User!.FindFirst("id")!.Value);

            var session = await _chatService.StartChatSessionAsync(customerId, null);

            // put customer in their session group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{session.Id}");

            return session.Id;
        }

        // Both customer & admin send messages
        [Authorize(Roles = "Customer,Admin")]
        public async Task SendMessage(int sessionId, string message)
        {
            var isCustomer = Context.User!.IsInRole("Customer");

            var chatMessage = await _chatService.SendMessageAsync(sessionId, message, isCustomer);

            // Broadcast to everyone in this chat session
            await Clients.Group($"session_{sessionId}")
                .SendAsync("ReceiveMessage", new
                {
                    chatMessage.Id,
                    chatMessage.ChatSessionId,
                    chatMessage.Message,
                    chatMessage.IsFromCustomer,
                    chatMessage.SentAt
                });
        }

        // Admin joins an existing session
        [Authorize(Roles = "Admin")]
        public async Task JoinSession(int sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }

        // Admin closes a session (and notify customer)
        [Authorize(Roles = "Admin")]
        public async Task CloseSession(int sessionId)
        {
            var ok = await _chatService.CloseSessionAsync(sessionId);
            if (ok)
            {
                await Clients.Group($"session_{sessionId}")
                    .SendAsync("SessionClosed", sessionId);
            }
        }
    }
}
