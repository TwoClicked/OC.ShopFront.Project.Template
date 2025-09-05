using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OC.LUAC.ServiceLayer.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        // 🗑️ Soft delete (GDPR)
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null || customer.IsDeleted) return false;

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;

            customer.FirstName = "[Deleted]";
            customer.LastName = "[Deleted]";
            customer.Email = $"deleted_{customer.Id}@anon.invalid";
            customer.PasswordHash = HashPassword(Guid.NewGuid().ToString("N"));
            customer.Language = string.Empty;

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

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        // 🔑 Login
        public async Task<Customer?> LoginAsync(string email, string password)
        {
            var user = await _context.Customers
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null || user.IsGuest) return null; // 🚫 block guests

            if (!ValidatePassword(password, user.PasswordHash)) return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        // 🆕 Register or Upgrade
        public async Task<Customer?> RegisterAsync(Customer customer, string plainPassword)
        {
            var email = customer.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) return null;

            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);

            if (existingCustomer != null)
            {
                if (existingCustomer.IsGuest)
                {
                    // 🔄 Upgrade guest to full account
                    existingCustomer.FirstName = customer.FirstName;
                    existingCustomer.LastName = customer.LastName;
                    existingCustomer.Language = customer.Language;
                    existingCustomer.PasswordHash = HashPassword(plainPassword);
                    existingCustomer.IsGuest = false;
                    existingCustomer.LastLoginAt = DateTime.UtcNow;

                    _context.Customers.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    return existingCustomer;
                }

                throw new InvalidOperationException("A customer with this email already exists.");
            }

            // 🆕 New customer
            customer.Email = email;
            customer.PasswordHash = HashPassword(plainPassword);
            customer.IsGuest = false;
            customer.CreatedAt = DateTime.UtcNow;
            customer.LastLoginAt = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        public async Task<Customer> UpdateProfileAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public bool ValidatePassword(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash == storedHash;
        }

        public async Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || customer.IsDeleted || customer.IsGuest) return false;

            if (!ValidatePassword(oldPassword, customer.PasswordHash))
                return false;

            customer.PasswordHash = HashPassword(newPassword);
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        // 📦 Address CRUD
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
            var existing = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == address.Id && !a.IsDeleted);
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
            var existing = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            _context.Addresses.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        // 👮 Admin controls
        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return false;

            customer.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (customer == null) return false;

            customer.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var normalized = email.Trim().ToLowerInvariant();
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == normalized && !c.IsDeleted);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        // 🔑 Reset password (registered users only)
        public async Task<PasswordResetToken?> CreatePasswordResetTokenAsync(string email)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive && !c.IsDeleted && !c.IsGuest);
            if (customer == null) return null;

            var resetToken = new PasswordResetToken
            {
                CustomerId = customer.Id,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return resetToken;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var reset = await _context.PasswordResetTokens
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.Token == token && !t.Used && t.ExpiresAt > DateTime.UtcNow);

            if (reset == null || reset.Customer.IsGuest) return false;

            reset.Used = true;
            reset.Customer.PasswordHash = HashPassword(newPassword);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
