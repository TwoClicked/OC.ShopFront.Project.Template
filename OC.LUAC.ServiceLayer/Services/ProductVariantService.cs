using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
    /// Service class for managing product variants in the e-commerce system.
    /// </summary>
    public class ProductVariantService : IProductVariantService
    {

        private readonly AppDbContext _context;

        public ProductVariantService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new product variant to the database.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public async Task<ProductVariant> AddVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant; // Return the added variant with its ID populated
        }

        /// <summary>
        /// Updates an existing product variant in the database.
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="quantityChange"></param>
        /// <returns></returns>
        public async Task<bool> AdjustStockAsync(int variantId, int quantityChange)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant == null || variant.IsDeleted)
            {
                return false; // Variant not found or deleted
            }

            // Adjust the stock quantity
            variant.Stock += quantityChange;
            await _context.SaveChangesAsync();
            return true; // Stock adjustment successful
        }

        /// <summary>
        /// Retrieves all product variants associated with a specific product ID.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId && !v.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Soft deletes a product variant by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteVariatAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null || variant.IsDeleted)
            {
                return false; // Variant not found or already deleted
            }

            variant.IsDeleted = true; // Mark as deleted
            variant.DeletedAt = DateTime.UtcNow; // Set deletion time
            await _context.SaveChangesAsync();
            return true; // Soft delete successful
        }

        /// <summary>
        /// Updates an existing product variant in the database.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public async Task<ProductVariant> UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
            return variant; // Return the updated variant
        }
    }
}
