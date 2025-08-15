// OC.LUAC.ApiLayer/Controllers/ProductVariantController.cs
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Product;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/products/{productId:int}/variants")]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _variants;

        public ProductVariantController(IProductVariantService variants)
        {
            _variants = variants;
        }

        // GET /api/products/{productId}/variants
        [HttpGet]
        public async Task<ActionResult<List<ProductVariant>>> GetForProduct(int productId)
        {
            var list = await _variants.GetVariantsByProductIdAsync(productId);
            return Ok(list);
        }

        // POST /api/products/{productId}/variants
        [HttpPost]
        public async Task<ActionResult<ProductVariant>> Create(int productId, [FromBody] CreateProductVariantDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var variant = new ProductVariant
            {
                ProductId = productId,
                Size = dto.Size,
                Stock = dto.Stock
            };

            var created = await _variants.AddVariantAsync(variant);
            // We return collection as location target; alternatively expose GetById.
            return CreatedAtAction(nameof(GetForProduct), new { productId }, created);
        }

        // PUT /api/products/{productId}/variants/{variantId}
        [HttpPut("{variantId:int}")]
        public async Task<ActionResult<ProductVariant>> Update(int productId, int variantId, [FromBody] UpdateProductVariantDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Get all variants for the product
            var variants = await _variants.GetVariantsByProductIdAsync(productId);

            // Find the one we want
            var existing = variants.FirstOrDefault(v => v.Id == variantId);
            if (existing == null)
                return NotFound();

            // Update fields
            existing.Size = dto.Size;
            existing.Stock = dto.Stock;

            var updated = await _variants.UpdateVariantAsync(existing);
            return Ok(updated);
        }

        // DELETE /api/products/{productId}/variants/{variantId}
        [HttpDelete("{variantId:int}")]
        public async Task<IActionResult> SoftDelete(int productId, int variantId)
        {
            var ok = await _variants.SoftDeleteVariatAsync(variantId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
