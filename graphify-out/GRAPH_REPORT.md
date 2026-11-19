# Graph Report - researchProject  (2026-11-19)

## Corpus Check
- 239 files · ~125,567 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 1816 nodes · 4535 edges · 169 communities (73 shown, 96 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 719 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0b5a3381`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- microsoft_entityframeworkcore
- Research
- IActionResult
- Evaluation
- ExtendedWorkflowAndEdgeCaseTests
- ILogger
- CertificateVerificationResultViewModel
- ResearcherSignUpViewModel
- researchProject.Tests
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- Researcher
- http
- CollegeCoAdminController
- SignUp.cshtml
- ApplicationDbContext
- JudgeInvitation
- HangfireEmailDispatcher.SendEmailWithRollbackAsync
- Fact
- ResearchStatusHistory
- SeoMetadataViewModel
- SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType
- AuditLogViewModel
- SuperAdminRepository
- IActionResult
- _DynamicCategoryFieldsPartial.cshtml
- site.js
- _ManageCollegesPartial.cshtml
- _ManageCoAdminsPartial.cshtml
- SuperAdmin/_ManageCategoriesPartial.cshtml
- system_threading_tasks
- researchProject.Models.ViewModels.CertificateVerificationResultViewModel
- Login.cshtml
- _CreateResearchPartial.cshtml
- _UploadModificationsPartial.cshtml
- _CollegeReportsPartial.cshtml
- CollegeCoAdmin/Index.cshtml
- Judge/Index.cshtml
- Researcher/Index.cshtml
- _MyCertificatesPartial.cshtml
- _CertificatesArchivePartial.cshtml
- system_threading_tasks
- _ReportsPartial.cshtml
- HangfireAuditAndErrorHandlingTests.cs
- Category 1: Functional & Runtime Bugs
- CertificateService
- CollegeCoAdminRepository
- AccountRepository
- ResearchAttachment
- JudgeRepository
- EmailSender
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- ResearcherRepository.CreateResearchWithAttachmentsAsync
- AuditLogListViewModel
- SystemStatisticsViewModel
- IFileStorageService
- Program.cs
- ApplicationUser
- Notification
- Judge
- DateTime
- Controller
- AccountRepository
- ResearcherSignUpDto
- UserProfileViewModel
- AccountController
- notifications.js
- system
- ResearchCategory
- CollegeCoAdmin
- IResearcherRepository
- EnumExtensions
- ProfileViewModel
- ResearchCreateDto
- researchProject.Models.Enums
- ErrorController
- AttachmentType
- InvitationStatus
- UserRole
- ResearchStatus
- ResearchModificationViewModel
- ResearchModificationDto
- 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs
- AcceptInvitationDto
- EthicsRating
- EvaluationDecision
- UserSummaryDto
- OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin
- NewRequirementsAndEnhancementsTests
- VerifyEmailViewModel
- _ResearchStatusTrackerPartial.cshtml
- NotificationAndSignalRTests.cs
- researchProject.Models.ViewModels.AcceptInvitationViewModel
- researchProject.Models.ViewModels.ForgotPasswordViewModel
- researchProject.Models.ViewModels.ProfileViewModel
- researchProject.Models.ViewModels.ResetPasswordViewModel
- researchProject.Models.ViewModels.VerifyEmailViewModel
- CollegeCoAdmin/Categories.cshtml
- CollegeCoAdmin/_ManageCategoriesPartial.cshtml
- CollegeCoAdmin/Reports.cshtml
- GeneralError.cshtml
- NotFound.cshtml
- Researcher/Certificates.cshtml
- Create.cshtml
- Modify.cshtml
- HangfireEmailDispatcher
- EmailVerificationCode
- SuperAdmin/Categories.cshtml
- SuperAdmin/Certificates.cshtml
- CoAdmins.cshtml
- List<researchProject.Models.Entities.College>
- _ManageResearchersPartial.cshtml
- SuperAdmin/Reports.cshtml
- Researchers.cshtml
- UserSummaryViewModel
- 3. Architecture Rules Compliance Scorecard
- AuditLog
- ResetPasswordDto
- ILogger
- AuditLogs.cshtml
- ChangePasswordDto
- ResearchCreateViewModel
- 2. Master Catalog of Verified Bugs
- JudgeToResearcherDto
- HangfireDashboardAuthorizationFilter
- _ResearchReviewPartial.cshtml
- ResearchDetails.cshtml
- _ResearchDetailsPartial.cshtml
- _RegisterAsResearcherModalPartial.cshtml
- _UserProfilePartial.cshtml
- _AuditLogsPartial.cshtml
- Review.cshtml
- ScreeningQueue.cshtml
- _ScreeningQueuePartial.cshtml
- AssignedResearches.cshtml
- _AssignedResearchesPartial.cshtml
- Evaluate.cshtml
- _EvaluationFormPartial.cshtml
- MyResearches.cshtml
- _MyResearchesPartial.cshtml
- Tracker.cshtml
- _AllUniversityResearchesPartial.cshtml
- SuperAdmin/Index.cshtml
- Judges.cshtml
- _ManageJudgesPartial.cshtml
- _OverviewDashboardPartial.cshtml
- Researches.cshtml
- _SeoMetaTagsPartial.cshtml

## God Nodes (most connected - your core abstractions)
1. `AccountRepository` - 60 edges
2. `ApplicationDbContext` - 54 edges
3. `SuperAdminController` - 48 edges
4. `SuperAdminRepository` - 47 edges
5. `Judge` - 44 edges
6. `ResearchCategory` - 40 edges
7. `ISuperAdminRepository` - 39 edges
8. `College` - 39 edges
9. `CollegeCoAdminRepository` - 38 edges
10. `Evaluation` - 37 edges

## Surprising Connections (you probably didn't know these)
- `2. اختبارات المرحلة السابعة` --references--> `ResearchStatusHistory`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ResearchStatusHistory.cs
- `BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges` --references--> `Evaluation`  [INFERRED]
  BUGS.md → researchProject/Models/Entities/Evaluation.cs
- `2. اختبارات المرحلة الأولى` --references--> `Judge`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/Judge.cs
- `3.3 Detailed Audit: Rule 3 — Single Type Per File Principle` --references--> `UserSummaryViewModel`  [INFERRED]
  BUGS.md → researchProject/Models/ViewModels/UserSummaryViewModel.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `SuperAdminController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/SuperAdminController.cs

## Import Cycles
- None detected.

## Communities (169 total, 96 thin omitted)

### Community 1 - "microsoft_entityframeworkcore"
Cohesion: 0.13
Nodes (13): researchProject.Data, researchProject.Tests, researchProject.Models.DTOs, researchProject.Models.Constants, researchProject.Repositories.Implementations, Microsoft.AspNetCore.Identity, AttachmentTypeConstants, EthicsRatingConstants (+5 more)

### Community 2 - "Research"
Cohesion: 0.09
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 4 - "Evaluation"
Cohesion: 0.11
Nodes (19): 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد), Evaluation, Decision, Id, InformedConsentRating, IsDigitallySigned (+11 more)

