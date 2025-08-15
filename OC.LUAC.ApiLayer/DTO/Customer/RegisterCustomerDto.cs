using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Customer
{
    public class RegisterCustomerDto
    {

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language {  get; set; } = "en";

    }
}
