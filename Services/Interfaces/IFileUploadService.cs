using Microsoft.AspNetCore.Http;

namespace _2tlob.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<(bool Success, string? FilePath, string? ErrorMessage)> UploadProductImageAsync(IFormFile file);
        void DeleteFile(string? relativeFilePath);
    }
}
