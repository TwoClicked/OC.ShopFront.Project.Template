using OC.LUAC.ApiLayer.DTO.Order;

namespace OC.LUAC.ApiLayer.DTO.Product
{
    public class CreateOrderDto
    {
        public int? CustomerId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Language { get; set; } = "en";

        public string ShippingStreet { get; set; }
        public string ShippingNumber { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingCountry { get; set; }

        public List<CreateOrderItemDto> Items { get; set; }

        public string? VoucherCode { get; set; }
    }

}
