using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Common;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Security.Claims;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // -------------------------
        // 🔹 Admin-only endpoints
        // -------------------------

        /// <summary>
        /// Get all active chat sessions (not closed).
        /// </summary>
        [HttpGet("sessions/active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _chatService.GetActiveSessionsAsync();

            var dtoList = sessions.Select(s =>
            {
                var lastMessage = s.Messages != null
                    ? s.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
                    : null;

                return new ChatSessionDto
                {
                    Id = s.Id,
                    CustomerId = s.CustomerId,
                    StartedAt = s.StartedAt,
                    ClosedAt = s.ClosedAt,
                    CustomerFirstName = s.Customer?.FirstName ?? "",
                    CustomerLastName = s.Customer?.LastName ?? "",
                    CustomerEmail = s.Customer?.Email ?? "",
                    LastMessageFromCustomer = lastMessage?.IsFromCustomer ?? true
                };
            }).ToList();

            return Ok(dtoList);
        }

        /// <summary>
        /// Get details of a single chat session by ID.
        /// </summary>
        [HttpGet("sessions/{sessionId:int}/details")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var sessions = await _chatService.GetActiveSessionsAsync();
            var session = sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { error = "Chat session not found." });

            var lastMessage = session.Messages != null
                ? session.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
                : null;

            var dto = new ChatSessionDto
            {
                Id = session.Id,
                CustomerId = session.CustomerId,
                StartedAt = session.StartedAt,
                ClosedAt = session.ClosedAt,
                CustomerFirstName = session.Customer?.FirstName ?? "",
                CustomerLastName = session.Customer?.LastName ?? "",
                CustomerEmail = session.Customer?.Email ?? "",
                LastMessageFromCustomer = lastMessage?.IsFromCustomer ?? true
            };

            return Ok(dto);
        }

        /// <summary>
        /// Get all messages for a specific chat session (Admin).
        /// </summary>
        [HttpGet("sessions/{sessionId:int}/messages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMessagesForSession(int sessionId)
        {
            var messages = await _chatService.GetMessagesBySessionIdAsync(sessionId);
            if (messages == null)
                return NotFound(new { error = "Chat session not found or no messages." });

            return Ok(messages);
        }

        /// <summary>
        /// Close a chat session.
        /// </summary>
        [HttpPost("sessions/{sessionId:int}/close")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var success = await _chatService.CloseSessionAsync(sessionId);
            return success
                ? Ok(new { status = "Closed", sessionId })
                : NotFound(new { error = "Chat session not found or already closed." });
        }

        // -------------------------
        // 🔹 Customer endpoints
        // -------------------------

        /// <summary>
        /// Get the current customer's active session (creates one if none exists).
        /// </summary>
        [HttpGet("sessions/me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMySession()
        {
            var customerIdClaim = User.FindFirstValue("customerId");
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { error = "CustomerId missing in token." });

            var customerId = int.Parse(customerIdClaim);

            var existingSession = (await _chatService.GetActiveSessionsAsync())
                .FirstOrDefault(s => s.CustomerId == customerId);

            if (existingSession != null)
                return Ok(existingSession);

            var newSession = await _chatService.GetOrCreateActiveSessionForCustomerAsync(customerId);
            return Ok(newSession);
        }

        /// <summary>
        /// Get all messages for the current customer's active session.
        /// </summary>
        [HttpGet("sessions/me/messages")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyMessages()
        {
            var customerIdClaim = User.FindFirstValue("customerId");
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { error = "CustomerId missing in token." });

            var customerId = int.Parse(customerIdClaim);

            var session = (await _chatService.GetActiveSessionsAsync())
                .FirstOrDefault(s => s.CustomerId == customerId);

            if (session == null)
                return NotFound(new { error = "No active session found." });

            var messages = await _chatService.GetMessagesBySessionIdAsync(session.Id);
            return Ok(messages);
        }
    }
}
