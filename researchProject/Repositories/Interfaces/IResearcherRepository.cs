using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;

namespace researchProject.Repositories.Interfaces
{
    public interface IResearcherRepository
    {
        Task<Researcher?> GetProfileByUserIdAsync(string userId);
        Task<List<ResearchCategory>> GetCategoriesForCollegeAsync(int collegeId);
        Task<ResearchCategory?> GetCategoryWithFieldsAsync(int categoryId);
        Task<List<Research>> GetMyResearchesAsync(string researcherUserId);
        Task<Research?> GetResearchDetailsAsync(int researchId, string researcherUserId);
        Task<(bool Succeeded, string? ErrorMessage, int? ResearchId)> CreateResearchWithAttachmentsAsync(ResearchCreateDto dto, string researcherUserId);
        Task<(bool Succeeded, string? ErrorMessage)> SubmitModifiedAttachmentsAsync(ResearchModificationDto dto, string researcherUserId);
        Task<List<Research>> GetMyCertificatesAsync(string researcherUserId);
        Task<(byte[]? FileBytes, string? SerialNumber)> GetCertificateDownloadAsync(int researchId, string researcherUserId);
    }
}
