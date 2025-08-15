
using System;
using System.Resources;

namespace OC.LUAC.ApiLayer.Storage
{
    public class LocalImageStorage : IImageStorage
    {

        private readonly IWebHostEnvironment _env;
        private static readonly string[] _permitted = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };


        public LocalImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> SaveAsync(IFormFile file, string relativeFolder, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Empty file.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permitted.Contains(ext))
            {
                throw new InvalidOperationException("Unsupported file type.");
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var targetFolder = Path.Combine(webRoot, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(targetFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(targetFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream, ct);
            }

            // Return a web URL starting with /...
            var url = "/" + Path.Combine(relativeFolder, fileName).Replace('\\', '/');
            return url;
        }

        public Task<bool> DeleteAsync(string relativeOrAbsoluteUrl, CancellationToken ct = default)
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var localPath = relativeOrAbsoluteUrl.StartsWith("/") ? relativeOrAbsoluteUrl.Substring(1) : relativeOrAbsoluteUrl;
                var diskPath = Path.Combine(webRoot, localPath.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(diskPath))
                {
                    File.Delete(diskPath);
                        return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch 
            {
                return Task.FromResult(false);
            }
        }
    }
}
