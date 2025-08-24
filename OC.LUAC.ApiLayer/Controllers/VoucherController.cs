using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Voucher;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

[ApiController]
[Route("api/vouchers")]
[Authorize(Roles = "Admin")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _vouchers;

    public VoucherController(IVoucherService vouchers)
    {
        _vouchers = vouchers;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Voucher>>> GetAll()
        => Ok(await _vouchers.GetAllVouchersAsync());

    [HttpGet("{code}")]
    public async Task<ActionResult<Voucher>> GetByCode(string code)
    {
        var voucher = await _vouchers.GetVoucherByCodeAsync(code);
        return voucher == null ? NotFound() : Ok(voucher);
    }

    [HttpPost]
    public async Task<ActionResult<Voucher>> Create(CreateVoucherDto dto)
    {
        var voucher = new Voucher
        {
            Code = dto.Code.ToUpperInvariant(),
            Percentage = dto.Percentage,
            FixedAmount = dto.FixedAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            MaxUsageCount = dto.MaxUsageCount,
            AppliesToShipping = dto.AppliesToShipping
        };

        var created = await _vouchers.CreateVoucherAsync(voucher);
        return CreatedAtAction(nameof(GetByCode), new { code = created.Code }, created);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _vouchers.DeleteVoucherAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
