using OC.LUAC.ObjectLayer.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IAdminUserService
    {
        /// <summary>
        /// Logs in an admin user with the provided email and password.
        /// </summary>
        /// <param name="email">The admin's email address.</param>
        /// <param name="password">The plain-text password to validate.</param>
        /// <returns>The matching AdminUser if credentials are valid; otherwise, null.</returns>
        Task<AdminUser?> LoginAsync(string email, string password);

        /// <summary>
        /// Validates a plain-text password against the stored hashed password.
        /// </summary>
        /// <param name="password">The plain-text password entered by the user.</param>
        /// <param name="storedHash">The hashed password stored in the database.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        bool ValidatePassword(string password, string storedHash);

        /// <summary>
        /// Updates the last login timestamp for the given admin user.
        /// </summary>
        /// <param name="adminId">The ID of the admin user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateLastLoginAsync(int adminId);
    }
}
