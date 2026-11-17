namespace researchProject.Models.ViewModels
{
    public class UserSummaryViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Id { get => UserId; set => UserId = value; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
