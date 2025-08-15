using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;


namespace OC.LUAC.ApiLayer.Storage
{
    public interface IImageStorage
    {

        Task<string> SaveAsync(IFormFile file, string relativeFolder, CancellationToken ct = default);
        Task<bool> DeleteAsync(string relativeOrAbsoluteUrl, CancellationToken ct = default);

    }
}
