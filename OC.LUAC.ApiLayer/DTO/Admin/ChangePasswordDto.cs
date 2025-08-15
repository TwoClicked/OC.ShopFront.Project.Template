using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Admin
{
    public class ChangePasswordDto
    {
        [Required, MinLength(6)] public string NewPassword { get; set; }
    }
}
