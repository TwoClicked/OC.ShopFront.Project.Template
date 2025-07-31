using OC.LUAC.ObjectLayer.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    /// <summary>
    /// Service interface for managing addresses in the e-commerce system.
    /// </summary>
    public interface IAddressService
    {
        /// <summary>
        /// Retrieves all addresses associated with a specific customer ID.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<List<Address>> GetAddressesForCustomerAsync(int customerId);

        /// <summary>
        /// Retrieves a specific address by its ID, including its details.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<Address> AddAddressAsync(Address address);

        /// <summary>
        /// Updates an existing address in the database, including its details.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<Address> UpdateAddressAsync(Address address);

        /// <summary>
        /// Sets a specific address as the default address for a customer by updating the IsDefault flag.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        Task<bool> SetDefaultAddressAsync(int customerId, int addressId);

        /// <summary>
        /// Soft deletes an address by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="addressId"></param>
        /// <returns></returns>
        Task<bool> DeleteAddressAsync(int addressId);

    }
}
