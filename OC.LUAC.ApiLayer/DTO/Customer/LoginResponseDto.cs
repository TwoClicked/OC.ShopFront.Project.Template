using CustomerEntity = OC.LUAC.ObjectLayer.Accounts.Customer;

namespace OC.LUAC.ApiLayer.DTO.Customer
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Role { get; set; } = "Customer";
        public CustomerEntity Customer { get; set; }
    }
}
