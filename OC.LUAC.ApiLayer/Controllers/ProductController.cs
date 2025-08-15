using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ServiceLayer.Interfaces;
using OC.LUAC.ApiLayer.DTO.Product;
using OC.LUAC.ObjectLayer.Entities;
using Microsoft.AspNetCore.Authorization;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _products;

        public ProductController(IProductService products)
        {
            _products = products;
        }

        // GET /api/products
        [HttpGet]
        public async Task<ActionResult<List<ObjectLayer.Entities.Product>>> GetAll()
        {
            var products = await _products.GetAllProductsAsync();
            return Ok(products);
        }

        // GET /api/products/featured
        [HttpGet("featured")]
        public async Task<ActionResult<List<ObjectLayer.Entities.Product>>> GetFeatured()
        {
            var featured = await _products.GetFeaturedProductsAsync();
            return Ok(featured);
        }

        // GET /api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ObjectLayer.Entities.Product>> GetById(int id)
        {
            var product = await _products.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // GET /api/products/by-category/{categoryId}
        [HttpGet("by-category/{categoryId:int}")]
        public async Task<ActionResult<List<ObjectLayer.Entities.Product>>> GetByCategory(int categoryId)
        {
            var products = await _products.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        // POST /api/products
        [HttpPost]
        [ProducesResponseType(typeof(ObjectLayer.Entities.Product), 201)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ObjectLayer.Entities.Product>> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var product = new ObjectLayer.Entities.Product
            {
                Name_en = dto.Name_en,
                Name_de = dto.Name_de,
                Description_en = dto.Description_en,
                Description_de = dto.Description_de,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                IsFeatured = dto.IsFeatured
            };

            var created = await _products.AddProductAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/products/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ObjectLayer.Entities.Product), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ObjectLayer.Entities.Product>> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existing = await _products.GetProductByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name_en = dto.Name_en;
            existing.Name_de = dto.Name_de;
            existing.Description_en = dto.Description_en;
            existing.Description_de = dto.Description_de;
            existing.Price = dto.Price;
            existing.CategoryId = dto.CategoryId;
            existing.IsFeatured = dto.IsFeatured;

            var updated = await _products.UpdateProductAsync(existing);
            return Ok(updated);
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var result = await _products.SoftDeleteProductAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
