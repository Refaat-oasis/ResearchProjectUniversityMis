using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;

namespace researchProject.Repositories.Interfaces
{
    public interface ICollegeCoAdminRepository
    {
        Task<CollegeCoAdmin?> GetProfileByUserIdAsync(string userId);
        Task<List<Research>> GetScreeningQueueAsync(int collegeId);
        Task<Research?> GetResearchDetailsAsync(int researchId, int collegeId);
        Task<(bool Succeeded, string? ErrorMessage)> SaveScreeningNotesAsync(int researchId, int collegeId, string notes);

        Task<List<Judge>> SuggestJudgesAsync(int researchId, int collegeId);
        Task<List<Judge>> GetAvailableJudgesForCollegeAsync(int researchId);
        Task<(bool Succeeded, string? ErrorMessage)> AssignJudgesToResearchAsync(int researchId, int collegeId, List<int> judgeIds, string actingUserId);

        Task<(bool Succeeded, string? ErrorMessage, string? RawToken)> InviteJudgeAsync(string email, string? specialty, string invitedByUserId);

        Task<List<Research>> GetApprovedResearchesForIssuanceAsync(int collegeId);
        Task<(bool Succeeded, string? ErrorMessage, string? SerialNumber)> IssueEthicalCertificateAsync(int researchId, string coAdminUserId, string baseUrl);
        Task<(bool Succeeded, string? ErrorMessage)> UploadSignatureImageAsync(string coAdminUserId, string relativePath);

        Task<SystemStatisticsDto> GetCollegeStatisticsAsync(int collegeId);
    }
}
