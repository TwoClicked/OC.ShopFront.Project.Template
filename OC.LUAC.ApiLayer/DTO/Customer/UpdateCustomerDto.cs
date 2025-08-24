using System.ComponentModel.DataAnnotations;

namespace OC.LUAC.ApiLayer.DTO.Customer
{
    public class UpdateCustomerDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Language { get; set; }
    }
}
