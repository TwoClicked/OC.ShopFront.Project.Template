using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;

namespace OC.LUAC.ServiceLayer.Services
{
    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;

        public AddressService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Address>> GetAddressesForCustomerAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Address> AddAddressAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAddressAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId && !a.IsDeleted)
                .ToListAsync();

            foreach (var addr in addresses)
                addr.IsDefault = addr.Id == addressId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null || address.IsDeleted)
                return false;

            address.IsDeleted = true;
            address.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
