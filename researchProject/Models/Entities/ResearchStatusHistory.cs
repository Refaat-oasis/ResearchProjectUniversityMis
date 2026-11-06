using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class ResearchStatusHistory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف البحث مطلوب")]
        [Display(Name = "معرف البحث")]
        public int ResearchId { get; set; }

        [Display(Name = "الحالة السابقة")]
        [StringLength(50, ErrorMessage = "الحالة السابقة يجب ألا تتجاوز 50 حرفاً")]
        public string? OldStatus { get; set; }

        [Required(ErrorMessage = "الحالة الجديدة مطلوبة")]
        [Display(Name = "الحالة الجديدة")]
        [StringLength(50, ErrorMessage = "الحالة الجديدة يجب ألا تتجاوز 50 حرفاً")]
        public string NewStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "معرف المستخدم المغيّر مطلوب")]
        [Display(Name = "معرف المستخدم المغيّر")]
        public string ChangedByUserId { get; set; } = string.Empty;

        [Display(Name = "تاريخ التغيير")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "سبب التغيير / ملاحظات")]
        [StringLength(1000, ErrorMessage = "سبب التغيير يجب ألا يتجاوز 1000 حرف")]
        public string? Reason { get; set; }

        // Navigation properties
        public virtual Research Research { get; set; } = null!;
        public virtual ApplicationUser ChangedByUser { get; set; } = null!;
    }
}
