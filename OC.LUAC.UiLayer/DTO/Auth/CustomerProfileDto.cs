namespace OC.LUAC.UiLayer.DTO.Auth
{
    public class CustomerProfileDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public DateTime CreatedAt { get; set; }
    }
}
