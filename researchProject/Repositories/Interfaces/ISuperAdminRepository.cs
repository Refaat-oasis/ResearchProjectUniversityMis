using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Models.ViewModels;

namespace researchProject.Repositories.Interfaces
{
    public interface ISuperAdminRepository
    {
        Task<List<College>> GetAllCollegesAsync();
        Task<College?> GetCollegeByIdAsync(int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> CreateCollegeAsync(College college);
        Task<(bool Succeeded, string? ErrorMessage)> UpdateCollegeAsync(College college);

        Task<List<ResearchCategory>> GetAllCategoriesAsync();
        Task<ResearchCategory?> GetCategoryByIdAsync(int categoryId);
        Task<(bool Succeeded, string? ErrorMessage)> SaveCategoryAsync(ResearchCategory category);

        Task<List<CollegeCoAdmin>> GetAllCoAdminsAsync();
        Task<(bool Succeeded, string? ErrorMessage, string? TempPassword)> CreateCoAdminNewUserAsync(string email, string fullName, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> AddCoAdminRoleToExistingUserAsync(string userId, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> UpdateCoAdminSignatureAsync(int coAdminId, string signaturePath);

        Task<List<Judge>> GetAllJudgesAsync();
        Task<List<JudgeInvitation>> GetAllJudgeInvitationsAsync();
        Task<(bool Succeeded, string? ErrorMessage, string? RawToken)> InviteJudgeAsync(string email, string? specialty, string invitedByUserId);
        Task<(bool Succeeded, string? ErrorMessage)> SetJudgeActiveStatusAsync(int judgeId, bool isActive);
        Task<(bool Succeeded, string? ErrorMessage)> RevokeInvitationAsync(int invitationId);

        Task<List<Research>> GetAllResearchesAsync();
        Task<Research?> GetResearchDetailsAsync(int researchId);

        Task<List<Research>> SearchCertificatesArchiveAsync(string? serialNumber, string? researcherName, int? collegeId, DateTime? fromDate, DateTime? toDate);
        Task<SystemStatisticsViewModel> GetSystemStatisticsAsync();
    }
}
