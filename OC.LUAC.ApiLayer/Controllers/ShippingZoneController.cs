using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Shipping;
using OC.LUAC.ApiLayer.DTO.ShippingZone;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/shipping-zones")]
    [Authorize(Roles = "Admin")]
    public class ShippingZoneController : ControllerBase
    {
        private readonly IShippingZoneService _zones;

        public ShippingZoneController(IShippingZoneService zones)
        {
            _zones = zones;
        }

        // GET: api/shipping-zones
        [HttpGet]
        public async Task<ActionResult<List<ShippingZoneDto>>> GetAll()
        {
            var zones = await _zones.GetAllZonesAsync();

            return Ok(zones.Select(z => new ShippingZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                BaseCost = z.BaseCost,
                FreeShippingThreshold = z.FreeShippingThreshold,
                IsDefault = z.IsDefault,
                Countries = z.Countries.Select(c => c.CountryCode).ToList()
            }));
        }

        // GET: api/shipping-zones/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShippingZoneDto>> GetById(int id)
        {
            var zone = await _zones.GetZoneByIdAsync(id);
            if (zone == null) return NotFound();

            return Ok(new ShippingZoneDto
            {
                Id = zone.Id,
                Name = zone.Name,
                BaseCost = zone.BaseCost,
                FreeShippingThreshold = zone.FreeShippingThreshold,
                IsDefault = zone.IsDefault,
                Countries = zone.Countries.Select(c => c.CountryCode).ToList()
            });
        }

        // POST: api/shipping-zones
        [HttpPost]
        public async Task<ActionResult<ShippingZoneDto>> Create([FromBody] CreateShippingZoneDto dto)
        {
            var zone = new ShippingZone
            {
                Name = dto.Name,
                BaseCost = dto.BaseCost,
                FreeShippingThreshold = dto.FreeShippingThreshold,
                IsDefault = dto.IsDefault,
                Countries = dto.Countries.Select(c => new ShippingZoneCountry
                {
                    CountryCode = c
                }).ToList()
            };

            var created = await _zones.CreateZoneAsync(zone);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ShippingZoneDto
            {
                Id = created.Id,
                Name = created.Name,
                BaseCost = created.BaseCost,
                FreeShippingThreshold = created.FreeShippingThreshold,
                IsDefault = created.IsDefault,
                Countries = created.Countries.Select(c => c.CountryCode).ToList()
            });
        }

        // PUT: api/shipping-zones/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ShippingZoneDto>> Update(int id, [FromBody] UpdateShippingZoneDto dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched ID");

            var existing = await _zones.GetZoneByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.BaseCost = dto.BaseCost;
            existing.FreeShippingThreshold = dto.FreeShippingThreshold;
            existing.IsDefault = dto.IsDefault;

            // Replace countries
            existing.Countries = dto.Countries.Select(c => new ShippingZoneCountry
            {
                CountryCode = c,
                ShippingZoneId = id
            }).ToList();

            var updated = await _zones.UpdateZoneAsync(existing);

            return Ok(new ShippingZoneDto
            {
                Id = updated.Id,
                Name = updated.Name,
                BaseCost = updated.BaseCost,
                FreeShippingThreshold = updated.FreeShippingThreshold,
                IsDefault = updated.IsDefault,
                Countries = updated.Countries.Select(c => c.CountryCode).ToList()
            });
        }

        // DELETE: api/shipping-zones/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _zones.DeleteZoneAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