### Community 7 - "CertificateVerificationResultViewModel"
Cohesion: 0.14
Nodes (13): 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, PublicController, CertificateVerificationResultViewModel, CollegeName, IssueDate, IsValid, Message (+5 more)

### Community 8 - "ResearcherSignUpViewModel"
Cohesion: 0.04
Nodes (42): BUG-31: Input DTOs Used Directly as Models in AccountController Razor Views, BUG-35: EF Core Entities Used Directly as Presentation Models across Views, Category 7: Architecture Rule 4 — Separation Between ViewModels and DTOs, AcceptInvitationViewModel, AcademicTitle, ConfirmPassword, Email, ErrorMessage (+34 more)

### Community 9 - "researchProject.Tests"
Cohesion: 0.11
Nodes (18): net9.0, coverlet.collector (6.0.4), Hangfire.AspNetCore (1.8.25), Hangfire.SqlServer (1.8.25), Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.2), Microsoft.EntityFrameworkCore.Design (9.0.2), Microsoft.EntityFrameworkCore.InMemory (9.0.2), Microsoft.EntityFrameworkCore.SqlServer (9.0.2) (+10 more)

### Community 10 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.11
Nodes (17): 1. الهدف من المشروع (Project Objective), ① بوابة الباحثين (Researchers Portal), 2. الهيكل العام والمقترح التقني (System Architecture), ② بوابة المحكمين وأعضاء اللجنة (Reviewers & Committee Portal), 3. دورة العمل الرقمية لإصدار الشهادة الأخلاقية (Workflow), ③ لوحة التحكم الإدارية (Admin Dashboard), 4. أدوار النظام وارتباطها بالبوابات, 5. المحكمون: الباحث كمحكم ونظام الدعوات (+9 more)

