using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    public class AdminUserService : IAdminUserService
    {

        private readonly AppDbContext _context;

        public AdminUserService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Logs in an admin user with the provided email and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<AdminUser?> LoginAsync(string email, string password)
        {
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null || !ValidatePassword(password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            return user;
        }

        /// <summary>
        /// Validates a plain-text password against the stored hashed password.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="storedHash"></param>
        /// <returns></returns>
        public bool ValidatePassword(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hashed = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hashed == storedHash;
        }

        public async Task UpdateLastLoginAsync(int adminId)
        {
            var user = await _context.AdminUsers.FindAsync(adminId);
            if (user == null || user.IsDeleted)
            {
                throw new Exception("Admin user not found or deleted");
            }

            user.LastLoginAt = DateTime.Now;
            _context.AdminUsers.Update(user);
        }
    }
}
