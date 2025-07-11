using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    /// <summary>
    /// Service class for managing categories in the e-commerce system.
    /// </summary>
    public class CategoryService : ICategoryService
    {

        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {

            _context = context;
            
        }

        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Retrieves all categories from the database, including their subcategories.
        /// </summary>
        /// <returns></returns>
        public Task<List<Category>> GetAllCategoriesAsync()
        {
            return _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync(); 
        }

        /// <summary>
        /// Retrieves a specific category by its ID, including its subcategories.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        /// <summary>
        /// Soft deletes a category by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                return false; // Category not found or already deleted
            }

            // soft delete the category
            category.IsDeleted = true;
            // Record the deletion time
            category.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true; // Soft delete successful
        }

        /// <summary>
        /// Updates an existing category in the database, including its subcategories.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }
    }
}
