using _2tlob.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace _2tlob.Services.Implementations
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        // Magic numbers for image verification
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new()
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".webp", new List<byte[]> { new byte[] { 0x52, 0x49, 0x46, 0x46 } } } 
        };

        public FileUploadService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _maxFileSize = configuration.GetValue<long>("FileUpload:MaxFileSizeInBytes", 2097152); // 2 MB
            _allowedExtensions = configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() 
                ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
        }

        public async Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadProductImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file provided.");
            }

            if (file.Length > _maxFileSize)
            {
                return (false, null, $"File size exceeds the allowed limit of {_maxFileSize / (1024 * 1024)} MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                return (false, null, $"Invalid file extension. Allowed extensions are: {string.Join(", ", _allowedExtensions)}.");
            }

            // Verify MIME Type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, null, "Invalid image MIME type.");
            }

            // Verify Magic Bytes (File Signatures) to prevent disguised executable files
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                if (FileSignatures.ContainsKey(extension))
                {
                    var signatures = FileSignatures[extension];
                    var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                    bool isValidSignature = signatures.Any(signature => 
                        headerBytes.Take(signature.Length).SequenceEqual(signature));

                    if (!isValidSignature)
                    {
                        return (false, null, "File content does not match the image extension.");
                    }
                }
            }

            // Generate safe unique filename
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var uploadFolder = Path.Combine(_environment.WebRootPath, "images", "products");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var fullPath = Path.Combine(uploadFolder, safeFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/images/products/{safeFileName}";
            return (true, relativePath, null);
        }

        public void DeleteFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath)) return;

            // Never delete default product placeholder
            if (relativeFilePath.Contains("default-product.png", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var cleanRelative = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(_environment.WebRootPath, cleanRelative);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
               
            }
        }
    }
}
