// ServiceLayer/Services/ShippingZoneService.cs
using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;

public class ShippingZoneService : IShippingZoneService
{
    private readonly AppDbContext _context;
    public ShippingZoneService(AppDbContext context) => _context = context;

    public async Task<List<ShippingZone>> GetAllZonesAsync() =>
        await _context.ShippingZones
            .Include(z => z.Countries)
            .OrderBy(z => z.Name)
            .ToListAsync();

    public async Task<ShippingZone?> GetZoneByIdAsync(int id) =>
        await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.Id == id);

    public async Task<ShippingZone?> GetZoneByCountryAsync(string countryCode)
    {
        countryCode = (countryCode ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(countryCode))
            return await _context.ShippingZones
                .Include(z => z.Countries)
                .FirstOrDefaultAsync(z => z.IsDefault);

        var zone = await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.Countries.Any(c => c.CountryCode == countryCode));

        return zone ?? await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.IsDefault);
    }

    public async Task<ShippingZone> CreateZoneAsync(ShippingZone zone)
    {
        // Ensure only one default
        if (zone.IsDefault)
        {
            foreach (var other in _context.ShippingZones.Where(z => z.IsDefault))
                other.IsDefault = false;
        }

        // Normalize country codes
        if (zone.Countries != null)
        {
            foreach (var c in zone.Countries)
                c.CountryCode = (c.CountryCode ?? "").Trim().ToUpperInvariant();
        }

        _context.ShippingZones.Add(zone);
        await _context.SaveChangesAsync();
        return zone;
    }

    public async Task<ShippingZone> UpdateZoneAsync(ShippingZone zone)
    {
        var existing = await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.Id == zone.Id);

        if (existing == null) throw new KeyNotFoundException("Zone not found.");

        existing.Name = zone.Name;
        existing.BaseCost = zone.BaseCost;
        existing.FreeShippingThreshold = zone.FreeShippingThreshold;

        if (zone.IsDefault && !existing.IsDefault)
        {
            foreach (var other in _context.ShippingZones.Where(z => z.IsDefault))
                other.IsDefault = false;
            existing.IsDefault = true;
        }
        else if (!zone.IsDefault && existing.IsDefault)
        {
            existing.IsDefault = false;
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteZoneAsync(int id)
    {
        var zone = await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zone == null) return false;

        _context.ShippingZones.Remove(zone);
        await _context.SaveChangesAsync();
        return true;
    }

    // ✅ replace country mapping atomically (with code + name)
    public async Task SetCountriesAsync(int zoneId, IEnumerable<(string Code, string Name)> countries)
    {
        var zone = await _context.ShippingZones
            .Include(z => z.Countries)
            .FirstOrDefaultAsync(z => z.Id == zoneId);

        if (zone == null) throw new KeyNotFoundException("Zone not found.");

        _context.ShippingZoneCountries.RemoveRange(zone.Countries);
        await _context.SaveChangesAsync();

        foreach (var c in countries.Where(c => !string.IsNullOrWhiteSpace(c.Code)))
        {
            _context.ShippingZoneCountries.Add(new ShippingZoneCountry
            {
                ShippingZoneId = zoneId,
                CountryCode = c.Code.Trim().ToUpperInvariant(),
                CountryName = c.Name?.Trim() ?? string.Empty
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(int zoneId)
    {
        foreach (var z in _context.ShippingZones)
            z.IsDefault = z.Id == zoneId;

        await _context.SaveChangesAsync();
    }
}
