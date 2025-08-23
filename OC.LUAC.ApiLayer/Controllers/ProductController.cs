// OC.LUAC.ApiLayer/Controllers/ProductController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OC.LUAC.ApiLayer.DTO.Common;
using OC.LUAC.ApiLayer.DTO.Product;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // --------------------------------------------------
        // PUBLIC READ ENDPOINTS (storefront)
        // --------------------------------------------------

        /// <summary>
        /// Browse products (paged list for grids/search/category pages).
        /// Query: q, categoryId, featured, sort(price|name|name_de|newest), dir(asc|desc), page, pageSize
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductSummaryDto>>> Browse([FromQuery] BrowseQuery q)
        {
            var sort = string.IsNullOrWhiteSpace(q.Sort) ? "newest" : q.Sort!;
            var desc = string.IsNullOrWhiteSpace(q.Dir) || q.Dir!.Equals("desc", StringComparison.OrdinalIgnoreCase);

            var (entities, total) = await _products.SearchAsync(
                q.Q, q.CategoryId, q.Featured, sort, desc, q.Page, q.PageSize);

            var items = new List<ProductSummaryDto>(entities.Count);

            foreach (var p in entities)
            {
                var firstImg = p.Images?.OrderBy(i => i.SortOrder).FirstOrDefault();
                items.Add(new ProductSummaryDto
                {
                    Id = p.Id,
                    Name_en = p.Name_en,
                    Name_de = p.Name_de,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName_en = p.Category?.Name_en ?? string.Empty,
                    CategoryName_de = p.Category?.Name_de,
                    ThumbnailUrl = firstImg?.ImageUrl,
                    IsFeatured = p.IsFeatured,
                    VariantCount = p.Variants?.Count ?? 0,
                    TotalStock = p.Variants?.Sum(v => v.Stock) ?? 0
                });
            }

            return Ok(new PagedResult<ProductSummaryDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                Total = total,
                Items = items
            });
        }

        /// <summary>
        /// Product details (full data for PDP: images + variants).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDetailsDto>> Details(int id)
        {
            var p = await _products.GetByIdWithDetailsAsync(id);
            if (p == null) return NotFound();

            var dto = new ProductDetailsDto
            {
                Id = p.Id,
                Name_en = p.Name_en,
                Name_de = p.Name_de,
                Description_en = p.Description_en,
                Description_de = p.Description_de,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName_en = p.Category?.Name_en ?? string.Empty,
                CategoryName_de = p.Category?.Name_de,
                IsFeatured = p.IsFeatured,
                Images = p.Images?
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponseDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ImageUrl = i.ImageUrl,
                        SortOrder = i.SortOrder
                    }).ToList() ?? new List<ProductImageResponseDto>(),
                Variants = p.Variants?
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock
                    }).ToList() ?? new List<ProductVariantDto>()
            };

            return Ok(dto);
        }

        /// <summary>
        /// Featured products (lightweight list).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("featured")]
        public async Task<ActionResult<List<ProductSummaryDto>>> GetFeatured()
        {
            var featured = await _products.GetFeaturedProductsAsync();

            var list = featured.Select(p =>
            {
                var firstImg = p.Images?.OrderBy(i => i.SortOrder).FirstOrDefault();
                return new ProductSummaryDto
                {
                    Id = p.Id,
                    Name_en = p.Name_en,
                    Name_de = p.Name_de,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName_en = p.Category?.Name_en ?? string.Empty,
                    CategoryName_de = p.Category?.Name_de,
                    ThumbnailUrl = firstImg?.ImageUrl,
                    IsFeatured = p.IsFeatured,
                    VariantCount = p.Variants?.Count ?? 0,
                    TotalStock = p.Variants?.Sum(v => v.Stock) ?? 0
                };
            }).ToList();

            return Ok(list);
        }

        /// <summary>
        /// Products for a category (lightweight list).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("by-category/{categoryId:int}")]
        public async Task<ActionResult<List<ProductSummaryDto>>> GetByCategory(int categoryId)
        {
            var products = await _products.GetProductsByCategoryAsync(categoryId);

            var list = products.Select(p =>
            {
                var firstImg = p.Images?.OrderBy(i => i.SortOrder).FirstOrDefault();
                return new ProductSummaryDto
                {
                    Id = p.Id,
                    Name_en = p.Name_en,
                    Name_de = p.Name_de,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName_en = p.Category?.Name_en ?? string.Empty,
                    CategoryName_de = p.Category?.Name_de,
                    ThumbnailUrl = firstImg?.ImageUrl,
                    IsFeatured = p.IsFeatured,
                    VariantCount = p.Variants?.Count ?? 0,
                    TotalStock = p.Variants?.Sum(v => v.Stock) ?? 0
                };
            }).ToList();

            return Ok(list);
        }

        // --------------------------------------------------
        // ADMIN WRITE ENDPOINTS
        // --------------------------------------------------

        /// <summary>
        /// Create a product.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailsDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new Product
            {
                Name_en = dto.Name_en,
                Name_de = dto.Name_de,
                Description_en = dto.Description_en,
                Description_de = dto.Description_de,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                IsFeatured = dto.IsFeatured
            };

            var created = await _products.AddProductAsync(entity);
            // Point Location header to Details
            return CreatedAtAction(nameof(Details), new { id = created.Id }, new { id = created.Id });
        }

        /// <summary>
        /// Update a product.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existing = await _products.GetProductByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name_en = dto.Name_en;
            existing.Name_de = dto.Name_de;
            existing.Description_en = dto.Description_en;
            existing.Description_de = dto.Description_de;
            existing.Price = dto.Price;
            existing.CategoryId = dto.CategoryId;
            existing.IsFeatured = dto.IsFeatured;

            await _products.UpdateProductAsync(existing);
            return NoContent();
        }

        /// <summary>
        /// Soft-delete a product.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var ok = await _products.SoftDeleteProductAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