### Community 11 - "Researcher"
Cohesion: 0.10
Nodes (16): 1. مهام التنفيذ, 2. اختبارات المرحلة الثانية, المرحلة الثانية (Phase 2): الدخول الموحد والتسجيل بالبريد المؤسسي والواجهة العربية, Researcher, AcademicDegree, College, CollegeId, Department (+8 more)

### Community 12 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 14 - "SignUp.cshtml"
Cohesion: 0.40
Nodes (4): researchProject.Models.ViewModels.ResearcherSignUpViewModel, College, researchProject.Models.Entities, researchProject.Models.Enums

### Community 15 - "ApplicationDbContext"
Cohesion: 0.08
Nodes (21): ApplicationDbContext, AuditLogs, CollegeCoAdmins, Colleges, Departments, EmailVerificationCodes, Evaluations, JudgeInvitations (+13 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.15
Nodes (13): JudgeInvitation, AcceptedAt, College, CollegeId, CreatedAt, Email, ExpiresAt, Id (+5 more)

### Community 17 - "HangfireEmailDispatcher.SendEmailWithRollbackAsync"
Cohesion: 0.14
Nodes (7): EmailJobType, CoAdminCredentials, JudgeInvitation, UnconfirmedEmailCleanupService, IUnconfirmedEmailCleanupService, HangfireAuditAndErrorHandlingTests, ApplicationUser

### Community 19 - "ResearchStatusHistory"
Cohesion: 0.07
Nodes (31): 0. Identity Model Summary (READ FIRST), 10. Security & Data Isolation, 11. Seeding & Configuration, 1. Entity & Database Rules (ERD Compliance), 5. UI/UX, RTL, Right Sidebar & Partial Views, 6. Research Status Workflow (Strict), 9. Reports & Statistics, AI Agent Instructions & Project Rules (`AGENTS.md`) (+23 more)

### Community 20 - "SeoMetadataViewModel"
Cohesion: 0.09
Nodes (15): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId, SeoMetadataViewModel, Author, CanonicalUrl (+7 more)

### Community 21 - "SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType"
Cohesion: 0.15
Nodes (6): UpdateProfileDto, AcademicDegree, FullName, PhoneNumber, ResearcherType, SevenEnhancementsAndFixesTests

### Community 22 - "AuditLogViewModel"
Cohesion: 0.12
Nodes (17): AuditLogViewModel, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 24 - "IActionResult"
Cohesion: 0.11
Nodes (3): BUG-11: SuperAdmin Proposal Creation Fails with Null Profile and Hardcoded College Fallback, ResearcherController, SuperAdminController

### Community 26 - "site.js"
Cohesion: 0.18
Nodes (6): initCustomDatalists(), initDedicatedDropdowns(), filterOptions(), getItems(), initUniversalTablePagination(), resolveInitialRoute()

### Community 30 - "system_threading_tasks"
Cohesion: 0.19
Nodes (3): researchProject.Models.ViewModels, researchProject.Repositories.Interfaces, researchProject.Models.Entities

### Community 32 - "Login.cshtml"
Cohesion: 0.40
Nodes (4): researchProject.Models.ViewModels.LoginViewModel, College, researchProject.Models.Entities, researchProject.Models.Enums

### Community 41 - "system_threading_tasks"
Cohesion: 0.18
Nodes (3): researchProject.Controllers, researchProject.Filters, researchProject.Models.Constants

### Community 52 - "Category 1: Functional & Runtime Bugs"
Cohesion: 0.18
Nodes (11): BUG-01: Missing Attachment Download Endpoint and UI Action Links (Functional Blocker), BUG-02: Missing Validation on Inactive/Invalid Judges Traps Research in `AssignedToJudges` with 0 Evaluations (Deadlock), BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges, BUG-04: Round Number Corruption and Previous Judge Lockout on Research Re-assignment, BUG-05: Assigning Judges During `ModificationsRequired` State Locks Researcher Out, BUG-07: Premature Verification OTP Invalidation (Burned) Before Password Complexity Check, BUG-09: Unhandled `FormatException` in `ResendVerificationCode` When Passing `UserId` Instead of Email, BUG-12: Certificate Archive Date Range Filter Truncates Matches on Target End Date (+3 more)

### Community 53 - "CertificateService"
Cohesion: 0.24
Nodes (7): BUG-21: Concurrent OTP Brute-Force Race Condition Bypassing 5-Attempt Lockout Limit, BUG-22: Stored Cross-Site Scripting (XSS) and Raw HTML Injection in Generated Certificates, BUG-23: Arbitrary File Disclosure via Path Traversal in Certificate Signatures and File Streaming, BUG-24: Insecure Storage of Authentication Verification OTP Codes in Plaintext in Database, BUG-25: Arithmetic Overflow `Math.Abs(int.MinValue)` in Certificate Serial Number Generation, Category 3: Security & Input Validation, CertificateService

### Community 54 - "CollegeCoAdminRepository"
Cohesion: 0.18
Nodes (12): BUG-08: Digital Certificate Issuance Bypasses Mandatory Co-Admin Signature Image Requirement (Specification Conflict), College, Code, CollegeCoAdmins, Departments, Id, Name, ResearchCategories (+4 more)

### Community 56 - "ResearchAttachment"
Cohesion: 0.17
Nodes (12): ResearchAttachment, AttachmentType, ContentType, FilePath, FileSize, Id, OriginalFileName, Research (+4 more)

### Community 57 - "JudgeRepository"
Cohesion: 0.18
Nodes (10): BUG-10: Arbitrary Evaluation Decision Value Bypass Defaulting to Modifications Required, BUG-18: Race Condition and Lost Updates in Concurrent Multi-Judge Evaluation Submissions, EvaluationSubmitDto, Decision, EvaluationId, InformedConsentRating, ModificationNotes, PrivacyProtectionRating (+2 more)

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

### Community 60 - "ResearcherRepository.CreateResearchWithAttachmentsAsync"
Cohesion: 0.22
Nodes (5): BUG-16: Invalid `RollbackAsync` Invoked on Already Committed Database Transaction on Notification Failure, BUG-17: Unhandled SignalR Exception in Judge Evaluation Transaction Rolls Back Official Evaluation, BUG-19: Missing College Isolation Validation Enables Cross-College Proposal Injection, BUG-20: N+1 Database Query Cascade in SuperAdmin Role Retrieval for Judge Invitations, Category 2: Concurrency, Transactions & Data Integrity

### Community 61 - "AuditLogListViewModel"
Cohesion: 0.12
Nodes (12): DeveloperController, AuditLogListViewModel, ActionFilter, CurrentPage, FromDate, Logs, RoleFilter, SearchTerm (+4 more)

### Community 62 - "SystemStatisticsViewModel"
Cohesion: 0.05
Nodes (38): 4. Comprehensive Cross-Reference Index, BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions, BUG-28: Direct Form Data Key Iteration and Dynamic Field Parsing in Controller Action, BUG-29: Controllers Executing Direct Domain Profile Resolution via Repositories, BUG-33: EF Core Domain Entities Bound Directly as Controller Action Input Parameters (Mass Assignment), Category 5: Architecture Rule 1 — Thin Controllers Principle, Excluded / Resolved Items Reference Table, CategorySaveDto (+30 more)

### Community 63 - "IFileStorageService"
Cohesion: 0.20
Nodes (3): BUG-36: Complete Absence of Test Coverage (0%) for Core Security, Auth, Storage, and Controllers, FileStorageService, IFileStorageService

### Community 65 - "ApplicationUser"
Cohesion: 0.16
Nodes (11): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), 1. مهام التنفيذ, 2. اختبارات المرحلة الثامنة, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر, ApplicationUser, CreatedAt, FullName, IsActive (+3 more)

