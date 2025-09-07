using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OC.LUAC.ApiLayer.DTO.Common;
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

        // Customer starts or reuses a chat session
        [Authorize(Roles = "Customer")]
        public async Task<int> StartSession()
        {
            var customerIdClaim = Context.User?.FindFirst("customerId")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                throw new HubException("No customerId claim found in token.");

            var customerId = int.Parse(customerIdClaim);

            var session = await _chatService.GetOrCreateActiveSessionForCustomerAsync(customerId);

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

            // Broadcast to everyone in this chat session (typed DTO)
            await Clients.Group($"session_{sessionId}")
                .SendAsync("ReceiveMessage", new ChatMessageDto
                {
                    Id = chatMessage.Id,
                    ChatSessionId = chatMessage.ChatSessionId,
                    Message = chatMessage.Message,
                    IsFromCustomer = chatMessage.IsFromCustomer,
                    SentAt = chatMessage.SentAt
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
