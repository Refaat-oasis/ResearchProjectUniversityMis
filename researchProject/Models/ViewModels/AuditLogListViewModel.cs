using System;
using System.Collections.Generic;

namespace researchProject.Models.ViewModels
{
    public class AuditLogListViewModel
    {
        public List<AuditLogViewModel> Logs { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public bool? SuccessFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

