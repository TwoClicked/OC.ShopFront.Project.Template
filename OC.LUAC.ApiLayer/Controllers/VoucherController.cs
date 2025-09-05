using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OC.LUAC.ApiLayer.DTO.Voucher;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

[ApiController]
[Route("api/vouchers")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _vouchers;

    public VoucherController(IVoucherService vouchers)
    {
        _vouchers = vouchers;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Voucher>>> GetAll()
        => Ok(await _vouchers.GetAllVouchersAsync());

    [HttpGet("active")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Voucher>>> GetActive()
    {
        var all = await _vouchers.GetAllVouchersAsync();
        var now = DateTime.UtcNow;

        var active = all.Where(v => v.IsActive &&
                                    v.StartDate <= now &&
                                    v.EndDate >= now &&
                                    (!v.MaxUsageCount.HasValue || v.CurrentUsageCount < v.MaxUsageCount.Value))
                        .ToList();

        return Ok(active);
    }

    [HttpGet("{code}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Voucher>> GetByCode(string code)
    {
        var voucher = await _vouchers.GetVoucherByCodeAsync(code);
        return voucher == null ? NotFound() : Ok(voucher);
    }

    [HttpGet("id/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Voucher>> GetById(int id)
    {
        var voucher = await _vouchers.GetVoucherByIdAsync(id);
        return voucher == null ? NotFound() : Ok(voucher);
    }

    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> ValidateVoucher(string code)
    {
        var voucher = await _vouchers.GetVoucherByCodeAsync(code);
        if (voucher == null) return NotFound();

        var now = DateTime.UtcNow;
        if (!voucher.IsActive || voucher.StartDate > now || voucher.EndDate < now ||
            (voucher.MaxUsageCount.HasValue && voucher.CurrentUsageCount >= voucher.MaxUsageCount.Value))
        {
            return BadRequest("Voucher is not valid.");
        }

        return Ok(new
        {
            voucher.Code,
            voucher.Percentage,
            voucher.FixedAmount,
            voucher.AppliesToShipping
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Voucher>> Create(CreateVoucherDto dto)
    {
        try
        {
            var existing = await _vouchers.GetVoucherByCodeAsync(dto.Code);
            if (existing != null)
                return BadRequest("A voucher with this code already exists.");

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
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Vouchers_Code") == true)
        {
            return BadRequest("A voucher with this code already exists.");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Voucher>> Update(int id, UpdateVoucherDto dto)
    {
        try
        {
            var existing = await _vouchers.GetVoucherByIdAsync(id);
            if (existing == null) return NotFound();

            if (!string.Equals(existing.Code, dto.Code, StringComparison.OrdinalIgnoreCase))
            {
                var duplicate = await _vouchers.GetVoucherByCodeAsync(dto.Code);
                if (duplicate != null)
                    return BadRequest("A voucher with this code already exists.");

                existing.Code = dto.Code.ToUpperInvariant();
            }

            existing.Percentage = dto.Percentage;
            existing.FixedAmount = dto.FixedAmount;
            existing.StartDate = dto.StartDate;
            existing.EndDate = dto.EndDate;
            existing.IsActive = dto.IsActive;
            existing.MaxUsageCount = dto.MaxUsageCount;
            existing.AppliesToShipping = dto.AppliesToShipping;

            var updated = await _vouchers.UpdateVoucherAsync(existing);
            return Ok(updated);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Vouchers_Code") == true)
        {
            return BadRequest("A voucher with this code already exists (DB constraint).");
        }
    }

    [HttpPut("{id:int}/toggle")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Voucher>> ToggleActive(int id)
    {
        var existing = await _vouchers.GetVoucherByIdAsync(id);
        if (existing == null) return NotFound();

        existing.IsActive = !existing.IsActive;
        var updated = await _vouchers.UpdateVoucherAsync(existing);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _vouchers.DeleteVoucherAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
