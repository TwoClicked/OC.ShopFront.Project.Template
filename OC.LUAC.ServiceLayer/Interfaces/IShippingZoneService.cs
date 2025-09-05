using OC.LUAC.ObjectLayer.Orders;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IShippingZoneService
    {
        Task<List<ShippingZone>> GetAllZonesAsync();
        Task<ShippingZone?> GetZoneByIdAsync(int id);
        Task<ShippingZone?> GetZoneByCountryAsync(string country);
        Task<ShippingZone> CreateZoneAsync(ShippingZone zone);
        Task<ShippingZone> UpdateZoneAsync(ShippingZone zone);
        Task<bool> DeleteZoneAsync(int id);

        // CHANGED: accept CountryDto so we store both Code and Name
        Task SetCountriesAsync(int zoneId, IEnumerable<(string Code, string Name)> countries);

        Task SetDefaultAsync(int zoneId);
    }
}