### Community 66 - "Notification"
Cohesion: 0.06
Nodes (28): BUG-26: Redundant Group Subscription in `NotificationHub` Desynchronized from `Clients.User` Dispatch, BUG-38: Overly Broad SignalR Test Mocks Resulting in False-Positive Verification, Category 4: SignalR & Real-Time Communication, NotificationHub, Notification, CreatedAt, Icon, Id (+20 more)

### Community 67 - "Judge"
Cohesion: 0.11
Nodes (17): 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 7. Evaluation (Review) & Round Closing, 1. مهام التنفيذ, 2. اختبارات المرحلة السادسة, المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد), Judge, AcademicTitle, College (+9 more)

### Community 68 - "DateTime"
Cohesion: 0.07
Nodes (14): researchProject.Migrations, InitialCreate, init1, AddDepartments, AddEmailVerificationCodeTable, UpdateEmailVerificationCodeLength, AddNationalIdUniqueAndUserCreatedAt, AddNationalIdUniqueAndUserCreatedAt (+6 more)

### Community 69 - "Controller"
Cohesion: 0.15
Nodes (4): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), AttachmentController, JudgeController, IJudgeRepository

### Community 71 - "ResearcherSignUpDto"
Cohesion: 0.17
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 72 - "UserProfileViewModel"
Cohesion: 0.10
Nodes (20): UserProfileViewModel, AvailableColleges, CanBecomeResearcher, Email, FullName, IsCollegeCoAdmin, IsJudge, IsResearcher (+12 more)

