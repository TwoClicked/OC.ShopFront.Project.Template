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
        /// Soft delete (GDPR)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null || customer.IsDeleted) return false;

            // Mark & timestamp
            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;

            // Scrub PII on Customer (use unique email to satisfy unique index)
            customer.FirstName = "[Deleted]";
            customer.LastName = "[Deleted]";
            customer.Email = $"deleted_{customer.Id}@anon.invalid";
            customer.PasswordHash = HashPassword(Guid.NewGuid().ToString("N"));           // Safe NULL
            customer.Language = string.Empty;   // or leave as-is

            // Scrub Addresses (keep FK, but clear personal fields)
            if (customer.Addresses != null)
            {
                foreach (var a in customer.Addresses.Where(a => !a.IsDeleted))
                {
                    a.IsDeleted = true;
                    a.DeletedAt = DateTime.UtcNow;
                    a.Label = "[Deleted]";
                    a.Street = string.Empty;
                    a.Number = string.Empty;
                    a.PostalCode = string.Empty;
                    a.City = string.Empty;
                    a.Country = string.Empty;
                }
            }

            await _context.SaveChangesAsync();
            return true;
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
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive);

            if (user == null || !ValidatePassword(password, user.PasswordHash))
            {
                return null;
            }

            user.LastLoginAt = DateTime.UtcNow; // use UTC for consistency
            await _context.SaveChangesAsync();  // persist the change

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
            // Normalize & basic checks
            var email = customer.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) return null;

            var exists = await _context.Customers
                .AnyAsync(c => c.Email == email && !c.IsDeleted);
            if (exists)
            {
                throw new InvalidOperationException("A customer with this email already exists.");
            }

            // Hash password (use your existing hashing util)
            var hash = HashPassword(plainPassword); // your method
            customer.Email = email;
            customer.PasswordHash = hash;

            // Timestamps
            customer.CreatedAt = DateTime.UtcNow;
            customer.LastLoginAt = DateTime.UtcNow;   // <= consider them “logged in” on registration

            // Persist
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

        public async Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.IsDeleted) return false;

            if (!ValidatePassword(oldPassword, customer.PasswordHash))
                return false;

            customer.PasswordHash = HashPassword(newPassword);
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ Address CRUD
        public async Task<List<Address>> GetAddressesByCustomerIdAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Address?> AddAddressAsync(int customerId, Address address)
        {
            address.CustomerId = customerId;
            address.CreatedAt = DateTime.UtcNow;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address?> UpdateAddressAsync(Address address)
        {
            var existing = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id && !a.IsDeleted);
            if (existing == null) return null;

            existing.Label = address.Label ?? existing.Label;
            existing.Street = address.Street ?? existing.Street;
            existing.Number = address.Number ?? existing.Number;
            existing.PostalCode = address.PostalCode ?? existing.PostalCode;
            existing.City = address.City ?? existing.City;
            existing.Country = address.Country ?? existing.Country;

            _context.Addresses.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var existing = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            _context.Addresses.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Admin controls
        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return false;

            customer.IsActive = false; // add IsActive column if not there
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateCustomerAsync(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return false;

            customer.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalized = email.Trim().ToLowerInvariant();
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == normalized && !c.IsDeleted);
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
