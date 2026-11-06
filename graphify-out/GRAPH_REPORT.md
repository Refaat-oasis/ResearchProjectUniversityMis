# Graph Report - researchProject  (2026-11-07)

## Corpus Check
- 96 files · ~34,753 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 12 file(s) not represented in the graph (top: .vsidx 5, (none) 2, .css 2)

## Summary
- 733 nodes · 1919 edges · 51 communities (24 shown, 27 thin omitted)
- Extraction: 82% EXTRACTED · 18% INFERRED · 0% AMBIGUOUS · INFERRED: 352 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Task
- researchProject.Models.Entities
- Research
- SystemStatisticsDto
- Evaluation
- College
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
- AI Agent Instructions & Project Rules (`AGENTS.md`)
- 1. Entity & Database Rules (ERD Compliance)
- AcceptInvitationDto
- LoginDto
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
- CollegeCoAdmin/Index.cshtml
- Judge/Index.cshtml
- Researcher/Index.cshtml
- _MyCertificatesPartial.cshtml
- _CertificatesArchivePartial.cshtml
- SuperAdmin/Index.cshtml
- _ReportsPartial.cshtml

## God Nodes (most connected - your core abstractions)
1. `Research` - 62 edges
2. `College` - 40 edges
3. `ApplicationDbContext` - 39 edges
4. `Judge` - 38 edges
5. `Evaluation` - 37 edges
6. `researchProject.Models.Entities` - 32 edges
7. `ResearchCategory` - 32 edges
8. `SuperAdminRepository` - 32 edges
9. `Researcher` - 31 edges
10. `CollegeCoAdmin` - 30 edges

## Surprising Connections (you probably didn't know these)
- `1. مهام التنفيذ` --references--> `ApplicationUser`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ApplicationUser.cs
- `7. Evaluation (Review) & Round Closing` --references--> `JudgeRepository`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Repositories/Implementations/JudgeRepository.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `AccountController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/AccountController.cs
- `1. مهام التنفيذ` --references--> `AccountController`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Controllers/AccountController.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `CollegeCoAdminController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/CollegeCoAdminController.cs

## Import Cycles
- None detected.

## Communities (51 total, 27 thin omitted)

### Community 0 - "Task"
Cohesion: 0.05
Nodes (8): AccountController, CollegeCoAdminController, JudgeController, ResearcherController, SuperAdminController, IAccountRepository, ICollegeCoAdminRepository, ISuperAdminRepository

### Community 1 - "researchProject.Models.Entities"
Cohesion: 0.06
Nodes (25): researchProject.Services, researchProject.Data, researchProject.Tests, researchProject.Repositories.Interfaces, researchProject.Controllers, researchProject.Models.DTOs, researchProject, researchProject.Models.Constants (+17 more)

### Community 2 - "Research"
Cohesion: 0.05
Nodes (41): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+33 more)

### Community 3 - "SystemStatisticsDto"
Cohesion: 0.05
Nodes (32): ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId, CollegeId, DynamicAnswers, InformedConsentFile, ResearchProposalFile (+24 more)

### Community 4 - "Evaluation"
Cohesion: 0.07
Nodes (32): 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, 2. اختبارات المرحلة السادسة, المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد), المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد), EvaluationSubmitDto, Decision (+24 more)

### Community 5 - "College"
Cohesion: 0.13
Nodes (12): College, Code, CollegeCoAdmins, Id, Name, ResearchCategories, Researchers, Researches (+4 more)

### Community 6 - "SuperAdminRepository"
Cohesion: 0.11
Nodes (7): 9. Reports & Statistics, ApplicationUser, FullName, Program, AccountRepository, SuperAdminRepository, ApplicationUser

### Community 7 - "4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)"
Cohesion: 0.14
Nodes (14): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, PublicController, CertificateVerificationResultDto, CollegeName, IssueDate, IsValid (+6 more)

### Community 8 - "ResearchStatusHistory"
Cohesion: 0.10
Nodes (21): 6. Research Status Workflow (Strict), 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الثامنة, 2. اختبارات المرحلة الرابعة, 2. اختبارات المرحلة السابعة, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر, المرحلة الرابعة (Phase 4): بوابة الباحث وتقديم البحث (+13 more)

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