### Community 74 - "notifications.js"
Cohesion: 0.36
Nodes (10): buildNotificationItemHtml(), escapeHtml(), handleIncomingNotification(), initSignalR(), loadRecentNotifications(), loadUnreadCount(), markAndNavigate(), playSoftBeep() (+2 more)

### Community 75 - "system"
Cohesion: 0.17
Nodes (3): researchProject.Models.Entities, ResendVerificationCodeDto, EmailOrUserId

### Community 76 - "ResearchCategory"
Cohesion: 0.12
Nodes (10): ResearchCategory, College, CollegeId, Description, FieldsDefinitionJson, Id, IsActive, Name (+2 more)

### Community 77 - "CollegeCoAdmin"
Cohesion: 0.12
Nodes (17): 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, 2. اختبارات المرحلة الثالثة, 2. اختبارات المرحلة السابعة, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات, المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين), المرحلة السابعة (Phase 7): الاعتماد النهائي وإصدار الشهادة والتحقق, الهيكل العام للمشروع (Solution Folder Structure) (+9 more)

### Community 78 - "IResearcherRepository"
Cohesion: 0.11
Nodes (3): 3.1 Detailed Audit: Rule 1 — Thin Controllers Principle, BUG-15: Missing `ModelState.IsValid` Validation in `SuperAdminController.SaveCategory`, IResearcherRepository

### Community 79 - "EnumExtensions"
Cohesion: 0.29
Nodes (4): ResearcherType, FacultyMember, PostgraduateStudent, Researcher

### Community 80 - "ProfileViewModel"
Cohesion: 0.15
Nodes (13): ProfileViewModel, AcademicDegree, CollegeName, Department, Email, FullName, HasSignature, NationalId (+5 more)

