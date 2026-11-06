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
        Task SignOutAsync();
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<List<College>> GetActiveCollegesAsync();
    }
}
