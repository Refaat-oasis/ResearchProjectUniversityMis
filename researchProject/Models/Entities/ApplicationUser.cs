using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace researchProject.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        [StringLength(150, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز 150 حرفاً")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "تاريخ التسجيل")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "حالة الحساب")]
        public bool IsActive { get; set; } = true;
    }
}
