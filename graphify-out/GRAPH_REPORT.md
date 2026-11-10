# Graph Report - researchProject  (2026-11-11)

## Corpus Check
- 222 files · ~113,410 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 1638 nodes · 4383 edges · 140 communities (64 shown, 76 thin omitted)
- Extraction: 82% EXTRACTED · 18% INFERRED · 0% AMBIGUOUS · INFERRED: 781 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a0f2c71e`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- IActionResult
- researchProject.Data
- Research
- ResearchCreateDto
- Evaluation
- Fact
- Program.Main
- CertificateVerificationResultViewModel
- ResearcherSignUpViewModel
- researchProject.Tests
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- Researcher
- http
- IEmailSender
- Judge
- ApplicationDbContext
- JudgeInvitation
- CollegeCoAdminController
- AuditLog
- ResearchStatusHistory
- HomeController
- ApplicationUser
- AuditLogViewModel
- Task
- HttpGet
- _DynamicCategoryFieldsPartial.cshtml
- site.js
- _ManageCollegesPartial.cshtml
- _ManageCoAdminsPartial.cshtml
- SuperAdmin/_ManageCategoriesPartial.cshtml
- system
- VerifyCertificate.cshtml
- Login.cshtml
- _CreateResearchPartial.cshtml
- _UploadModificationsPartial.cshtml
- _CollegeReportsPartial.cshtml
- CollegeCoAdmin/Index.cshtml
- Judge/Index.cshtml
- Researcher/Index.cshtml
- _MyCertificatesPartial.cshtml
- _CertificatesArchivePartial.cshtml
- researchProject.Models.Constants
- _ReportsPartial.cshtml
- ResearcherRepository
- CertificateService
- College
- AccountRepository
- ResearchAttachment
- INotificationService
- DateTime
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- system_threading_tasks
- SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType
- IFileStorageService
- SuperAdminController
- HangfireAuditAndErrorHandlingTests.cs
- 4. Comprehensive Cross-Reference Index
- NotificationService
- ResearchCategory
- NotificationAndSignalRTests.cs
- ProfileViewModel
- 2. Master Catalog of Verified Bugs
- ResearcherSignUpDto
- Notification
- NotificationViewModel
- notifications.js
- UserSummaryDto
- NewRequirementsAndEnhancementsTests
- NotificationHub
- CategorySaveDto
- EnumExtensions
- UserSummaryViewModel
- CollegeUpdateDto
- researchProject.Models.Enums
- ErrorController
- AttachmentType
- InvitationStatus
- UserRole
- ResearchStatus
- ResearchModificationViewModel
- VerifyEmailViewModel
- 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs
- AcceptInvitationDto
- EthicsRating
- EvaluationDecision
- 3. Architecture Rules Compliance Scorecard
- ResetPasswordDto
- VerifyEmailDto
- _ResearchStatusTrackerPartial.cshtml
- AcceptJudgeInvitation.cshtml
- ForgotPassword.cshtml
- Profile.cshtml
- ResetPassword.cshtml
- VerifyEmailToken.cshtml
- CollegeCoAdmin/Categories.cshtml
- CollegeCoAdmin/_ManageCategoriesPartial.cshtml
- CollegeCoAdmin/Reports.cshtml
- GeneralError.cshtml
- NotFound.cshtml
- Researcher/Certificates.cshtml
- Create.cshtml
- Modify.cshtml
- AuditLogs.cshtml
- _AuditLogsPartial.cshtml
- SuperAdmin/Categories.cshtml
- SuperAdmin/Certificates.cshtml
- CoAdmins.cshtml
- Colleges.cshtml
- _ManageResearchersPartial.cshtml
- SuperAdmin/Reports.cshtml
- Researchers.cshtml

## God Nodes (most connected - your core abstractions)
1. `College` - 64 edges
2. `ApplicationDbContext` - 61 edges
3. `AccountRepository` - 58 edges
4. `researchProject.Models.Constants` - 53 edges
5. `Judge` - 52 edges
6. `ApplicationUser` - 50 edges
7. `SuperAdminController` - 49 edges
8. `CollegeCoAdminRepository` - 48 edges
9. `researchProject.Models.Entities` - 47 edges
10. `ResearchCategory` - 47 edges

## Surprising Connections (you probably didn't know these)
- `3.3 Detailed Audit: Rule 3 — Single Type Per File Principle` --references--> `UserSummaryViewModel`  [INFERRED]
  BUGS.md → researchProject/Models/ViewModels/UserSummaryViewModel.cs
- `7. Evaluation (Review) & Round Closing` --references--> `JudgeRepository`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Repositories/Implementations/JudgeRepository.cs
- `BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions` --references--> `IReportExportService`  [INFERRED]
  BUGS.md → researchProject/Services/IReportExportService.cs
- `BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges` --references--> `Evaluation`  [INFERRED]
  BUGS.md → researchProject/Models/Entities/Evaluation.cs
- `2. اختبارات المرحلة السابعة` --references--> `ResearchStatusHistory`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ResearchStatusHistory.cs

## Import Cycles
- None detected.

## Communities (140 total, 76 thin omitted)

### Community 1 - "researchProject.Data"
Cohesion: 0.18
Nodes (5): researchProject.Services, researchProject.Data, researchProject.Tests, researchProject.Repositories.Implementations, Microsoft.AspNetCore.Identity

### Community 2 - "Research"
Cohesion: 0.10
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 3 - "ResearchCreateDto"
Cohesion: 0.05
Nodes (37): ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId, CollegeId, DynamicAnswers, DynamicFields, InformedConsentFile (+29 more)

### Community 4 - "Evaluation"
Cohesion: 0.10
Nodes (22): 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, 2. اختبارات المرحلة السادسة, المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد), المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد), Evaluation, Decision (+14 more)

### Community 5 - "Fact"
Cohesion: 0.20
Nodes (3): CollegeCoAdminRepository, EntityAndDatabaseRulesTests, ExtendedWorkflowAndEdgeCaseTests

### Community 6 - "Program.Main"
Cohesion: 0.11
Nodes (6): AuditLogFilter, Program, UnconfirmedEmailCleanupBackgroundService, HangfireDashboardAuthorizationFilter, HangfireEmailDispatcher, IHangfireEmailDispatcher

### Community 7 - "CertificateVerificationResultViewModel"
Cohesion: 0.12
Nodes (14): 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, 2. اختبارات المرحلة السابعة, المرحلة السابعة (Phase 7): الاعتماد النهائي وإصدار الشهادة والتحقق, PublicController, CertificateVerificationResultViewModel, CollegeName, IssueDate (+6 more)

### Community 8 - "ResearcherSignUpViewModel"
Cohesion: 0.05
Nodes (40): BUG-31: Input DTOs Used Directly as Models in AccountController Razor Views, AcceptInvitationViewModel, AcademicTitle, ConfirmPassword, Email, ErrorMessage, FullName, IsExistingUser (+32 more)

### Community 9 - "researchProject.Tests"
Cohesion: 0.11
Nodes (18): net9.0, coverlet.collector (6.0.4), Hangfire.AspNetCore (1.8.25), Hangfire.SqlServer (1.8.25), Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.2), Microsoft.EntityFrameworkCore.Design (9.0.2), Microsoft.EntityFrameworkCore.InMemory (9.0.2), Microsoft.EntityFrameworkCore.SqlServer (9.0.2) (+10 more)

### Community 10 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.11
Nodes (17): 1. الهدف من المشروع (Project Objective), ① بوابة الباحثين (Researchers Portal), 2. الهيكل العام والمقترح التقني (System Architecture), ② بوابة المحكمين وأعضاء اللجنة (Reviewers & Committee Portal), 3. دورة العمل الرقمية لإصدار الشهادة الأخلاقية (Workflow), ③ لوحة التحكم الإدارية (Admin Dashboard), 4. أدوار النظام وارتباطها بالبوابات, 5. المحكمون: الباحث كمحكم ونظام الدعوات (+9 more)

### Community 11 - "Researcher"
Cohesion: 0.13
Nodes (15): 1. مهام التنفيذ, 2. اختبارات المرحلة الثانية, المرحلة الثانية (Phase 2): الدخول الموحد والتسجيل بالبريد المؤسسي والواجهة العربية, Researcher, AcademicDegree, College, CollegeId, Department (+7 more)

### Community 12 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 14 - "Judge"
Cohesion: 0.08
Nodes (26): 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, 2. اختبارات المرحلة الثالثة, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات, المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين), الهيكل العام للمشروع (Solution Folder Structure), خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan), CollegeCoAdmin (+18 more)

### Community 15 - "ApplicationDbContext"
Cohesion: 0.07
Nodes (27): ApplicationDbContext, AuditLogs, CollegeCoAdmins, Colleges, Departments, EmailVerificationCodes, Evaluations, JudgeInvitations (+19 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.14
Nodes (13): JudgeInvitation, AcceptedAt, College, CollegeId, CreatedAt, Email, ExpiresAt, Id (+5 more)

### Community 17 - "CollegeCoAdminController"
Cohesion: 0.11
Nodes (3): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), CollegeCoAdminController, ICollegeCoAdminRepository

### Community 18 - "AuditLog"
Cohesion: 0.08
Nodes (22): AuditLog, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+14 more)

### Community 19 - "ResearchStatusHistory"
Cohesion: 0.07
Nodes (33): 0. Identity Model Summary (READ FIRST), 10. Security & Data Isolation, 11. Seeding & Configuration, 1. Entity & Database Rules (ERD Compliance), 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 5. UI/UX, RTL, Right Sidebar & Partial Views, 6. Research Status Workflow (Strict), 7. Evaluation (Review) & Round Closing (+25 more)

### Community 20 - "HomeController"
Cohesion: 0.20
Nodes (5): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId

### Community 21 - "ApplicationUser"
Cohesion: 0.13
Nodes (8): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), 1. مهام التنفيذ, 2. اختبارات المرحلة الثامنة, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر, ApplicationUser, CreatedAt, FullName, IsActive

### Community 22 - "AuditLogViewModel"
Cohesion: 0.07
Nodes (27): AuditLogListViewModel, CurrentPage, FromDate, Logs, RoleFilter, SearchTerm, SuccessFilter, ToDate (+19 more)

### Community 23 - "Task"
Cohesion: 0.06
Nodes (8): Department, College, CollegeId, Id, Name, SuperAdminRepository, IAccountRepository, ISuperAdminRepository

### Community 24 - "HttpGet"
Cohesion: 0.11
Nodes (3): BUG-11: SuperAdmin Proposal Creation Fails with Null Profile and Hardcoded College Fallback, ResearcherController, IResearcherRepository

### Community 26 - "site.js"
Cohesion: 0.28
Nodes (3): initCustomDatalists(), initDedicatedDropdowns(), resolveInitialRoute()

### Community 30 - "system"
Cohesion: 0.08
Nodes (5): researchProject.Models.ViewModels, researchProject.Models.DTOs, researchProject.Models.Entities, ResendVerificationCodeDto, EmailOrUserId

### Community 41 - "researchProject.Models.Constants"
Cohesion: 0.03
Nodes (30): researchProject.Models.Constants, AttachmentTypeConstants, EthicsRatingConstants, EvaluationDecisionConstants, InvitationStatusConstants, ResearcherTypeConstants, List<researchProject.Models.Entities.Research>, List<researchProject.Models.Entities.Research> (+22 more)

### Community 52 - "ResearcherRepository"
Cohesion: 0.06
Nodes (33): BUG-01: Missing Attachment Download Endpoint and UI Action Links (Functional Blocker), BUG-02: Missing Validation on Inactive/Invalid Judges Traps Research in `AssignedToJudges` with 0 Evaluations (Deadlock), BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges, BUG-04: Round Number Corruption and Previous Judge Lockout on Research Re-assignment, BUG-05: Assigning Judges During `ModificationsRequired` State Locks Researcher Out, BUG-07: Premature Verification OTP Invalidation (Burned) Before Password Complexity Check, BUG-09: Unhandled `FormatException` in `ResendVerificationCode` When Passing `UserId` Instead of Email, BUG-10: Arbitrary Evaluation Decision Value Bypass Defaulting to Modifications Required (+25 more)

### Community 53 - "CertificateService"
Cohesion: 0.22
Nodes (7): BUG-21: Concurrent OTP Brute-Force Race Condition Bypassing 5-Attempt Lockout Limit, BUG-22: Stored Cross-Site Scripting (XSS) and Raw HTML Injection in Generated Certificates, BUG-23: Arbitrary File Disclosure via Path Traversal in Certificate Signatures and File Streaming, BUG-24: Insecure Storage of Authentication Verification OTP Codes in Plaintext in Database, BUG-25: Arithmetic Overflow `Math.Abs(int.MinValue)` in Certificate Serial Number Generation, Category 3: Security & Input Validation, CertificateService

### Community 54 - "College"
Cohesion: 0.16
Nodes (12): BUG-08: Digital Certificate Issuance Bypasses Mandatory Co-Admin Signature Image Requirement (Specification Conflict), College, Code, CollegeCoAdmins, Departments, Id, Name, ResearchCategories (+4 more)

### Community 56 - "ResearchAttachment"
Cohesion: 0.17
Nodes (12): ResearchAttachment, AttachmentType, ContentType, FilePath, FileSize, Id, OriginalFileName, Research (+4 more)

### Community 57 - "INotificationService"
Cohesion: 0.12
Nodes (5): 3.1 Detailed Audit: Rule 1 — Thin Controllers Principle, JudgeController, NotificationController, IJudgeRepository, INotificationService

### Community 58 - "DateTime"
Cohesion: 0.07
Nodes (12): researchProject.Migrations, InitialCreate, init1, AddDepartments, AddEmailVerificationCodeTable, UpdateEmailVerificationCodeLength, AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers (+4 more)

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

### Community 61 - "SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType"
Cohesion: 0.17
Nodes (10): ChangePasswordDto, ConfirmPassword, CurrentPassword, NewPassword, UpdateProfileDto, AcademicDegree, FullName, PhoneNumber (+2 more)

### Community 62 - "IFileStorageService"
Cohesion: 0.19
Nodes (3): BUG-36: Complete Absence of Test Coverage (0%) for Core Security, Auth, Storage, and Controllers, FileStorageService, IFileStorageService

### Community 65 - "4. Comprehensive Cross-Reference Index"
Cohesion: 0.20
Nodes (6): 4. Comprehensive Cross-Reference Index, Excluded / Resolved Items Reference Table, AttachmentController, AttachmentService, IAttachmentService, AttachmentAndSecurityFixTests

### Community 67 - "ResearchCategory"
Cohesion: 0.14
Nodes (12): BUG-32: Input DTOs Used Directly as Models in ResearcherController Razor Views, BUG-35: EF Core Entities Used Directly as Presentation Models across Views, Category 7: Architecture Rule 4 — Separation Between ViewModels and DTOs, ResearchCategory, College, CollegeId, Description, FieldsDefinitionJson (+4 more)

### Community 68 - "NotificationAndSignalRTests.cs"
Cohesion: 0.22
Nodes (4): researchProject.Services.Interfaces, researchProject.Hubs, researchProject.Services.Implementations, researchProject

### Community 69 - "ProfileViewModel"
Cohesion: 0.15
Nodes (13): ProfileViewModel, AcademicDegree, CollegeName, Department, Email, FullName, HasSignature, NationalId (+5 more)

### Community 70 - "2. Master Catalog of Verified Bugs"
Cohesion: 0.18
Nodes (11): 2. Master Catalog of Verified Bugs, BUG-26: Redundant Group Subscription in `NotificationHub` Desynchronized from `Clients.User` Dispatch, BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions, BUG-28: Direct Form Data Key Iteration and Dynamic Field Parsing in Controller Action, BUG-29: Controllers Executing Direct Domain Profile Resolution via Repositories, BUG-30: Authentication Cookie and Claims Not Refreshed upon Accepting Judge Role, Category 4: SignalR & Real-Time Communication, Category 5: Architecture Rule 1 — Thin Controllers Principle (+3 more)

### Community 71 - "ResearcherSignUpDto"
Cohesion: 0.18
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 72 - "Notification"
Cohesion: 0.18
Nodes (11): Notification, CreatedAt, Icon, Id, IsRead, Message, TargetUrl, Title (+3 more)

### Community 73 - "NotificationViewModel"
Cohesion: 0.18
Nodes (10): NotificationViewModel, CreatedAt, Icon, Id, IsRead, Message, TargetUrl, TimeAgo (+2 more)

### Community 74 - "notifications.js"
Cohesion: 0.36
Nodes (10): buildNotificationItemHtml(), escapeHtml(), handleIncomingNotification(), initSignalR(), loadRecentNotifications(), loadUnreadCount(), markAndNavigate(), playSoftBeep() (+2 more)

### Community 75 - "UserSummaryDto"
Cohesion: 0.20
Nodes (9): 1.1 Key Metrics & Remediation Status, 1.2 Breakdown by Severity, 1.3 Breakdown by Category, 1. Executive Summary & Statistics Tables, BUG-34: Empty Shell DTO Subclasses ViewModel Violating Layer Isolation, UserSummaryDto, Email, FullName (+1 more)

### Community 77 - "NotificationHub"
Cohesion: 0.22
Nodes (5): BUG-37: Artificial Unit Test Scenario Concealing Round Incrementation Defect, BUG-38: Overly Broad SignalR Test Mocks Resulting in False-Positive Verification, BUG-39: Test Suite Omits Primary 6-Digit OTP Verification Flow, Category 8: Test Suite Gaps & Blind Spots, NotificationHub

### Community 78 - "CategorySaveDto"
Cohesion: 0.25
Nodes (7): CategorySaveDto, CollegeId, Description, FieldsDefinitionJson, Id, IsActive, Name

### Community 79 - "EnumExtensions"
Cohesion: 0.25
Nodes (5): EnumExtensions, ResearcherType, FacultyMember, PostgraduateStudent, Researcher

### Community 80 - "UserSummaryViewModel"
Cohesion: 0.25
Nodes (6): UserSummaryViewModel, Email, FullName, Id, RoleName, UserId

### Community 81 - "CollegeUpdateDto"
Cohesion: 0.25
Nodes (8): BUG-33: EF Core Domain Entities Bound Directly as Controller Action Input Parameters (Mass Assignment), CollegeCreateDto, Code, Name, CollegeUpdateDto, Code, Id, Name

### Community 82 - "researchProject.Models.Enums"
Cohesion: 0.25
Nodes (3): researchProject.Models.Enums, RoleConstants, researchProject.Models.ViewModels.ResearcherSignUpViewModel

### Community 84 - "AttachmentType"
Cohesion: 0.25
Nodes (6): AttachmentType, AdditionalDocument, InformedConsent, ModifiedFile, ResearchProposal, ResearchTools

### Community 85 - "InvitationStatus"
Cohesion: 0.25
Nodes (5): InvitationStatus, Accepted, Expired, Pending, Revoked

### Community 86 - "UserRole"
Cohesion: 0.25
Nodes (5): UserRole, CollegeCoAdmin, Judge, Researcher, SuperAdmin

### Community 87 - "ResearchStatus"
Cohesion: 0.25
Nodes (6): ResearchStatus, Approved, AssignedToJudges, CertificateIssued, ModificationsRequired, UnderFormalScreening

### Community 88 - "ResearchModificationViewModel"
Cohesion: 0.25
Nodes (8): ResearchModificationViewModel, CurrentRound, ErrorMessage, ModificationNotes, Research, ResearcherNotes, ResearchId, Title

### Community 89 - "VerifyEmailViewModel"
Cohesion: 0.25
Nodes (7): VerifyEmailViewModel, EmailOrUserId, ErrorMessage, HasPendingCode, SuccessMessage, Token, ValidityMinutes

### Community 90 - "3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs"
Cohesion: 0.29
Nodes (7): 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs, ForgotPasswordDto, Email, LoginDto, Email, Password, RememberMe

### Community 91 - "AcceptInvitationDto"
Cohesion: 0.29
Nodes (7): BUG-06: Deceptive Success and Password Desynchronization on Logged-Out Existing User Judge Invitation Acceptance, AcceptInvitationDto, AcademicTitle, ConfirmPassword, FullName, Password, Token

### Community 92 - "EthicsRating"
Cohesion: 0.29
Nodes (4): EthicsRating, Appropriate, Inappropriate, NeedsModification

### Community 93 - "EvaluationDecision"
Cohesion: 0.29
Nodes (5): EvaluationDecision, Approved, ClosedDueToOtherJudge, ModificationsRequired, UnderEvaluation

### Community 94 - "3. Architecture Rules Compliance Scorecard"
Cohesion: 0.33
Nodes (5): 3.2 Detailed Audit: Rule 2 — Native ASP.NET Core Identity & Claims, 3.3 Detailed Audit: Rule 3 — Single Type Per File Principle, 3. Architecture Rules Compliance Scorecard, Comprehensive Code Review & Verified Defect Catalog: `researchProject`, Scorecard Summary Table

### Community 95 - "ResetPasswordDto"
Cohesion: 0.40
Nodes (5): ResetPasswordDto, ConfirmPassword, Email, NewPassword, Token

### Community 97 - "VerifyEmailDto"
Cohesion: 0.50
Nodes (3): VerifyEmailDto, EmailOrUserId, Token

## Knowledge Gaps
- **484 isolated node(s):** `Colleges`, `Departments`, `CollegeCoAdmins`, `Researchers`, `ResearchCategories` (+479 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 653 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **76 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `researchProject.Models.Constants` connect `researchProject.Models.Constants` to `HangfireAuditAndErrorHandlingTests.cs`, `researchProject.Data`, `NotificationAndSignalRTests.cs`, `researchProject.Models.Enums`, `ResearchStatusHistory`, `system_threading_tasks`?**
  _High betweenness centrality (0.080) - this node is a cross-community bridge._
- **Are the 34 inferred relationships involving `College` (e.g. with `1. Entity & Database Rules (ERD Compliance)` and `2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up)`) actually correct?**
  _`College` has 34 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Colleges`, `Departments`, `CollegeCoAdmins` to the rest of the system?**
  _484 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `IActionResult` be split into smaller, more focused modules?**
  _Cohesion score 0.12408163265306123 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `researchProject.Data`, `Fact`, `Program.Main`, `Researcher`, `Judge`, `JudgeInvitation`, `CollegeCoAdminController`, `AuditLog`, `ResearchStatusHistory`, `ApplicationUser`, `Task`, `List`, `ResearcherRepository`, `College`, `AccountRepository`, `ResearchAttachment`, `INotificationService`, `SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType`, `4. Comprehensive Cross-Reference Index`, `NotificationService`, `ResearchCategory`, `Notification`, `NewRequirementsAndEnhancementsTests`, `3. Architecture Rules Compliance Scorecard`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Should `Research` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._
- **Why does `Evaluation` connect `Evaluation` to `Research`, `ResearchCategory`, `DateTime`, `Fact`, `Judge`, `ResearchStatusHistory`, `ResearcherRepository`, `INotificationService`, `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs`, `system`?**
  _High betweenness centrality (0.042) - this node is a cross-community bridge._