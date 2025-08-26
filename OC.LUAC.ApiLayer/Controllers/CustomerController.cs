using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.Auth;
using OC.LUAC.ApiLayer.DTO.Customer;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

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

    // =========================
    // AUTH
    // =========================

    [HttpPost("register")]
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

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var customer = await _customers.LoginAsync(dto.Email, dto.Password);
        if (customer == null) return Unauthorized();

        var token = _tokens.CreateCustomerToken(customer);
        return Ok(new LoginResponseDto { Token = token, Customer = customer });
    }

    // =========================
    // ADMIN ENDPOINTS
    // =========================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _customers.GetCustomerByIdAsync(id);
        return customer == null ? NotFound() : Ok(customer);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _customers.DeleteCustomerAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<List<Order>>> GetOrdersForCustomer(int id)
    {
        var orders = await _orders.GetOrdersByCustomerIdAsync(id);
        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateCustomer(int id)
    {
        var ok = await _customers.DeactivateCustomerAsync(id);
        return ok ? Ok(new { status = "Deactivated" }) : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/reactivate")]
    public async Task<IActionResult> ReactivateCustomer(int id)
    {
        var ok = await _customers.ReactivateCustomerAsync(id);
        return ok ? Ok(new { status = "Reactivated" }) : NotFound();
    }

    // =========================
    // SELF-SERVICE ENDPOINTS
    // =========================

    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<ActionResult<CustomerProfileDto>> GetProfile()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var customer = await _customers.GetCustomerByIdAsync(customerId);
        if (customer == null) return NotFound();

        return new CustomerProfileDto
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Language = customer.Language,
            CreatedAt = customer.CreatedAt
        };
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("me")]
    public async Task<ActionResult<Customer>> UpdateProfile([FromBody] UpdateCustomerDto dto)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var existing = await _customers.GetCustomerByIdAsync(customerId);
        if (existing == null) return NotFound();

        existing.FirstName = dto.FirstName ?? existing.FirstName;
        existing.LastName = dto.LastName ?? existing.LastName;
        existing.Language = dto.Language ?? existing.Language;

        var updated = await _customers.UpdateProfileAsync(existing);
        return Ok(updated);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var success = await _customers.ChangePasswordAsync(customerId, dto.OldPassword, dto.NewPassword);
        return success ? Ok(new { status = "PasswordChanged" }) : BadRequest("Invalid old password");
    }

    [Authorize(Roles = "Customer")]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var success = await _customers.DeleteCustomerAsync(customerId);
        return success ? Ok(new { status = "Deleted" }) : NotFound();
    }

    // =========================
    // SELF-SERVICE: ADDRESSES
    // =========================

    [Authorize(Roles = "Customer")]
    [HttpGet("me/addresses")]
    public async Task<ActionResult<List<Address>>> GetAddresses()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var addresses = await _customers.GetAddressesByCustomerIdAsync(customerId);
        return Ok(addresses);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("me/addresses")]
    public async Task<ActionResult<Address>> AddAddress([FromBody] Address address)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim.Value, out var customerId))
            return Unauthorized();

        var added = await _customers.AddAddressAsync(customerId, address);
        return Ok(added);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("me/addresses/{id:int}")]
    public async Task<ActionResult<Address>> UpdateAddress(int id, [FromBody] Address address)
    {
        address.Id = id;
        var updated = await _customers.UpdateAddressAsync(address);
        return updated == null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = "Customer")]
    [HttpDelete("me/addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var success = await _customers.DeleteAddressAsync(id);
        return success ? Ok(new { status = "Deleted" }) : NotFound();
    }
}
