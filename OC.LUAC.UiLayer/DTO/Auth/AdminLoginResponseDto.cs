namespace OC.LUAC.UiLayer.DTO.Auth
{
    public class AdminLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public AdminUserDto? Admin { get; set; }
    }

    public class AdminUserDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
    }
}
