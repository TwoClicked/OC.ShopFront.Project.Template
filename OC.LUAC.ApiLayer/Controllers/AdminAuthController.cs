using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.DTO.Admin;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminUserService _admins;
        private readonly ITokenService _tokens;

        public AdminAuthController(IAdminUserService admins, ITokenService tokens)
        {
            _admins = admins;
            _tokens = tokens;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var admin = await _admins.LoginAsync(dto.Email, dto.Password);
            if (admin == null || !admin.IsActive)
                return Unauthorized();

            // Create JWT for admin
            var token = _tokens.CreateAdminToken(admin.Email);

            return Ok(new
            {
                token,
                role = "Admin",
                admin = new
                {
                    admin.Id,
                    admin.Email
                }
            });
        }
    }
}