### Community 81 - "ResearchCreateDto"
Cohesion: 0.18
Nodes (11): ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId, CollegeId, DynamicAnswers, DynamicFields, InformedConsentFile (+3 more)

### Community 84 - "AttachmentType"
Cohesion: 0.25
Nodes (6): AttachmentType, AdditionalDocument, InformedConsent, ModifiedFile, ResearchProposal, ResearchTools

### Community 85 - "InvitationStatus"
Cohesion: 0.25
Nodes (5): InvitationStatus, Accepted, Expired, Pending, Revoked

### Community 86 - "UserRole"
Cohesion: 0.25
Nodes (6): UserRole, CollegeCoAdmin, Developer, Judge, Researcher, SuperAdmin

### Community 87 - "ResearchStatus"
Cohesion: 0.22
Nodes (6): ResearchStatus, Approved, AssignedToJudges, CertificateIssued, ModificationsRequired, UnderFormalScreening

### Community 88 - "ResearchModificationViewModel"
Cohesion: 0.25
Nodes (8): ResearchModificationViewModel, CurrentRound, ErrorMessage, ModificationNotes, Research, ResearcherNotes, ResearchId, Title

### Community 89 - "ResearchModificationDto"
Cohesion: 0.33
Nodes (6): BUG-32: Input DTOs Used Directly as Models in ResearcherController Razor Views, ResearchModificationDto, ModifiedProposalFile, ModifiedToolsFile, ResearcherNotes, ResearchId

### Community 90 - "3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs"
Cohesion: 0.22
Nodes (7): 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs, ForgotPasswordDto, Email, LoginDto, Email, Password, RememberMe

### Community 91 - "AcceptInvitationDto"
Cohesion: 0.25
Nodes (7): BUG-06: Deceptive Success and Password Desynchronization on Logged-Out Existing User Judge Invitation Acceptance, AcceptInvitationDto, AcademicTitle, ConfirmPassword, FullName, Password, Token

### Community 92 - "EthicsRating"
Cohesion: 0.29
Nodes (4): EthicsRating, Appropriate, Inappropriate, NeedsModification

### Community 93 - "EvaluationDecision"
Cohesion: 0.29
Nodes (6): EnumExtensions, EvaluationDecision, Approved, ClosedDueToOtherJudge, ModificationsRequired, UnderEvaluation

### Community 94 - "UserSummaryDto"
Cohesion: 0.20
Nodes (9): 1.1 Key Metrics & Remediation Status, 1.2 Breakdown by Severity, 1.3 Breakdown by Category, 1. Executive Summary & Statistics Tables, BUG-34: Empty Shell DTO Subclasses ViewModel Violating Layer Isolation, UserSummaryDto, Email, FullName (+1 more)

### Community 95 - "OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin"
Cohesion: 0.25
Nodes (3): VerifyEmailDto, EmailOrUserId, Token

### Community 97 - "VerifyEmailViewModel"
Cohesion: 0.25
Nodes (7): VerifyEmailViewModel, EmailOrUserId, ErrorMessage, HasPendingCode, SuccessMessage, Token, ValidityMinutes

### Community 99 - "NotificationAndSignalRTests.cs"
Cohesion: 0.18
Nodes (4): researchProject.Services.BackgroundServices, researchProject.Services.Interfaces, researchProject.Hubs, researchProject.Services.Implementations

### Community 114 - "EmailVerificationCode"
Cohesion: 0.18
Nodes (11): EmailVerificationCode, AttemptsCount, Code, CreatedAt, Email, ExpiresAt, Id, IsUsed (+3 more)

### Community 122 - "UserSummaryViewModel"
Cohesion: 0.29
Nodes (6): UserSummaryViewModel, Email, FullName, Id, RoleName, UserId

