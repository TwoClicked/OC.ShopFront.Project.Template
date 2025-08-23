using OC.LUAC.ObjectLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IProductVariantService
    {
        /// <summary>
        /// Retrieves all product variants associated with a specific product ID.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId);
        /// <summary>
        /// Retrieves a specific product variant by its ID.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        Task<ProductVariant> AddVariantAsync(ProductVariant variant);
        /// <summary>
        /// Updates an existing product variant in the database.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        Task<ProductVariant> UpdateVariantAsync(ProductVariant variant);
        /// <summary>
        /// Adjusts the stock quantity of a product variant by a specified amount.
        /// </summary>
        /// <param name="variantId"></param>
        /// <param name="quantityChange"></param>
        /// <returns></returns>
        Task<bool> AdjustStockAsync(int variantId, int quantityChange);
        /// <summary>
        /// Soft deletes a product variant by setting its IsDeleted flag to true and recording the deletion time.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> SoftDeleteVariatAsync(int id);

        Task<ProductVariant?> GetVariantByIdAsync(int id);
        Task<List<ProductVariant>> GetAllVariantsAsync();
    }
}
