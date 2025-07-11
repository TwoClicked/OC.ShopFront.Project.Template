using OC.LUAC.ObjectLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    /// <summary>
    /// Service interface for managing categories in the e-commerce system.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories from the database, including their subcategories.
        /// </summary>
        /// <returns></returns>
        Task<List<Category>> GetAllCategoriesAsync();
        /// <summary>
        /// Retrieves a specific category by its ID, including its subcategories.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category?> GetCategoryByIdAsync(int id);
        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<Category> AddCategoryAsync(Category category);
        /// <summary>
        /// Updates an existing category in the database, including its subcategories.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<Category> UpdateCategoryAsync(Category category);
        /// <summary>
        /// Soft deletes a category by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SoftDeleteCategoryAsync(int id);
    }
}
