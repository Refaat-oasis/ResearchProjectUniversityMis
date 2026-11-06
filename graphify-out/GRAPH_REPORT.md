# Graph Report - researchProject  (2026-11-07)

## Corpus Check
- 100 files · ~41,718 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 808 nodes · 2076 edges · 63 communities (27 shown, 36 thin omitted)
- Extraction: 83% EXTRACTED · 17% INFERRED · 0% AMBIGUOUS · INFERRED: 356 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4e9eb5fe`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Task
- researchProject.Models.Entities
- Research
- SystemStatisticsDto
- Evaluation
- CollegeCoAdminRepository
- SuperAdminRepository
- 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)
- ResearchStatusHistory
- researchProject.Tests
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- Researcher
- http
- IEmailSender
- Judge
- ApplicationDbContext
- JudgeInvitation
- CollegeCoAdmin
- ResearcherSignUpDto
- ResearcherResearchesBridge
- HomeController
- ApplicationUser
- 1. Entity & Database Rules (ERD Compliance)
- Task
- ResearcherRepository
- _DynamicCategoryFieldsPartial.cshtml
- List<researchProject.Models.Entities.College>
- List<researchProject.Models.Entities.CollegeCoAdmin>
- List<researchProject.Models.Entities.ResearchCategory>
- researchProject.Models.DTOs.AcceptInvitationDto
- researchProject.Models.DTOs.CertificateVerificationResultDto
- researchProject.Models.DTOs.LoginDto
- researchProject.Models.DTOs.ResearchCreateDto
- researchProject.Models.DTOs.ResearchModificationDto
- _CollegeReportsPartial.cshtml
- researchProject.Models.Entities.CollegeCoAdmin
- researchProject.Models.Entities.Judge
- researchProject.Models.Entities.Researcher
- _MyCertificatesPartial.cshtml
- _CertificatesArchivePartial.cshtml
- SuperAdmin/Index.cshtml
- _ReportsPartial.cshtml
- JudgeRepository
- ICertificateService
- خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan)
- AccountRepository
- ResearchAttachment
- ApplicationDbContext.OnModelCreating
- 20261106233041_InitialCreate.Designer.cs
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- InitialCreate

## God Nodes (most connected - your core abstractions)
1. `Research` - 44 edges
2. `Evaluation` - 38 edges
3. `CollegeCoAdminRepository` - 37 edges
4. `ApplicationDbContext` - 34 edges
5. `SuperAdminRepository` - 33 edges
6. `Judge` - 33 edges
7. `College` - 33 edges
8. `Researcher` - 29 edges
9. `ResearchStatusHistory` - 29 edges
10. `SuperAdminController` - 26 edges

## Surprising Connections (you probably didn't know these)
- `7. Evaluation (Review) & Round Closing` --references--> `JudgeRepository`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Repositories/Implementations/JudgeRepository.cs
- `1. مهام التنفيذ` --references--> `ApplicationUser`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ApplicationUser.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `CollegeCoAdminController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/CollegeCoAdminController.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `JudgeController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/JudgeController.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `ResearcherController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/ResearcherController.cs

## Import Cycles
- None detected.

## Communities (63 total, 36 thin omitted)

### Community 0 - "Task"
Cohesion: 0.07
Nodes (16): AccountController, CollegeCoAdminController, HomeController, JudgeController, SuperAdminController, AcceptInvitationDto, AcademicTitle, ConfirmPassword (+8 more)

### Community 1 - "researchProject.Models.Entities"
Cohesion: 0.06
Nodes (26): researchProject.Services, researchProject.Data, researchProject.Tests, researchProject.Repositories.Interfaces, researchProject.Controllers, researchProject.Models.DTOs, researchProject, researchProject.Models.Constants (+18 more)

### Community 2 - "Research"
Cohesion: 0.10
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 3 - "SystemStatisticsDto"
Cohesion: 0.08
Nodes (25): ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId, CollegeId, DynamicAnswers, InformedConsentFile, ResearchProposalFile (+17 more)

