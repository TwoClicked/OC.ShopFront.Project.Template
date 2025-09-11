using Microsoft.EntityFrameworkCore;
using OC.LUAC.DataLayer;                // AppDbContext
using OC.LUAC.ObjectLayer.Entities;
using OC.LUAC.ServiceLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly AppDbContext _context;

        public ProductImageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductImage>> GetImagesByProductIdAsync(int productId)
        {
            return await _context.ProductImages
                .Where(i => i.ProductId == productId && !i.IsDeleted)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
        }

        public async Task<ProductImage> AddImageAsync(ProductImage image)
        {
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<ProductImage> UpdateImageAsync(ProductImage image)
        {
            _context.ProductImages.Update(image);
            await _context.SaveChangesAsync();
            return image;
        }
       
        public async Task<bool> DeleteImageAsync(int imageId)
        {
            var existing = await _context.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId);
            if (existing == null) return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
