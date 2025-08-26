using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // GET /api/vouchers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Voucher>>> GetAll()
        => Ok(await _vouchers.GetAllVouchersAsync());

    // GET /api/vouchers/active
    [HttpGet("active")]
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

    // GET /api/vouchers/{code}
    [HttpGet("{code}")]
    public async Task<ActionResult<Voucher>> GetByCode(string code)
    {
        var voucher = await _vouchers.GetVoucherByCodeAsync(code);
        return voucher == null ? NotFound() : Ok(voucher);
    }

    // POST /api/vouchers
    [HttpPost]
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
            return CreatedAtAction(nameof(GetByCode), new { code = created.Code }, created);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Vouchers_Code") == true)
        {
            return BadRequest("A voucher with this code already exists.");
        }
    }

    // PUT /api/vouchers/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Voucher>> Update(int id, UpdateVoucherDto dto)
    {
        try
        {
            var existing = await _vouchers.GetVoucherByIdAsync(id);
            if (existing == null) return NotFound();

            // If admin tries to change code, check uniqueness
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

    // PUT /api/vouchers/{id}/toggle
    [HttpPut("{id:int}/toggle")]
    public async Task<ActionResult<Voucher>> ToggleActive(int id)
    {
        var existing = await _vouchers.GetVoucherByIdAsync(id);
        if (existing == null) return NotFound();

        existing.IsActive = !existing.IsActive;
        var updated = await _vouchers.UpdateVoucherAsync(existing);
        return Ok(updated);
    }

    // DELETE /api/vouchers/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _vouchers.DeleteVoucherAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
