using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.DTO.Admin;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ITokenService _tokens;

        public AdminAuthController(IConfiguration config, ITokenService tokens)
        {
            _config = config; _tokens = tokens;
        }

        [HttpPost("login")]
        public ActionResult<object> Login([FromBody] AdminLoginDto dto)
        {
            var adminEmail = _config["AdminAuth:Email"];
            var adminPass = _config["AdminAuth:Password"];

            if (string.Equals(dto.Email?.Trim(), adminEmail, StringComparison.OrdinalIgnoreCase)
                && dto.Password == adminPass)
            {
                var token = _tokens.CreateAdminToken(adminEmail);
                return Ok(new { token, role = "Admin" });
            }

            return Unauthorized();
        }
    }
}