### Community 4 - "Evaluation"
Cohesion: 0.09
Nodes (19): 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد), Evaluation, Decision, Id, InformedConsentRating, IsDigitallySigned (+11 more)

### Community 5 - "CollegeCoAdminRepository"
Cohesion: 0.08
Nodes (21): College, Code, CollegeCoAdmins, Id, Name, ResearchCategories, Researchers, Researches (+13 more)

### Community 7 - "4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)"
Cohesion: 0.21
Nodes (11): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), 1. مهام التنفيذ, PublicController, CertificateVerificationResultDto, CollegeName, IssueDate, IsValid, Message (+3 more)

### Community 8 - "ResearchStatusHistory"
Cohesion: 0.17
Nodes (12): 2. اختبارات المرحلة السابعة, المرحلة السابعة (Phase 7): الاعتماد النهائي وإصدار الشهادة والتحقق, ResearchStatusHistory, ChangedAt, ChangedByUser, ChangedByUserId, Id, NewStatus (+4 more)

### Community 9 - "researchProject.Tests"
Cohesion: 0.12
Nodes (16): coverlet.collector (6.0.4), Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.2), Microsoft.EntityFrameworkCore.Design (9.0.2), Microsoft.EntityFrameworkCore.InMemory (9.0.2), Microsoft.EntityFrameworkCore.SqlServer (9.0.2), Microsoft.EntityFrameworkCore.Tools (9.0.2), Microsoft.NET.Test.Sdk (17.14.1), Moq (4.20.72) (+8 more)

### Community 10 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.11
Nodes (17): 1. الهدف من المشروع (Project Objective), ① بوابة الباحثين (Researchers Portal), 2. الهيكل العام والمقترح التقني (System Architecture), ② بوابة المحكمين وأعضاء اللجنة (Reviewers & Committee Portal), 3. دورة العمل الرقمية لإصدار الشهادة الأخلاقية (Workflow), ③ لوحة التحكم الإدارية (Admin Dashboard), 4. أدوار النظام وارتباطها بالبوابات, 5. المحكمون: الباحث كمحكم ونظام الدعوات (+9 more)

### Community 11 - "Researcher"
Cohesion: 0.14
Nodes (14): 1. مهام التنفيذ, 2. اختبارات المرحلة الثانية, المرحلة الثانية (Phase 2): الدخول الموحد والتسجيل بالبريد المؤسسي والواجهة العربية, Researcher, AcademicDegree, College, CollegeId, Department (+6 more)

### Community 12 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 13 - "IEmailSender"
Cohesion: 0.08
Nodes (10): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), ResearchModificationDto, ModifiedProposalFile, ModifiedToolsFile, ResearcherNotes, ResearchId, EmailSender, FileStorageService (+2 more)

### Community 14 - "Judge"
Cohesion: 0.14
Nodes (13): 11. Seeding & Configuration, 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات, DbSeeder, Judge, AcademicTitle, Evaluations (+5 more)

### Community 15 - "ApplicationDbContext"
Cohesion: 0.14
Nodes (12): ApplicationDbContext, CollegeCoAdmins, Colleges, Evaluations, JudgeInvitations, Judges, ResearchAttachments, ResearchCategories (+4 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.18
Nodes (11): JudgeInvitation, AcceptedAt, CreatedAt, Email, ExpiresAt, Id, InvitedByUser, InvitedByUserId (+3 more)

### Community 17 - "CollegeCoAdmin"
Cohesion: 0.20
Nodes (10): 1. مهام التنفيذ, 2. اختبارات المرحلة الثالثة, المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين), CollegeCoAdmin, College, CollegeId, Id, SignatureImagePath (+2 more)

