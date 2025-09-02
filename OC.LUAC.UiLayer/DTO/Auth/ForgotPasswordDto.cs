using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.UiLayer.DTO.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
