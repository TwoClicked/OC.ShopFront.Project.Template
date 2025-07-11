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
    /// Service class for managing products in the e-commerce system.
    /// </summary>
    public class ProductService : IProductService
    {

        private readonly AppDbContext _context;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class with the specified database context.
        /// </summary>
        /// <param name="context"></param>
        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all products from the database, including their images and variants.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific product by its ID, including its images and variants.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Product>> GetFeaturedProductsAsync()
        {

            return await _context.Products
                .Include(p => p.Images)
                .Where(p => !p.IsDeleted && p.IsFeatured)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a product by its ID, including its images and variants.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        /// <summary>
        /// Retrieves all products that belong to a specific category, including their images and variants.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<Product> AddProductAsync(Product product)
        {
            // timestamp for creation
            product.CreatedAt = DateTime.Now;

            // add the product to the context
            _context.Products.Add(product);

            // save changes to the database
            await _context.SaveChangesAsync();
            return product;

        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Product> UpdateProductAsync(Product product)
        {
            // Check if the product exists
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found.");
            }

            // Update the existing product
            _context.Products.Update(product);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return product;
        }

        /// <summary>
        /// Soft deletes a product by marking it as deleted without removing it from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> SoftDeleteProductAsync(int id)
        {
            // Find the product by ID
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false; // Product not found
            }

            // Mark the product as deleted
            product.IsDeleted = true;
            // Set the deletion timestamp
            product.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true; // Deletion successful

        }
    }
}
