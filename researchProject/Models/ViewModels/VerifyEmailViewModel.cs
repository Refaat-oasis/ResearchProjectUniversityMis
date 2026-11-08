namespace researchProject.Models.ViewModels
{
    public class VerifyEmailViewModel
    {
        public string EmailOrUserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasPendingCode { get; set; } = true;
        public int ValidityMinutes { get; set; } = 15;
    }
}