### Community 123 - "3. Architecture Rules Compliance Scorecard"
Cohesion: 0.33
Nodes (5): 3.2 Detailed Audit: Rule 2 — Native ASP.NET Core Identity & Claims, 3.3 Detailed Audit: Rule 3 — Single Type Per File Principle, 3. Architecture Rules Compliance Scorecard, Comprehensive Code Review & Verified Defect Catalog: `researchProject`, Scorecard Summary Table

### Community 124 - "AuditLog"
Cohesion: 0.12
Nodes (17): AuditLog, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 125 - "ResetPasswordDto"
Cohesion: 0.33
Nodes (5): ResetPasswordDto, ConfirmPassword, Email, NewPassword, Token

### Community 130 - "ChangePasswordDto"
Cohesion: 0.40
Nodes (4): ChangePasswordDto, ConfirmPassword, CurrentPassword, NewPassword

### Community 131 - "ResearchCreateViewModel"
Cohesion: 0.20
Nodes (9): ResearchCreateViewModel, AcademicSpecialty, AvailableCategories, CategoryId, CollegeId, CollegeName, DynamicAnswers, ErrorMessage (+1 more)

### Community 135 - "2. Master Catalog of Verified Bugs"
Cohesion: 0.25
Nodes (8): 2. Master Catalog of Verified Bugs, BUG-30: Authentication Cookie and Claims Not Refreshed upon Accepting Judge Role, BUG-37: Artificial Unit Test Scenario Concealing Round Incrementation Defect, BUG-39: Test Suite Omits Primary 6-Digit OTP Verification Flow, Category 6: Architecture Rule 2 — Native Identity & Claims, Category 8: Test Suite Gaps & Blind Spots, EXCLUDED-01 (Formerly BUG-14): Uncontrolled Cascading Polling Timers in Research Status Tracker Partial View, Excluded Findings & Resolved False Positives (Audit Trail)

### Community 143 - "JudgeToResearcherDto"
Cohesion: 0.29
Nodes (6): JudgeToResearcherDto, AcademicDegree, CollegeId, Department, NationalId, ResearcherType

### Community 154 - "_ResearchReviewPartial.cshtml"
Cohesion: 0.50
Nodes (3): researchProject.Models.Constants, researchProject.Models.Entities.Research, System.Text.Json

### Community 155 - "ResearchDetails.cshtml"
Cohesion: 0.50
Nodes (3): researchProject.Models.Constants, researchProject.Models.Entities.Research, System.Text.Json

### Community 156 - "_ResearchDetailsPartial.cshtml"
Cohesion: 0.50
Nodes (3): researchProject.Models.Constants, researchProject.Models.Entities.Research, System.Text.Json

## Knowledge Gaps
- **549 isolated node(s):** `Logs`, `CurrentPage`, `TotalPages`, `TotalCount`, `SearchTerm` (+544 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 748 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **96 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `researchProject.Models.Enums` connect `researchProject.Models.Enums` to `microsoft_entityframeworkcore`, `EnumExtensions`, `InvitationStatus`, `ResearchStatus`, `EthicsRating`?**
  _High betweenness centrality (0.039) - this node is a cross-community bridge._
- **What connects `Logs`, `CurrentPage`, `TotalPages` to the rest of the system?**
  _549 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `List` be split into smaller, more focused modules?**
  _Cohesion score 0.12413793103448276 - nodes in this community are weakly interconnected._
- **Why does `6. إدارة المستخدمين والأدوار (Identity & Roles)` connect `المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي` to `Researcher`, `Judge`, `CollegeCoAdmin`?**
  _High betweenness centrality (0.034) - this node is a cross-community bridge._
- **Should `microsoft_entityframeworkcore` be split into smaller, more focused modules?**
  _Cohesion score 0.132890365448505 - nodes in this community are weakly interconnected._
- **Should `Research` be split into smaller, more focused modules?**
  _Cohesion score 0.08695652173913043 - nodes in this community are weakly interconnected._
- **Should `IActionResult` be split into smaller, more focused modules?**
  _Cohesion score 0.10241820768136557 - nodes in this community are weakly interconnected._