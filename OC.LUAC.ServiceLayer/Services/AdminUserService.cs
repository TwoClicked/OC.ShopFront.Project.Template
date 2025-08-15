using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;                       // your DbContext
using OC.LUAC.ObjectLayer.Accounts;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace OC.LUAC.ServiceLayer.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;
        public AdminUserService(AppDbContext context) => _context = context;

        // --- helpers ---
        private static string Hash(string input)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(input)));
        }

        // --- queries ---
        public async Task<IEnumerable<AdminUser>> GetAllAsync(bool includeInactive = false)
        {
            var q = _context.AdminUsers.AsNoTracking().Where(a => !a.IsDeleted);
            if (!includeInactive) q = q.Where(a => a.IsActive);
            return await q.OrderBy(a => a.Email).ToListAsync();
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
            => await _context.AdminUsers.AsNoTracking()
                   .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        // --- auth ---
        public async Task<AdminUser?> LoginAsync(string email, string password)
        {
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive);

            if (user == null || !ValidatePassword(password, user.PasswordHash))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();   // persist
            return user;
        }

        public bool ValidatePassword(string password, string storedHash)
            => Hash(password) == storedHash;

        public async Task UpdateLastLoginAsync(int adminId)
        {
            var user = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == adminId && !a.IsDeleted);
            if (user == null) throw new InvalidOperationException("Admin user not found");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();   // persist
        }

        // --- management ---
        public async Task<AdminUser> CreateAsync(AdminUser admin, string password)
        {
            // basic uniqueness check (consider unique index on Email too)
            var exists = await _context.AdminUsers.AnyAsync(a => a.Email == admin.Email && !a.IsDeleted);
            if (exists) throw new InvalidOperationException("Admin with this email already exists.");

            admin.PasswordHash = Hash(password);
            admin.IsActive = true;
            admin.CreatedAt = DateTime.UtcNow;

            _context.AdminUsers.Add(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<bool> UpdatePasswordAsync(int id, string newPassword)
        {
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            if (admin == null) return false;

            admin.PasswordHash = Hash(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            if (admin == null) return false;

            admin.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateAsync(int id)
        {
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            if (admin == null) return false;

            admin.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
