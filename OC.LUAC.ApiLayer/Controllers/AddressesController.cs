using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Adress;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Security.Claims;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addresses;
        private readonly ICustomerService _customers;

        public AddressesController(IAddressService addresses, ICustomerService customers)
        {
            _addresses = addresses;
            _customers = customers;
        }

        // --- helper: get logged-in Customer entity ---
        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var claim = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (claim == null) return null;

            if (!int.TryParse(claim.Value, out var customerId))
                return null;

            return await _customers.GetCustomerByIdAsync(customerId);
        }

        // -------------------------------------------------
        // CUSTOMER ENDPOINTS
        // -------------------------------------------------

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return Unauthorized();

            var list = await _addresses.GetAddressesForCustomerAsync(customer.Id);
            return Ok(list.Select(MapToDto).ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<AddressDto>> AddAddress([FromBody] CreateAddressDto dto)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return Unauthorized();

            var address = new Address
            {
                CustomerId = customer.Id,
                Label = dto.Label,
                Street = dto.Street,
                Number = dto.Number,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Country = dto.Country
            };

            var created = await _addresses.AddAddressAsync(address);
            return CreatedAtAction(nameof(GetMyAddresses), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<AddressDto>> UpdateAddress(int id, [FromBody] CreateAddressDto dto)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return Unauthorized();

            var address = new Address
            {
                Id = id,
                CustomerId = customer.Id,
                Label = dto.Label,
                Street = dto.Street,
                Number = dto.Number,
                PostalCode = dto.PostalCode,
                City = dto.City,
                Country = dto.Country
            };

            var updated = await _addresses.UpdateAddressAsync(address);
            return Ok(MapToDto(updated));
        }

        [HttpPut("{id:int}/default")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return Unauthorized();

            var ok = await _addresses.SetDefaultAddressAsync(customer.Id, id);
            return ok ? Ok(new { id, status = "DefaultSet" }) : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _addresses.DeleteAddressAsync(id);
            return ok ? Ok(new { id, status = "Deleted" }) : NotFound();
        }

        // -------------------------------------------------
        // ADMIN ENDPOINTS
        // -------------------------------------------------

        // GET /api/addresses/customer/{customerId}
        [HttpGet("customer/{customerId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddressesForCustomer(int customerId)
        {
            var customer = await _customers.GetCustomerByIdAsync(customerId);
            if (customer == null) return NotFound($"Customer {customerId} not found.");

            var list = await _addresses.GetAddressesForCustomerAsync(customer.Id);
            return Ok(list.Select(MapToDto).ToList());
        }

        // --- mapper ---
        private static AddressDto MapToDto(Address a) => new AddressDto
        {
            Id = a.Id,
            Label = a.Label,
            Street = a.Street,
            Number = a.Number,
            PostalCode = a.PostalCode,
            City = a.City,
            Country = a.Country,
            IsDefault = a.IsDefault
        };
    }
}