### Community 18 - "ResearcherSignUpDto"
Cohesion: 0.17
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 19 - "ResearcherResearchesBridge"
Cohesion: 0.13
Nodes (15): 10. Security & Data Isolation, 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 5. UI/UX, RTL, Right Sidebar & Partial Views, 6. Research Status Workflow (Strict), 7. Evaluation (Review) & Round Closing, 8. Certificate, Digital Signature & Archive, 9. Reports & Statistics, AI Agent Instructions & Project Rules (`AGENTS.md`) (+7 more)

### Community 20 - "HomeController"
Cohesion: 0.33
Nodes (4): researchProject.Models, ErrorViewModel, RequestId, ShowRequestId

### Community 21 - "ApplicationUser"
Cohesion: 0.32
Nodes (3): 0. Identity Model Summary (READ FIRST), ApplicationUser, FullName

### Community 22 - "1. Entity & Database Rules (ERD Compliance)"
Cohesion: 0.39
Nodes (8): 1. Entity & Database Rules (ERD Compliance), AttachmentTypeConstants, EthicsRatingConstants, EvaluationDecisionConstants, InvitationStatusConstants, ResearcherTypeConstants, ResearchStatusConstants, RoleConstants

### Community 24 - "ResearcherRepository"
Cohesion: 0.16
Nodes (3): ResearcherController, ResearcherRepository, IResearcherRepository

### Community 52 - "JudgeRepository"
Cohesion: 0.19
Nodes (11): 1. مهام التنفيذ, EvaluationSubmitDto, Decision, EvaluationId, InformedConsentRating, ModificationNotes, PrivacyProtectionRating, Recommendations (+3 more)

### Community 54 - "خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan)"
Cohesion: 0.18
Nodes (10): 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الثامنة, 2. اختبارات المرحلة الرابعة, 2. اختبارات المرحلة السادسة, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر, المرحلة الرابعة (Phase 4): بوابة الباحث وتقديم البحث, المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد) (+2 more)

### Community 56 - "ResearchAttachment"
Cohesion: 0.20
Nodes (10): ResearchAttachment, AttachmentType, FileSize, Id, OriginalFileName, Research, ResearchId, StoredFileName (+2 more)

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

## Knowledge Gaps
- **226 isolated node(s):** `researchProject.Models.Entities.CollegeCoAdmin`, `researchProject.Models.DTOs.SystemStatisticsDto`, `researchProject.Models.Entities.Judge`, `researchProject.Models.Entities.Researcher`, `researchProject.Models.Entities.Research` (+221 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 311 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **36 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `researchProject.Models.Constants` connect `researchProject.Models.Entities` to `1. Entity & Database Rules (ERD Compliance)`?**
  _High betweenness centrality (0.061) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `Research` (e.g. with `1. Entity & Database Rules (ERD Compliance)` and `1. مهام التنفيذ`) actually correct?**
  _`Research` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `researchProject.Models.Entities.CollegeCoAdmin`, `researchProject.Models.DTOs.SystemStatisticsDto`, `researchProject.Models.Entities.Judge` to the rest of the system?**
  _226 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Task` be split into smaller, more focused modules?**
  _Cohesion score 0.0669753086419753 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `researchProject.Models.Entities`, `Research`, `Evaluation`, `CollegeCoAdminRepository`, `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)`, `ResearchStatusHistory`, `Researcher`, `Judge`, `JudgeInvitation`, `CollegeCoAdmin`, `ResearcherResearchesBridge`, `JudgeRepository`, `ApplicationUser`, `1. Entity & Database Rules (ERD Compliance)`, `AccountRepository`, `ResearchAttachment`, `ApplicationDbContext.OnModelCreating`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **Are the 13 inferred relationships involving `Evaluation` (e.g. with `1. Entity & Database Rules (ERD Compliance)` and `3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution`) actually correct?**
  _`Evaluation` has 13 INFERRED edges - model-reasoned connections that need verification._
- **Should `researchProject.Models.Entities` be split into smaller, more focused modules?**
  _Cohesion score 0.059882312218760815 - nodes in this community are weakly interconnected._