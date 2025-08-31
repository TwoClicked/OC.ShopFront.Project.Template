// UiLayer/DTO/Checkout/AddressDto.cs
using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.UiLayer.DTO.Checkout
{
    public class AddressDto
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = "";
        [Required, MaxLength(50)] public string LastName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";

        [Required, MaxLength(80)] public string Line1 { get; set; } = "";
        [MaxLength(80)] public string? Line2 { get; set; }
        [Required, MaxLength(60)] public string City { get; set; } = "";
        [Required, MaxLength(20)] public string PostalCode { get; set; } = "";
        [Required, MaxLength(60)] public string Country { get; set; } = "";
    }
}
