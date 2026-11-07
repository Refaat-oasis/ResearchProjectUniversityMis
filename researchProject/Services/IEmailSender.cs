namespace researchProject.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendRegistrationTokenAsync(string email, string token, string confirmationLink);
        Task SendPasswordResetTokenAsync(string email, string token, string resetLink);
        Task SendJudgeInvitationAsync(string email, string invitationLink, string specialty);
        Task SendStatusChangedNotificationAsync(string email, string researchTitle, string oldStatus, string newStatus, string? notes = null);
        Task SendCertificateIssuedNotificationAsync(string email, string researchTitle, string certificateSerialNumber, string downloadUrl);
        Task SendTemporaryPasswordAsync(string email, string tempPassword);
    }
}
