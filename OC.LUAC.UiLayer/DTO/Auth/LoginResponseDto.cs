namespace OC.LUAC.UiLayer.DTO.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public CustomerProfileDto Customer { get; set; } = new();
    }
}
