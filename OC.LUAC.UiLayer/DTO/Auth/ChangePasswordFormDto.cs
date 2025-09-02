namespace OC.LUAC.UiLayer.DTO.Auth
{
    public class ChangePasswordFormDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword {  get; set; } = string.Empty;
    }
}