using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using researchProject.Models.Entities;

namespace researchProject.Models.ViewModels
{
    public class ResearcherSignUpViewModel
    {
        [Display(Name = "الاسم الكامل (رباعي)")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "كلمة المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "تأكيد كلمة المرور")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "الكلية التابع لها")]
        public int CollegeId { get; set; }

        [Display(Name = "فئة الباحث")]
        public string ResearcherType { get; set; } = string.Empty;

        [Display(Name = "الدرجة العلمية")]
        public string AcademicDegree { get; set; } = string.Empty;

        [Display(Name = "القسم الأكاديمي")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "الرقم القومي (14 رقم)")]
        public string NationalId { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف المحمول")]
        public string PhoneNumber { get; set; } = string.Empty;

        public List<College> AvailableColleges { get; set; } = new List<College>();
        public string? ErrorMessage { get; set; }
    }
}
