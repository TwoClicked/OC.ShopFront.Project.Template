using OC.LUAC.ObjectLayer.Entities;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    /// <summary>
    /// Service interface for managing products in the e-commerce system.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Product> AddProductAsync(Product product);
        /// <summary>
        /// Retrieves all products from the database, including their images and variants.
        /// </summary>
        /// <returns></returns>
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Retrieves featured products from the database, including their images and variants.
        /// </summary>
        /// <returns></returns>
        Task<List<Product>> GetFeaturedProductsAsync();
        /// <summary>
        /// Retrieves a specific product by its ID, including its images and variants.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product?> GetProductByIdAsync(int id);
        /// <summary>
        /// Retrieves products by category ID, including their images and variants.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(string? term, int? categoryId, bool? featured, string sort, bool desc, int page, int pageSize);

        /// <summary>
        /// Soft deletes a product by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SoftDeleteProductAsync(int id);
        /// <summary>
        /// Updates an existing product in the database, including its images and variants.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Product> UpdateProductAsync(Product product);
    }
}