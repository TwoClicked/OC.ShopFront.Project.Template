using OC.LUAC.ObjectLayer.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer?> RegisterAsync(Customer customer, string plainPassword);
        Task<Customer?> LoginAsync(string email, string password);
        bool ValidatePassword(string password, string storedHash);
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<Customer> UpdateProfileAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword);

        Task<List<Address>> GetAddressesByCustomerIdAsync(int customerId);
        Task<Address?> AddAddressAsync(int customerId, Address address);
        Task<Address?> UpdateAddressAsync(Address address);
        Task<bool> DeleteAddressAsync(int addressId);

        Task<bool> DeactivateCustomerAsync(int id);
        Task<bool> ReactivateCustomerAsync(int id);
    }
}
