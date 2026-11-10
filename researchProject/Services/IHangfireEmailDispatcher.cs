using System.Threading.Tasks;
using Hangfire;

namespace researchProject.Services
{
    public interface IHangfireEmailDispatcher
    {
        string EnqueueJudgeInvitationEmail(int invitationId, string recipientEmail, string recipientName, string invitationLink, string inviterUserId);
        string EnqueueCoAdminCredentialsEmail(string userId, string recipientEmail, string fullName, string temporaryPassword, string collegeName, string creatorUserId);
        
        [AutomaticRetry(Attempts = 0)]
        Task SendEmailWithRollbackAsync(string toEmail, string subject, string htmlMessage, EmailJobType jobType, string entityKey, string initiatorUserId, int attemptNumber);
    }
}