### Community 14 - "Judge"
Cohesion: 0.16
Nodes (11): 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات, Judge, AcademicTitle, Evaluations, Id, IsActive (+3 more)

### Community 15 - "ApplicationDbContext"
Cohesion: 0.14
Nodes (12): ApplicationDbContext, CollegeCoAdmins, Colleges, Evaluations, JudgeInvitations, Judges, ResearchAttachments, ResearchCategories (+4 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.17
Nodes (11): JudgeInvitation, AcceptedAt, CreatedAt, Email, ExpiresAt, Id, InvitedByUser, InvitedByUserId (+3 more)

### Community 17 - "CollegeCoAdmin"
Cohesion: 0.20
Nodes (10): 1. مهام التنفيذ, 2. اختبارات المرحلة الثالثة, المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين), CollegeCoAdmin, College, CollegeId, Id, SignatureImagePath (+2 more)

### Community 18 - "ResearcherSignUpDto"
Cohesion: 0.18
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 19 - "ResearcherResearchesBridge"
Cohesion: 0.20
Nodes (8): 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, ResearcherResearchesBridge, Id, IsMainAuthor, Research, Researcher, ResearcherId, ResearchId

### Community 20 - "HomeController"
Cohesion: 0.20
Nodes (5): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId

### Community 21 - "AI Agent Instructions & Project Rules (`AGENTS.md`)"
Cohesion: 0.22
Nodes (8): 0. Identity Model Summary (READ FIRST), 10. Security & Data Isolation, 11. Seeding & Configuration, 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), 5. UI/UX, RTL, Right Sidebar & Partial Views, 7. Evaluation (Review) & Round Closing, AI Agent Instructions & Project Rules (`AGENTS.md`), DbSeeder

### Community 22 - "1. Entity & Database Rules (ERD Compliance)"
Cohesion: 0.39
Nodes (8): 1. Entity & Database Rules (ERD Compliance), AttachmentTypeConstants, EthicsRatingConstants, EvaluationDecisionConstants, InvitationStatusConstants, ResearcherTypeConstants, ResearchStatusConstants, RoleConstants

### Community 23 - "AcceptInvitationDto"
Cohesion: 0.33
Nodes (6): AcceptInvitationDto, AcademicTitle, ConfirmPassword, FullName, Password, Token

### Community 24 - "LoginDto"
Cohesion: 0.50
Nodes (4): LoginDto, Email, Password, RememberMe

## Knowledge Gaps
- **226 isolated node(s):** `net9.0`, `coverlet.collector (6.0.4)`, `Microsoft.NET.Test.Sdk (17.14.1)`, `xunit (2.9.3)`, `xunit.runner.visualstudio (3.1.4)` (+221 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 296 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **27 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `researchProject.Models.Constants` connect `researchProject.Models.Entities` to `1. Entity & Database Rules (ERD Compliance)`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **Are the 11 inferred relationships involving `Research` (e.g. with `1. Entity & Database Rules (ERD Compliance)` and `1. مهام التنفيذ`) actually correct?**
  _`Research` has 11 INFERRED edges - model-reasoned connections that need verification._
- **What connects `net9.0`, `coverlet.collector (6.0.4)`, `Microsoft.NET.Test.Sdk (17.14.1)` to the rest of the system?**
  _226 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Task` be split into smaller, more focused modules?**
  _Cohesion score 0.05212903225806451 - nodes in this community are weakly interconnected._
- **Why does `Research` connect `Research` to `Task`, `researchProject.Models.Entities`, `SystemStatisticsDto`, `Evaluation`, `College`, `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)`, `ResearchStatusHistory`, `Judge`, `ApplicationDbContext`, `ResearcherResearchesBridge`, `1. Entity & Database Rules (ERD Compliance)`?**
  _High betweenness centrality (0.073) - this node is a cross-community bridge._
- **Are the 13 inferred relationships involving `College` (e.g. with `1. Entity & Database Rules (ERD Compliance)` and `2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up)`) actually correct?**
  _`College` has 13 INFERRED edges - model-reasoned connections that need verification._
- **Should `researchProject.Models.Entities` be split into smaller, more focused modules?**
  _Cohesion score 0.05862584017923824 - nodes in this community are weakly interconnected._