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
        Task<List<Department>> GetDepartmentsByCollegeAsync(int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> SaveDepartmentAsync(int? id, string name, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> DeleteDepartmentAsync(int id);

        Task<List<ResearchCategory>> GetAllCategoriesAsync();
        Task<ResearchCategory?> GetCategoryByIdAsync(int categoryId);
        Task<(bool Succeeded, string? ErrorMessage)> SaveCategoryAsync(ResearchCategory category);
        Task<(bool Succeeded, string? ErrorMessage)> ToggleCategoryStatusAsync(int categoryId, bool isActive);

        Task<List<CollegeCoAdmin>> GetAllCoAdminsAsync();
        Task<(bool Succeeded, string? ErrorMessage, string? TempPassword, string? UserId)> CreateCoAdminNewUserAsync(string email, string fullName, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> AddCoAdminRoleToExistingUserAsync(string userId, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> ReassignCoAdminCollegeAsync(int coAdminId, int newCollegeId);
        Task<(bool Succeeded, string? ErrorMessage)> SetCoAdminActiveStatusAsync(int coAdminId, bool isActive);
        Task<(bool Succeeded, string? ErrorMessage)> UpdateCoAdminSignatureAsync(int coAdminId, string signaturePath);

        Task<List<Researcher>> GetAllResearchersAsync();
        Task<(bool Succeeded, string? ErrorMessage)> SetResearcherActiveStatusAsync(int researcherId, bool isActive);

        Task<List<Judge>> GetAllJudgesAsync();
        Task<List<JudgeInvitation>> GetAllJudgeInvitationsAsync();
        Task<List<UserSummaryViewModel>> GetAssignableUsersForInvitationAsync();
        Task<(bool Succeeded, string? ErrorMessage, string? RawToken, int? InvitationId)> InviteJudgeAsync(string email, string? specialty, string invitedByUserId, int? collegeId = null);
        Task<(bool Succeeded, string? ErrorMessage)> SetJudgeActiveStatusAsync(int judgeId, bool isActive);
        Task<(bool Succeeded, string? ErrorMessage)> RevokeInvitationAsync(int invitationId);

        Task<(bool Succeeded, string? ErrorMessage)> SetUserActiveStatusAsync(string userId, bool isActive);

        Task<List<Research>> GetAllResearchesAsync();
        Task<Research?> GetResearchDetailsAsync(int researchId);

        Task<List<Research>> SearchCertificatesArchiveAsync(string? serialNumber, string? researcherName, int? collegeId, DateTime? fromDate, DateTime? toDate);
        Task<SystemStatisticsViewModel> GetSystemStatisticsAsync();
        Task<AuditLogListViewModel> GetAuditLogsAsync(int page = 1, int pageSize = 25, string? searchTerm = null, string? roleFilter = null, bool? successFilter = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
