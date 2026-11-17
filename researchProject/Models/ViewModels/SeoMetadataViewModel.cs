namespace researchProject.Models.ViewModels
{
    public class SeoMetadataViewModel
    {
        public string Title { get; set; } = "المنصة الإلكترونية لأخلاقيات البحث العلمي والنزاهة الأكاديمية";
        public string Description { get; set; } = "منصة جامعية متكاملة ومؤتمتة لإدارة ومتابعة طلبات أخلاقيات البحث العلمي والنزاهة الأكاديمية وإصدار الشهادات الأخلاقية المعتمدة.";
        public string Keywords { get; set; } = "أخلاقيات البحث العلمي, النزاهة الأكاديمية, لجان الأخلاقيات, تحكيم الأبحاث, شهادات رقمية, جامعة مصر";
        public string Author { get; set; } = "اللجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي";
        public string CanonicalUrl { get; set; } = "/";
        public string OgType { get; set; } = "website";
        public string OgImage { get; set; } = "/favicon.ico";
        public string TwitterCard { get; set; } = "summary_large_image";
        public bool NoIndex { get; set; } = false;
    }
}

