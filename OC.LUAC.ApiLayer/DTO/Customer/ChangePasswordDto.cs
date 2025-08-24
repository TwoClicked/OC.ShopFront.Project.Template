using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Customer
{
    public class ChangePasswordDto
    {
        [Required] public string OldPassword { get; set; }
        [Required, MinLength(6)] public string NewPassword { get; set; }
    }
}
