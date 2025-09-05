using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize(Roles = "Admin")] // only admins can manage sessions
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Get all active chat sessions (not closed).
        /// </summary>
        [HttpGet("sessions/active")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _chatService.GetActiveSessionsAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Get details of a single chat session by ID.
        /// </summary>
        [HttpGet("sessions/{sessionId:int}/details")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var sessions = await _chatService.GetActiveSessionsAsync();
            var session = sessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
                return NotFound(new { error = "Chat session not found." });

            return Ok(session);
        }

        /// <summary>
        /// Get all messages for a given session.
        /// </summary>
        [HttpGet("sessions/{sessionId:int}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var messages = await _chatService.GetMessagesBySessionIdAsync(sessionId);
            if (messages == null || !messages.Any())
                return NotFound(new { error = "No messages found for this session." });

            return Ok(messages);
        }

        /// <summary>
        /// Close a chat session.
        /// </summary>
        [HttpPost("sessions/{sessionId:int}/close")]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var success = await _chatService.CloseSessionAsync(sessionId);
            return success
                ? Ok(new { status = "Closed", sessionId })
                : NotFound(new { error = "Chat session not found or already closed." });
        }
    }
}
