using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace researchProject.Services
{
    public interface IFileStorageService
    {
        Task<(string StoredFileName, long FileSize, string RelativePath)> SaveFileAsync(IFormFile file, string subFolder);
        Task<(string StoredFileName, long FileSize, string RelativePath)> SaveFileBytesAsync(byte[] fileBytes, string originalFileName, string subFolder);
        Task<Stream?> GetFileStreamAsync(string storedFileName, string subFolder);
        string GetPhysicalPath(string storedFileName, string subFolder);
        string GetRelativePath(string storedFileName, string subFolder);
        void DeleteFile(string storedFileName, string subFolder);
        bool IsAllowedFileType(string fileName);
    }
}
