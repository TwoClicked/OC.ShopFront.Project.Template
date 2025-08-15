using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OC.LUAC.ApiLayer.DTO.Admin;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _admins;

        public AdminUsersController(IAdminUserService admins)
        {
            _admins = admins;
        }



        // GET /api/admin/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var list = await _admins.GetAllAsync(); // active admins by default
            return Ok(list.Select(a => new
            {
                a.Id,
                a.Email,
                a.Role,
                a.IsActive,
                a.CreatedAt,
                a.LastLoginAt
            }));
        }

        // POST /api/admin/users
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] CreateAdminDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var admin = new AdminUser
            {
                Email = dto.Email.Trim(),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Admin" : dto.Role.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _admins.CreateAsync(admin, dto.Password);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, new { created.Id, created.Email, created.Role });
        }

        // PUT /api/admin/users/{id}/password
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var ok = await _admins.UpdatePasswordAsync(id, dto.NewPassword);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /api/admin/users/{id}  (soft-deactivate)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _admins.DeactivateAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
