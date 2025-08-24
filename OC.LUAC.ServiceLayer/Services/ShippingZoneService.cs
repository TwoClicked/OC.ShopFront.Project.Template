using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Orders;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    public class ShippingZoneService : IShippingZoneService
    {

        private readonly AppDbContext _context;


        public ShippingZoneService(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Creates a Shipping fee zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public async Task<ShippingZone> CreateZoneAsync(ShippingZone zone)
        {
            _context.ShippingZones.Add(zone);
            await _context.SaveChangesAsync();
            return zone;
        }

        /// <summary>
        /// Deletes a Zone(Admin)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteZoneAsync(int id)
        {
            var zone = await _context.ShippingZones.FindAsync(id);
            if (zone == null) return false;

            _context.ShippingZones.Remove(zone);
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Get all zones in DB
        /// </summary>
        /// <returns></returns>
        public async Task<List<ShippingZone>> GetAllZonesAsync()
        {
            return await _context.ShippingZones
            .Include(z => z.Countries)
            .ToListAsync();
        }
        /// <summary>
        /// Get Zone by country 
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ShippingZone?> GetZoneByCountryAsync(string countryCode)
        {
            countryCode = countryCode.ToUpperInvariant();

            // First look for a zone that explicitlyu has the country
            var zone = await _context.ShippingZoneCountries
                .Include(c => c.ShippingZone)
                .Where(c => c.CountryCode == countryCode)
                .Select(c => c.ShippingZone)
                .FirstOrDefaultAsync();

            if (zone != null)
            {
                return zone;
            }

            // Fallback: default zone
            return await _context.ShippingZones
                .FirstOrDefaultAsync(z => z.IsDefault);
        }

        /// <summary>
        /// Get zone by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ShippingZone?> GetZoneByIdAsync(int id)
        {
            return await _context.ShippingZones
                .Include(z => z.Countries)
                .FirstOrDefaultAsync(z => z.Id == id);
        }


        /// <summary>
        /// Updates a current zone
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public async Task<ShippingZone> UpdateZoneAsync(ShippingZone zone)
        {
            _context.ShippingZones.Update(zone);
            await _context.SaveChangesAsync();
            return zone;
        }
    }
}
