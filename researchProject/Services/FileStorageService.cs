using System;
using System.IO;
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
                _baseStoragePath = Path.Combine(env.ContentRootPath, "App_Data", "UploadedFiles");
            }
            else if (Path.IsPathRooted(configPath))
            {
                _baseStoragePath = configPath;
            }
            else
            {
                _baseStoragePath = Path.Combine(env.ContentRootPath, configPath);
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

        public async Task<(string StoredFileName, long FileSize)> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى المسموح به (25 ميجابايت).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!IsAllowedFileType(file.FileName))
                throw new InvalidOperationException("نوع الملف غير مسموح به. الصيغ المسموحة هي: PDF, DOCX, XLSX, PNG, JPG.");

            var folderPath = Path.Combine(_baseStoragePath, subFolder);
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

            return (storedFileName, file.Length);
        }

        public Task<Stream?> GetFileStreamAsync(string storedFileName, string subFolder)
        {
            var fullPath = Path.Combine(_baseStoragePath, subFolder, storedFileName);
            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream?>(stream);
        }

        public void DeleteFile(string storedFileName, string subFolder)
        {
            var fullPath = Path.Combine(_baseStoragePath, subFolder, storedFileName);
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
