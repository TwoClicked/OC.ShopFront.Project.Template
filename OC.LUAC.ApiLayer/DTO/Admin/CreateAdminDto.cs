using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Admin
{
    public class CreateAdminDto
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MinLength(6)] public string Password { get; set; }
        public string? Role { get; set; } = "Admin"; // optional, defaults to Admin
    }
}
