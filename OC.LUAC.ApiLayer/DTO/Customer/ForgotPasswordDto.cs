using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Customer
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
