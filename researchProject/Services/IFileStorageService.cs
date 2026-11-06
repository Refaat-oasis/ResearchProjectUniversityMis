using Microsoft.AspNetCore.Http;

namespace researchProject.Services
{
    public interface IFileStorageService
    {
        Task<(string StoredFileName, long FileSize)> SaveFileAsync(IFormFile file, string subFolder);
        Task<Stream?> GetFileStreamAsync(string storedFileName, string subFolder);
        void DeleteFile(string storedFileName, string subFolder);
        bool IsAllowedFileType(string fileName);
    }
}
