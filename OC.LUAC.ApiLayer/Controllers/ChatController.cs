using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize(Roles = "Admin")]
    public class ChatController : ControllerBase
    {

        private readonly IChatService _chatService;

        public ChatController(IChatService chatservice)
        {
            _chatService = chatservice;
        }

        [HttpGet("sessions/active")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _chatService.GetActiveSessionsAsync();
            return Ok(sessions);
        }

        [HttpGet("sessions/{sessionId:int}")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var messages = await _chatService.GetMessagesBySessionIdAsync(sessionId);
            return Ok(messages);
        }

        [HttpPost("sessions/{sessionId:int}/close")]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var success = await _chatService.CloseSessionAsync(sessionId);
            return success ? Ok(new { status = "Closed" }) : NotFound();
        }
    }
}
