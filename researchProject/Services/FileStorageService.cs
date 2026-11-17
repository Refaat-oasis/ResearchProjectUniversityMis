using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace researchProject.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _baseStoragePath;
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

        public FileStorageService(IWebHostEnvironment env, IConfiguration config)
        {
            var configPath = config["FileStorage:RootPath"];
            if (string.IsNullOrWhiteSpace(configPath))
            {
                var webRoot = !string.IsNullOrWhiteSpace(env.WebRootPath) 
                    ? env.WebRootPath 
                    : Path.Combine(env.ContentRootPath, "wwwroot");
                _baseStoragePath = Path.Combine(webRoot, "uploads", "attachments");
            }
            else if (Path.IsPathRooted(configPath))
            {
                _baseStoragePath = configPath;
            }
            else
            {
                var webRoot = !string.IsNullOrWhiteSpace(env.WebRootPath) 
                    ? env.WebRootPath 
                    : Path.Combine(env.ContentRootPath, "wwwroot");
                _baseStoragePath = Path.Combine(webRoot, configPath);
            }

            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }
        }

        public bool IsAllowedFileType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return Array.Exists(_allowedExtensions, e => e == ext);
        }

        public string GetRelativePath(string storedFileName, string subFolder)
        {
            var safeSubFolder = Path.GetFileName(subFolder ?? string.Empty);
            var safeFileName = Path.GetFileName(storedFileName ?? string.Empty);
            return $"uploads/attachments/{safeSubFolder}/{safeFileName}";
        }

        public string GetPhysicalPath(string storedFileName, string subFolder)
        {
            var safeSubFolder = Path.GetFileName(subFolder ?? string.Empty);
            var safeFileName = Path.GetFileName(storedFileName ?? string.Empty);
            return Path.Combine(_baseStoragePath, safeSubFolder, safeFileName);
        }

        public async Task<(string StoredFileName, long FileSize, string RelativePath)> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف المرفق فارغ أو لم يتم اختياره.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى المسموح به (25 ميجابايت).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsAllowedFileType(file.FileName))
                throw new InvalidOperationException("نوع الملف غير مسموح به. الصيغ المسموحة هي: PDF, DOCX, XLSX, PNG, JPG.");

            var folderPath = Path.GetFullPath(Path.Combine(_baseStoragePath, subFolder));
            var baseCanonicalPath = Path.GetFullPath(_baseStoragePath);
            if (!folderPath.StartsWith(baseCanonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("مسار الحفظ المحدد غير آمن.");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folderPath, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = GetRelativePath(storedFileName, subFolder);
            return (storedFileName, file.Length, relativePath);
        }

        public async Task<(string StoredFileName, long FileSize, string RelativePath)> SaveFileBytesAsync(byte[] fileBytes, string originalFileName, string subFolder)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("بيانات الملف فارغة أو غير صالحة.");

            if (fileBytes.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى المسموح به (25 ميجابايت).");

            var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!IsAllowedFileType(originalFileName))
                throw new InvalidOperationException("نوع الملف غير مسموح به. الصيغ المسموحة هي: PDF, DOCX, XLSX, PNG, JPG.");

            var folderPath = Path.GetFullPath(Path.Combine(_baseStoragePath, subFolder));
            var baseCanonicalPath = Path.GetFullPath(_baseStoragePath);
            if (!folderPath.StartsWith(baseCanonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("مسار الحفظ المحدد غير آمن.");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folderPath, storedFileName);

            await File.WriteAllBytesAsync(fullPath, fileBytes);
            var relativePath = GetRelativePath(storedFileName, subFolder);
            return (storedFileName, fileBytes.Length, relativePath);
        }

        public Task<Stream?> GetFileStreamAsync(string storedFileName, string subFolder)
        {
            var fullPath = GetPhysicalPath(storedFileName, subFolder);

            // Path Traversal Guard
            var fullCanonicalPath = Path.GetFullPath(fullPath);
            var baseCanonicalPath = Path.GetFullPath(_baseStoragePath);
            if (!fullCanonicalPath.StartsWith(baseCanonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<Stream?>(null);
            }

            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        public void DeleteFile(string storedFileName, string subFolder)
        {
            var fullPath = GetPhysicalPath(storedFileName, subFolder);

            // Path Traversal Guard
            var fullCanonicalPath = Path.GetFullPath(fullPath);
            var baseCanonicalPath = Path.GetFullPath(_baseStoragePath);
            if (!fullCanonicalPath.StartsWith(baseCanonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Ignore deletion exceptions
                }
            }
        }
    }
}
