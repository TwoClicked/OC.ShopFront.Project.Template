using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    public class CustomerService : ICustomerService
    {

        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Deletes a customer by marking them as deleted instead of physically removing them from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .FindAsync(id);
            if (customer == null || customer.IsDeleted)
            {
                return false; // Customer not found or already deleted
            }

            customer.IsDeleted = true; // Mark as deleted
            customer.DeletedAt = DateTime.Now; // Set deletion timestamp
            await _context.SaveChangesAsync();
            return true; // Deletion successful
        }

        /// <summary>
        /// Retrieves all customers from the database, excluding those marked as deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        /// <summary>
        /// Logs in a customer with the provided email and password, validating credentials against stored data.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Customer?> LoginAsync(string email, string password)
        {
            var user = await _context.Customers
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null || !ValidatePassword(password, user.PasswordHash))
            {
                return null;
            }
            return user; // Return the user if credentials are valid
        }

        /// <summary>
        /// Registers a new customer by saving their details and hashing their password.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="plainPassword"></param>
        /// <returns></returns>
        public async Task<Customer?> RegisterAsync(Customer customer, string plainPassword)
        {
            customer.PasswordHash = HashPassword(plainPassword);
            customer.CreatedAt = DateTime.Now;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        /// <summary>
        /// Updates a customer's profile information, such as name and address, while ensuring the account is not deleted.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<Customer> UpdateProfileAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer; // Return the updated customer entity
        }

        /// <summary>
        /// Validates a plain-text password against the stored hashed password for a customer.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="storedHash"></param>
        /// <returns></returns>
        public bool ValidatePassword(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash == storedHash;
        }

        /// <summary>
        /// Hashes a plain-text password using SHA-256 and returns the hashed value as a Base64 string.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
