using System;
using System.ComponentModel.DataAnnotations;

namespace researchProject.Models.Entities
{
    public class ResearchAttachment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "معرف البحث مطلوب")]
        [Display(Name = "معرف البحث")]
        public int ResearchId { get; set; }

        [Required(ErrorMessage = "نوع المرفق مطلوب")]
        [Display(Name = "نوع المرفق")]
        [StringLength(100, ErrorMessage = "نوع المرفق يجب ألا يتجاوز 100 حرف")]
        public string AttachmentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم النسخة مطلوب")]
        [Display(Name = "رقم النسخة")]
        public int Version { get; set; } = 1;

        [Required(ErrorMessage = "اسم الملف الأصلي مطلوب")]
        [Display(Name = "اسم الملف الأصلي")]
        [StringLength(260, ErrorMessage = "اسم الملف الأصلي يجب ألا يتجاوز 260 حرفاً")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الملف المخزن مطلوب")]
        [Display(Name = "اسم الملف المخزن")]
        [StringLength(260, ErrorMessage = "اسم الملف المخزن يجب ألا يتجاوز 260 حرفاً")]
        public string StoredFileName { get; set; } = string.Empty;

        [Display(Name = "مسار حفظ الملف")]
        [StringLength(500, ErrorMessage = "مسار حفظ الملف يجب ألا يتجاوز 500 حرف")]
        public string FilePath { get; set; } = string.Empty;

        [Display(Name = "حجم الملف بالبايت")]
        public long FileSize { get; set; }

        [Display(Name = "نوع المحتوى")]
        [StringLength(150)]
        public string? ContentType { get; set; }

        [Display(Name = "تاريخ الرفع")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Research Research { get; set; } = null!;
    }
}
