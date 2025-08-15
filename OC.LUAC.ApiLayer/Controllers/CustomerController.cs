using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.DTO.Customer;
using OC.LUAC.ObjectLayer.Accounts;   // Customer
using OC.LUAC.ObjectLayer.Orders;     // Order
using OC.LUAC.ServiceLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customers;
        private readonly IOrderService _orders;
        private readonly ITokenService _tokens;

        public CustomerController(ICustomerService customers, IOrderService orders, ITokenService tokens)
        {
            _customers = customers;
            _orders = orders;
            _tokens = tokens;
        }

        // POST /api/customers/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(Customer), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Customer>> Register([FromBody] RegisterCustomerDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var customer = new Customer
            {
                Email = dto.Email.Trim(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Language = dto.Language
            };

            var created = await _customers.RegisterAsync(customer, dto.Password);
            if (created == null) return BadRequest("Registration failed.");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // POST /api/customers/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var customer = await _customers.LoginAsync(dto.Email, dto.Password);
            if (customer == null) return Unauthorized();

            var token = _tokens.CreateCustomerToken(customer);
            return Ok(new LoginResponseDto { Token = token, Customer = customer });
        }

        // GET /api/customers/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _customers.GetCustomerByIdAsync(id);
            return customer == null ? NotFound() : Ok(customer);
        }

        // PUT /api/customers
        [HttpPut]
        [ProducesResponseType(typeof(Customer), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Customer>> Update([FromBody] UpdateCustomerDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existing = await _customers.GetCustomerByIdAsync(dto.Id);
            if (existing == null) return NotFound();

            existing.FirstName = dto.FirstName ?? existing.FirstName;
            existing.LastName = dto.LastName ?? existing.LastName;
            existing.Language = dto.Language ?? existing.Language;

            var updated = await _customers.UpdateProfileAsync(existing);
            return Ok(updated);
        }

        // DELETE /api/customers/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _customers.DeleteCustomerAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // GET /api/customers/{id}/orders
        [HttpGet("{id:int}/orders")]
        [ProducesResponseType(typeof(List<Order>), 200)]
        public async Task<ActionResult<List<Order>>> GetOrdersForCustomer(int id)
        {
            var orders = await _orders.GetOrdersByCustomerIdAsync(id);
            return Ok(orders);
        }
    }
}
