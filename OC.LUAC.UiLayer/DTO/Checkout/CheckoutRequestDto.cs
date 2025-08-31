// UiLayer/DTO/Checkout/CheckoutRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.UiLayer.DTO.Checkout;

public class CheckoutRequestDto
{
    [Required] public AddressDto Shipping { get; set; } = new();
    public AddressDto? Billing { get; set; } // optional if "same as shipping"
    [Required] public List<CheckoutItemDto> Items { get; set; } = new();
}
