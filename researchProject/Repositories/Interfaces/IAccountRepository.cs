using System.Collections.Generic;
using System.Threading.Tasks;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;

namespace researchProject.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<(bool Succeeded, bool IsLockedOut, bool RequiresTwoFactor, string? ErrorMessage, IList<string> Roles)> LoginAsync(string email, string password, bool rememberMe);
        Task<(bool Succeeded, string? ErrorMessage, string? ConfirmationToken, string? UserId)> SignUpResearcherAsync(ResearcherSignUpDto dto);
        Task<(bool Succeeded, string? ErrorMessage)> ConfirmEmailAsync(string userId, string token);
        Task<JudgeInvitation?> GetInvitationByTokenAsync(string token);
        Task<(bool Succeeded, string? ErrorMessage)> AcceptJudgeInvitationAsync(string token, string? fullName, string? password, string? academicTitle, string? signedInUserId);
        Task RefreshSignInAsync(string userId);
        Task SignOutAsync();
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<List<College>> GetActiveCollegesAsync();
        Task<List<Department>> GetDepartmentsByCollegeAsync(int collegeId);
        Task<(bool Succeeded, string? ErrorMessage, string? ResetToken, string? UserId)> GeneratePasswordResetTokenAsync(string email);
        Task<(bool Succeeded, string? ErrorMessage)> ResetPasswordWithTokenAsync(string email, string token, string newPassword);
        Task<(bool Succeeded, string? ErrorMessage)> ConfirmEmailWithTokenOrCodeAsync(string emailOrUserId, string token);
        
        // 6-digit 15-minute OTP operations
        Task<(bool Succeeded, string? ErrorMessage, string? Code, string? UserId)> GenerateVerificationCodeAsync(string email, string purpose, string? userId = null);
        Task<(bool Succeeded, string? ErrorMessage)> ValidateVerificationCodeAsync(string emailOrUserId, string code, string purpose, bool markAsUsed = true);
        Task<(bool Succeeded, string? ErrorMessage, string? Code, string? ResolvedEmail)> ResendVerificationCodeAsync(string emailOrUserId, string purpose);
        Task<bool> MarkVerificationCodeAsUsedAsync(string emailOrUserId, string code, string purpose);

        // User Profile & Judge Dual-Role Promotion
        Task<researchProject.Models.ViewModels.UserProfileViewModel?> GetUserProfileAsync(string userId);
        Task<(bool Succeeded, string? ErrorMessage)> AddResearcherRoleToJudgeAsync(string userId, JudgeToResearcherDto dto);
    }
}
