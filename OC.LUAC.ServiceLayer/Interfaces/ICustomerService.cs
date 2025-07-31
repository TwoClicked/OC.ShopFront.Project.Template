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
        /// <summary>
        /// Registers a new customer by saving their details and hashing their password.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="plainPassword"></param>
        /// <returns></returns>
        Task<Customer?> RegisterAsync(Customer customer, string plainPassword);
        /// <summary>
        /// Logs in a customer with the provided email and password, validating credentials against stored data.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<Customer?> LoginAsync(string email, string password);
        /// <summary>
        /// Validates a plain-text password against the stored hashed password for a customer.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="storedHash"></param>
        /// <returns></returns>
        bool ValidatePassword(string password, string storedHash);
        /// <summary>
        /// Retrieves a customer by their email address, ensuring the account is not deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Customer?> GetCustomerByIdAsync(int id);
        /// <summary>
        /// Updates a customer's profile information, such as name and address, while ensuring the account is not deleted.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<Customer> UpdateProfileAsync(Customer customer);
        /// <summary>
        /// Deletes a customer account by setting the IsDeleted flag to true and recording the deletion time, ensuring the account is not already deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteCustomerAsync(int id);
    }
}
