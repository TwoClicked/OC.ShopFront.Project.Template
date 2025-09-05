using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ProductVariant> AddVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            return variant; // Return the added variant with its ID populated
        }

        /// <summary>
        /// Adjusts stock for a variant by changing its quantity.
        /// </summary>
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
        /// Retrieves all non-deleted product variants, including their parent Product.
        /// </summary>
        public async Task<List<ProductVariant>> GetAllVariantsAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product) // ✅ Include product details
                .Where(v => !v.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a variant by ID, including its parent Product.
        /// </summary>
        public async Task<ProductVariant?> GetVariantByIdAsync(int id)
        {
            return await _context.ProductVariants
                .Include(v => v.Product) // ✅ Include product details
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        /// <summary>
        /// Retrieves all variants for a given product, including product details.
        /// </summary>
        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Include(v => v.Product) // ✅ Include product details
                .Where(v => v.ProductId == productId && !v.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Soft deletes a product variant by setting its IsDeleted flag.
        /// </summary>
        public async Task<bool> SoftDeleteVariatAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null || variant.IsDeleted)
            {
                return false; // Variant not found or already deleted
            }

            variant.IsDeleted = true;
            variant.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true; // Soft delete successful
        }

        /// <summary>
        /// Updates an existing product variant.
        /// </summary>
        public async Task<ProductVariant> UpdateVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync();
            return variant; // Return the updated variant
        }
    }
}
