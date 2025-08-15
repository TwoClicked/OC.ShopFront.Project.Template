using OC.LUAC.ObjectLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ServiceLayer.Interfaces
{
    public interface IProductImageService
    {
        Task<List<ProductImage>> GetImagesByProductIdAsync(int productId);
        Task<ProductImage> AddImageAsync(ProductImage image);
        Task<ProductImage> UpdateImageAsync(ProductImage image);
        Task<bool> DeleteImageAsync(int imageId); // soft-delete is fine
    }
}
