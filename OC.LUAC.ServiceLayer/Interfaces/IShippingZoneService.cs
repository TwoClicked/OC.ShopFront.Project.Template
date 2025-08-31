using OC.LUAC.ObjectLayer.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task SetCountriesAsync(int zoneId, IEnumerable<string> countryCodes);
        Task SetDefaultAsync(int zoneId);
    }
}
