using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Order
{
    public class CreateOrderDto
    {
        // Logged-in path (or admin): provide an existing customer
        public int? CustomerId { get; set; }

        // Guest path: if CustomerId is null, we'll create a minimal customer
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string Language { get; set; } = "en";

        // Shipping (required for both)
        [Required] public string ShippingStreet { get; set; }
        [Required] public string ShippingNumber { get; set; }
        [Required] public string ShippingPostalCode { get; set; }
        [Required] public string ShippingCity { get; set; }
        [Required] public string ShippingCountry { get; set; }

        [Required] public List<CreateOrderItemDto> Items { get; set; }
    }
}
