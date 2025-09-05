using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Shipping;
using OC.LUAC.ApiLayer.DTO.ShippingZone;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

[ApiController]
[Route("api/shipping-zones")]
[Authorize(Roles = "Admin")]
public class ShippingZoneController : ControllerBase
{
    private readonly IShippingZoneService _zones;
    public ShippingZoneController(IShippingZoneService zones) => _zones = zones;

    [HttpGet]
    public async Task<ActionResult<List<ShippingZoneDto>>> GetAll()
    {
        var zones = await _zones.GetAllZonesAsync();
        return Ok(zones.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShippingZoneDto>> GetById(int id)
    {
        var z = await _zones.GetZoneByIdAsync(id);
        return z == null ? NotFound() : Ok(Map(z));
    }

    [HttpPost]
    public async Task<ActionResult<ShippingZoneDto>> Create([FromBody] CreateShippingZoneDto dto)
    {
        var zone = new ShippingZone
        {
            Name = dto.Name,
            BaseCost = dto.BaseCost,
            FreeShippingThreshold = dto.FreeShippingThreshold,
            IsDefault = dto.IsDefault,
            Countries = dto.Countries
                .Select(c => new ShippingZoneCountry
                {
                    CountryCode = c.Code,
                    CountryName = c.Name
                })
                .ToList()
        };

        var created = await _zones.CreateZoneAsync(zone);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShippingZoneDto>> Update(int id, [FromBody] UpdateShippingZoneDto dto)
    {
        if (id != dto.Id) return BadRequest("Mismatched ID");

        // update main fields
        var updated = await _zones.UpdateZoneAsync(new ShippingZone
        {
            Id = dto.Id,
            Name = dto.Name,
            BaseCost = dto.BaseCost,
            FreeShippingThreshold = dto.FreeShippingThreshold,
            IsDefault = dto.IsDefault
        });

        // replace countries via service (normalized + atomic)
        await _zones.SetCountriesAsync(id, dto.Countries.Select(c => (c.Code, c.Name)));

        // reload for return
        var withCountries = await _zones.GetZoneByIdAsync(id);
        return Ok(Map(withCountries!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _zones.DeleteZoneAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // OPTIONAL helpers for admin UI:
    [HttpPut("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _zones.SetDefaultAsync(id);
        return NoContent();
    }

    [HttpPut("{id:int}/countries")]
    public async Task<IActionResult> SetCountries(int id, [FromBody] List<CountryDto> countries)
    {
        await _zones.SetCountriesAsync(id, countries.Select(c => (c.Code, c.Name)));
        return NoContent();
    }

    // PUBLIC quote endpoint for checkout UI
    [AllowAnonymous]
    [HttpGet("quote")]
    public async Task<ActionResult<ShippingQuoteDto>> Quote([FromQuery] string country, [FromQuery] decimal subtotal)
    {
        var zone = await _zones.GetZoneByCountryAsync(country);
        if (zone == null) return BadRequest("Region not supported.");

        var isFree = subtotal >= zone.FreeShippingThreshold;
        return Ok(new ShippingQuoteDto
        {
            ZoneId = zone.Id,
            ZoneName = zone.Name,
            BaseCost = zone.BaseCost,
            FreeShippingThreshold = zone.FreeShippingThreshold,
            IsFree = isFree,
            ShippingCost = isFree ? 0 : zone.BaseCost,
            Subtotal = subtotal
        });
    }

    [AllowAnonymous]
    [HttpGet("countries")]
    public async Task<ActionResult<List<CountryDto>>> GetAllCountries()
    {
        var zones = await _zones.GetAllZonesAsync();

        var countries = zones
            .SelectMany(z => z.Countries.Select(c => new CountryDto
            {
                Code = c.CountryCode,
                Name = c.CountryName
            }))
            .DistinctBy(c => c.Code)
            .OrderBy(c => c.Name)
            .ToList();

        return Ok(countries);
    }

    private static ShippingZoneDto Map(ShippingZone z) => new ShippingZoneDto
    {
        Id = z.Id,
        Name = z.Name,
        BaseCost = z.BaseCost,
        FreeShippingThreshold = z.FreeShippingThreshold,
        IsDefault = z.IsDefault,
        Countries = z.Countries
            .Select(c => new CountryDto
            {
                Code = c.CountryCode,
                Name = c.CountryName
            })
            .OrderBy(c => c.Name)
            .ToList()
    };
}
