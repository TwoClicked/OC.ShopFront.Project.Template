// OC.LUAC.ApiLayer/Controllers/ProductImageController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OC.LUAC.ApiLayer.DTO.Product;
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OC.LUAC.ApiLayer.Controllers
{
    [ApiController]
    [Route("api/products/{productId:int}/images")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _imageService;
        private readonly IWebHostEnvironment _env;
        private static readonly string[] _permitted = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public ProductImageController(IProductImageService imageService, IWebHostEnvironment env)
        {
            _imageService = imageService;
            _env = env;
        }

        // GET /api/products/{productId}/images
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductImageResponseDto>), 200)]
        public async Task<ActionResult<List<ProductImageResponseDto>>> GetImages(int productId)
        {
            var images = await _imageService.GetImagesByProductIdAsync(productId);
            var list = new List<ProductImageResponseDto>();
            foreach (var i in images)
            {
                list.Add(ToDto(i));
            }
            return Ok(list);
        }

        // POST /api/products/{productId}/images  (add by absolute URL)
        [HttpPost]
        [ProducesResponseType(typeof(ProductImageResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProductImageResponseDto>> AddImageByUrl(
            int productId,
            [FromBody] CreateProductImageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                return BadRequest("ImageUrl is required.");
            }

            var created = await _imageService.AddImageAsync(new ProductImage
            {
                ProductId = productId,
                ImageUrl = dto.ImageUrl,
                SortOrder = dto.SortOrder
            });

            return CreatedAtAction(nameof(GetImages), new { productId }, ToDto(created));
        }

        // POST /api/products/{productId}/images/upload  (single file)
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [ProducesResponseType(typeof(ProductImageResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProductImageResponseDto>> UploadSingle(
            int productId,
            [FromForm] UploadProductImageForm form)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (form.File == null || form.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var url = await SaveToDiskAsync(productId, form.File);

            var created = await _imageService.AddImageAsync(new ProductImage
            {
                ProductId = productId,
                ImageUrl = url,
                SortOrder = form.SortOrder
            });

            return CreatedAtAction(nameof(GetImages), new { productId }, ToDto(created));
        }

        // POST /api/products/{productId}/images/upload-multiple  (multiple files)
        [HttpPost("upload-multiple")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(40 * 1024 * 1024)]
        [ProducesResponseType(typeof(List<ProductImageResponseDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ProductImageResponseDto>>> UploadMultiple(
            int productId,
            [FromForm] UploadMultipleProductImagesForm form)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            if (form.Files == null || form.Files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var createdDtos = new List<ProductImageResponseDto>();

            for (int i = 0; i < form.Files.Count; i++)
            {
                var file = form.Files[i];
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var url = await SaveToDiskAsync(productId, file);

                var sort = (form.SortOrders != null && form.SortOrders.Count == form.Files.Count)
                           ? form.SortOrders[i]
                           : form.BaseSortOrder + i;

                var created = await _imageService.AddImageAsync(new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url,
                    SortOrder = sort
                });

                createdDtos.Add(ToDto(created));
            }

            return CreatedAtAction(nameof(GetImages), new { productId }, createdDtos);
        }

        // PUT /api/products/{productId}/images/{imageId}
        [HttpPut("{imageId:int}")]
        [ProducesResponseType(typeof(ProductImageResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductImageResponseDto>> UpdateImage(
            int productId,
            int imageId,
            [FromBody] UpdateProductImageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var list = await _imageService.GetImagesByProductIdAsync(productId);
            var existing = list.FirstOrDefault(i => i.Id == imageId);
            if (existing == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                existing.ImageUrl = dto.ImageUrl;
            }
            existing.SortOrder = dto.SortOrder;

            var updated = await _imageService.UpdateImageAsync(existing);
            return Ok(ToDto(updated));
        }

        // PUT /api/products/{productId}/images/reorder  (bulk sort orders)
        [HttpPut("reorder")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Reorder(int productId, [FromBody] List<ImageOrderItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest("No items provided.");
            }

            var list = await _imageService.GetImagesByProductIdAsync(productId);
            var map = list.ToDictionary(i => i.Id, i => i);

            foreach (var it in items)
            {
                if (map.TryGetValue(it.ImageId, out var img))
                {
                    img.SortOrder = it.SortOrder;
                    await _imageService.UpdateImageAsync(img);
                }
            }

            return NoContent();
        }

        // DELETE /api/products/{productId}/images/{imageId}
        [HttpDelete("{imageId:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteImage(int productId, int imageId)
        {
            // Optional: delete physical file too
            var list = await _imageService.GetImagesByProductIdAsync(productId);
            var existing = list.FirstOrDefault(i => i.Id == imageId);

            var ok = await _imageService.DeleteImageAsync(imageId);
            if (!ok)
            {
                return NotFound();
            }

            if (existing != null && !string.IsNullOrWhiteSpace(existing.ImageUrl))
            {
                try
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                    var diskPath = Path.Combine(webRoot, existing.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(diskPath))
                    {
                        System.IO.File.Delete(diskPath);
                    }
                }
                catch
                {
                    // ignore disk delete failures
                }
            }

            return NoContent();
        }

        // ------------ helpers ------------

        private async Task<string> SaveToDiskAsync(int productId, IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permitted.Contains(ext))
            {
                throw new InvalidOperationException("Unsupported file type.");
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var productFolder = Path.Combine(webRoot, "uploads", "products", productId.ToString());
            Directory.CreateDirectory(productFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(productFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            // return web-relative URL
            return $"/uploads/products/{productId}/{fileName}";
        }

        private static ProductImageResponseDto ToDto(ProductImage i)
        {
            return new ProductImageResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ImageUrl = i.ImageUrl,
                SortOrder = i.SortOrder
            };
        }

        public class ImageOrderItem
        {
            public int ImageId { get; set; }
            public int SortOrder { get; set; }
        }
    }
}
