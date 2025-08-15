using OC.LUAC.ObjectLayer.Accounts;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IAdminUserService
    {
        // Queries
        Task<IEnumerable<AdminUser>> GetAllAsync(bool includeInactive = false);
        Task<AdminUser?> GetByIdAsync(int id);

        // Auth
        Task<AdminUser?> LoginAsync(string email, string password);
        bool ValidatePassword(string password, string storedHash);
        Task UpdateLastLoginAsync(int adminId);

        // Management
        Task<AdminUser> CreateAsync(AdminUser admin, string password);
        Task<bool> UpdatePasswordAsync(int id, string newPassword);
        Task<bool> DeactivateAsync(int id);
        Task<bool> ReactivateAsync(int id);   // optional, handy later
    }
}
