using System.IO;
using System.Threading.Tasks;

namespace researchProject.Services.Interfaces
{
    public interface IAttachmentService
    {
        Task<(bool IsAuthorized, bool FileExists, Stream? FileStream, string ContentType, string FileName)> GetAttachmentDownloadAsync(int attachmentId, string userId);
    }
}
