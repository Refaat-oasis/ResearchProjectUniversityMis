# Graph Report - researchProject  (2026-11-18)

## Corpus Check
- 240 files · ~122,211 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 1792 nodes · 4508 edges · 176 communities (74 shown, 102 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 723 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a097c932`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- IActionResult
- HangfireAuditAndErrorHandlingTests.cs
- Research
- ResearchCreateDto
- Evaluation
- Fact
- AuditLogFilter.OnActionExecutionAsync
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
- IReportExportService
- ApplicationUser
- ResearchStatusHistory
- SeoMetadataViewModel
- AccountRepository
- AuditLogViewModel
- Task
- HttpGet
- _DynamicCategoryFieldsPartial.cshtml
- site.js
- _ManageCollegesPartial.cshtml
- _ManageCoAdminsPartial.cshtml
- SuperAdmin/_ManageCategoriesPartial.cshtml
- system
- researchProject.Models.ViewModels.CertificateVerificationResultViewModel
- researchProject.Models.ViewModels.LoginViewModel
- _CreateResearchPartial.cshtml
- _UploadModificationsPartial.cshtml
- _CollegeReportsPartial.cshtml
- CollegeCoAdmin/Index.cshtml
- Judge/Index.cshtml
- Researcher/Index.cshtml
- _MyCertificatesPartial.cshtml
- _CertificatesArchivePartial.cshtml
- researchProject.Services
- _ReportsPartial.cshtml
- List
- JudgeRepository
- CertificateService
- College
- AccountRepository
- ResearchAttachment
- IEmailSender
- system
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- DateTime
- SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType
- 4. Comprehensive Cross-Reference Index
- SuperAdminController
- AccountRepository.cs
- ApplicationUser
- Notification
- ResearchCategory
- MigrationBuilder
- ProfileViewModel
- 2. Master Catalog of Verified Bugs
- ResearcherSignUpDto
- UserProfileViewModel
- AccountController
- notifications.js
- UserSummaryDto
- AccountRepository
- ResearchCreateViewModel
- NotificationController
- EnumExtensions
- UserSummaryViewModel
- AuditLogFilter.cs
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
- OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin
- DateTime
- _ResearchStatusTrackerPartial.cshtml
- Comprehensive Code Review & Verified Defect Catalog: `researchProject`
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
- AuditLogs.cshtml
- _AuditLogsPartial.cshtml
- SuperAdmin/Categories.cshtml
- SuperAdmin/Certificates.cshtml
- List<researchProject.Models.Entities.CollegeCoAdmin>
- List<researchProject.Models.Entities.College>
- _ManageResearchersPartial.cshtml
- SuperAdmin/Reports.cshtml
- Researchers.cshtml
- NotificationService.cs
- AuditLog
- AddEmailVerificationCodeTable
- CollegeCoAdmin
- researchProject.Models.ViewModels.AuditLogListViewModel
- ResearcherRepository
- خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan)
- ResearcherResearchesBridge
- ResearchCreateDto
- EmailVerificationCode
- AuditLogListViewModel
- JudgeToResearcherDto
- HangfireDashboardAuthorizationFilter
- EntityAndDatabaseRulesTests.Evaluation_MultipleJudgesForSameRound_Succeeds
- init1
- AddAuditLogsAndRefactorAttachments
- _ResearchReviewPartial.cshtml
- ResearchDetails.cshtml
- _ResearchDetailsPartial.cshtml
- _RegisterAsResearcherModalPartial.cshtml
- _UserProfilePartial.cshtml
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
- _RightSidebarPartial.cshtml
- _SeoMetaTagsPartial.cshtml

## God Nodes (most connected - your core abstractions)
1. `AccountRepository` - 61 edges
2. `ApplicationDbContext` - 57 edges
3. `College` - 49 edges
4. `SuperAdminController` - 49 edges
5. `ResearchCategory` - 47 edges
6. `Judge` - 46 edges
7. `SuperAdminRepository` - 44 edges
8. `researchProject.Models.Entities` - 41 edges
9. `ApplicationUser` - 39 edges
10. `CollegeCoAdminRepository` - 38 edges

## Surprising Connections (you probably didn't know these)
- `2. اختبارات المرحلة الأولى` --references--> `Judge`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/Judge.cs
- `1. مهام التنفيذ` --references--> `ApplicationUser`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ApplicationUser.cs
- `2. اختبارات المرحلة السابعة` --references--> `ResearchStatusHistory`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ResearchStatusHistory.cs
- `BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges` --references--> `Evaluation`  [INFERRED]
  BUGS.md → researchProject/Models/Entities/Evaluation.cs
- `3.3 Detailed Audit: Rule 3 — Single Type Per File Principle` --references--> `UserSummaryViewModel`  [INFERRED]
  BUGS.md → researchProject/Models/ViewModels/UserSummaryViewModel.cs

## Import Cycles
- None detected.

## Communities (176 total, 102 thin omitted)

### Community 1 - "HangfireAuditAndErrorHandlingTests.cs"
Cohesion: 0.18
Nodes (5): researchProject.Services, researchProject.Tests, researchProject.Services.Implementations, researchProject.Repositories.Implementations, Microsoft.AspNetCore.Identity

### Community 2 - "Research"
Cohesion: 0.10
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 3 - "ResearchCreateDto"
Cohesion: 0.12
Nodes (15): SystemStatisticsViewModel, ApprovedCount, AssignedToJudgesCount, AverageJudgingDays, AverageScreeningHours, CertificatesIssuedCount, ModificationsRequiredCount, ResearchesByCategory (+7 more)

### Community 4 - "Evaluation"
Cohesion: 0.11
Nodes (19): 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, المرحلة الخامسة (Phase 5): موديول مدير اللجنة الفرعية (الفحص الشكلي، التوزيع، الإسناد), Evaluation, Decision, Id, InformedConsentRating, IsDigitallySigned (+11 more)

### Community 7 - "CertificateVerificationResultViewModel"
Cohesion: 0.17
Nodes (12): 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, PublicController, CertificateVerificationResultViewModel, CollegeName, IssueDate, IsValid, Message (+4 more)

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
Cohesion: 0.10
Nodes (17): 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 7. Evaluation (Review) & Round Closing, 1. مهام التنفيذ, 2. اختبارات المرحلة السادسة, المرحلة السادسة (Phase 6): موديول المحكم العلمي (التقييم، التعديلات، الاعتماد), Judge, AcademicTitle, College (+9 more)

### Community 15 - "ApplicationDbContext"
Cohesion: 0.08
Nodes (21): ApplicationDbContext, AuditLogs, CollegeCoAdmins, Colleges, Departments, EmailVerificationCodes, Evaluations, JudgeInvitations (+13 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.15
Nodes (13): JudgeInvitation, AcceptedAt, College, CollegeId, CreatedAt, Email, ExpiresAt, Id (+5 more)

### Community 17 - "IReportExportService"
Cohesion: 0.47
Nodes (3): BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions, IReportExportService, ReportExportService

### Community 18 - "ApplicationUser"
Cohesion: 0.17
Nodes (7): EmailJobType, CoAdminCredentials, JudgeInvitation, HangfireEmailDispatcher, IHangfireEmailDispatcher, INotificationService, HangfireAuditAndErrorHandlingTests

### Community 19 - "ResearchStatusHistory"
Cohesion: 0.11
Nodes (19): 0. Identity Model Summary (READ FIRST), 10. Security & Data Isolation, 11. Seeding & Configuration, 5. UI/UX, RTL, Right Sidebar & Partial Views, 6. Research Status Workflow (Strict), 9. Reports & Statistics, AI Agent Instructions & Project Rules (`AGENTS.md`), 1. مهام التنفيذ (+11 more)

### Community 20 - "SeoMetadataViewModel"
Cohesion: 0.09
Nodes (15): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId, SeoMetadataViewModel, Author, CanonicalUrl (+7 more)

### Community 21 - "AccountRepository"
Cohesion: 0.12
Nodes (3): AccountRepository, UnconfirmedEmailCleanupService, IUnconfirmedEmailCleanupService

### Community 22 - "AuditLogViewModel"
Cohesion: 0.12
Nodes (17): AuditLogViewModel, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 23 - "Task"
Cohesion: 0.07
Nodes (3): BUG-15: Missing `ModelState.IsValid` Validation in `SuperAdminController.SaveCategory`, SuperAdminRepository, ISuperAdminRepository

### Community 24 - "HttpGet"
Cohesion: 0.09
Nodes (4): 3.1 Detailed Audit: Rule 1 — Thin Controllers Principle, BUG-11: SuperAdmin Proposal Creation Fails with Null Profile and Hardcoded College Fallback, ResearcherController, IResearcherRepository

### Community 26 - "site.js"
Cohesion: 0.20
Nodes (4): initCustomDatalists(), initDedicatedDropdowns(), initUniversalTablePagination(), resolveInitialRoute()

### Community 30 - "system"
Cohesion: 0.08
Nodes (5): researchProject.Models.ViewModels, researchProject.Models.DTOs, researchProject.Models.Entities, ResendVerificationCodeDto, EmailOrUserId

### Community 41 - "researchProject.Services"
Cohesion: 0.10
Nodes (11): researchProject.Repositories.Interfaces, researchProject.Controllers, researchProject.Services.Interfaces, researchProject.Models.Constants, AttachmentTypeConstants, EthicsRatingConstants, EvaluationDecisionConstants, InvitationStatusConstants (+3 more)

### Community 52 - "JudgeRepository"
Cohesion: 0.09
Nodes (22): BUG-02: Missing Validation on Inactive/Invalid Judges Traps Research in `AssignedToJudges` with 0 Evaluations (Deadlock), BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges, BUG-04: Round Number Corruption and Previous Judge Lockout on Research Re-assignment, BUG-05: Assigning Judges During `ModificationsRequired` State Locks Researcher Out, BUG-07: Premature Verification OTP Invalidation (Burned) Before Password Complexity Check, BUG-08: Digital Certificate Issuance Bypasses Mandatory Co-Admin Signature Image Requirement (Specification Conflict), BUG-09: Unhandled `FormatException` in `ResendVerificationCode` When Passing `UserId` Instead of Email, BUG-10: Arbitrary Evaluation Decision Value Bypass Defaulting to Modifications Required (+14 more)

### Community 53 - "CertificateService"
Cohesion: 0.22
Nodes (7): BUG-21: Concurrent OTP Brute-Force Race Condition Bypassing 5-Attempt Lockout Limit, BUG-22: Stored Cross-Site Scripting (XSS) and Raw HTML Injection in Generated Certificates, BUG-23: Arbitrary File Disclosure via Path Traversal in Certificate Signatures and File Streaming, BUG-24: Insecure Storage of Authentication Verification OTP Codes in Plaintext in Database, BUG-25: Arithmetic Overflow `Math.Abs(int.MinValue)` in Certificate Serial Number Generation, Category 3: Security & Input Validation, CertificateService

### Community 54 - "College"
Cohesion: 0.19
Nodes (11): College, Code, CollegeCoAdmins, Departments, Id, Name, ResearchCategories, Researchers (+3 more)

### Community 56 - "ResearchAttachment"
Cohesion: 0.17
Nodes (12): ResearchAttachment, AttachmentType, ContentType, FilePath, FileSize, Id, OriginalFileName, Research (+4 more)

### Community 57 - "IEmailSender"
Cohesion: 0.12
Nodes (4): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), JudgeController, IJudgeRepository, IEmailSender

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

### Community 61 - "SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType"
Cohesion: 0.13
Nodes (10): ChangePasswordDto, ConfirmPassword, CurrentPassword, NewPassword, UpdateProfileDto, AcademicDegree, FullName, PhoneNumber (+2 more)

### Community 62 - "4. Comprehensive Cross-Reference Index"
Cohesion: 0.06
Nodes (26): 4. Comprehensive Cross-Reference Index, BUG-01: Missing Attachment Download Endpoint and UI Action Links (Functional Blocker), BUG-33: EF Core Domain Entities Bound Directly as Controller Action Input Parameters (Mass Assignment), BUG-36: Complete Absence of Test Coverage (0%) for Core Security, Auth, Storage, and Controllers, BUG-37: Artificial Unit Test Scenario Concealing Round Incrementation Defect, BUG-39: Test Suite Omits Primary 6-Digit OTP Verification Flow, Category 8: Test Suite Gaps & Blind Spots, Excluded / Resolved Items Reference Table (+18 more)

### Community 64 - "AccountRepository.cs"
Cohesion: 0.09
Nodes (4): researchProject.Services.BackgroundServices, researchProject, researchProject.Models.Entities, Program

### Community 65 - "ApplicationUser"
Cohesion: 0.17
Nodes (8): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), ApplicationUser, CreatedAt, FullName, IsActive, AttachmentService, AttachmentAndSecurityFixTests, ApplicationUser

### Community 66 - "Notification"
Cohesion: 0.07
Nodes (27): BUG-26: Redundant Group Subscription in `NotificationHub` Desynchronized from `Clients.User` Dispatch, BUG-38: Overly Broad SignalR Test Mocks Resulting in False-Positive Verification, Category 4: SignalR & Real-Time Communication, NotificationHub, Notification, CreatedAt, Icon, Id (+19 more)

### Community 67 - "ResearchCategory"
Cohesion: 0.18
Nodes (9): ResearchCategory, College, CollegeId, Description, FieldsDefinitionJson, Id, IsActive, Name (+1 more)

### Community 68 - "MigrationBuilder"
Cohesion: 0.13
Nodes (6): InitialCreate, AddDepartments, AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers, AddFileDataAndCollegeToJudges, AddResearcherTypeToJudges

### Community 69 - "ProfileViewModel"
Cohesion: 0.15
Nodes (13): ProfileViewModel, AcademicDegree, CollegeName, Department, Email, FullName, HasSignature, NationalId (+5 more)

### Community 70 - "2. Master Catalog of Verified Bugs"
Cohesion: 0.25
Nodes (8): 2. Master Catalog of Verified Bugs, BUG-28: Direct Form Data Key Iteration and Dynamic Field Parsing in Controller Action, BUG-29: Controllers Executing Direct Domain Profile Resolution via Repositories, BUG-30: Authentication Cookie and Claims Not Refreshed upon Accepting Judge Role, Category 5: Architecture Rule 1 — Thin Controllers Principle, Category 6: Architecture Rule 2 — Native Identity & Claims, EXCLUDED-01 (Formerly BUG-14): Uncontrolled Cascading Polling Timers in Research Status Tracker Partial View, Excluded Findings & Resolved False Positives (Audit Trail)

### Community 71 - "ResearcherSignUpDto"
Cohesion: 0.18
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 72 - "UserProfileViewModel"
Cohesion: 0.10
Nodes (20): UserProfileViewModel, AvailableColleges, CanBecomeResearcher, Email, FullName, IsCollegeCoAdmin, IsJudge, IsResearcher (+12 more)

### Community 74 - "notifications.js"
Cohesion: 0.36
Nodes (10): buildNotificationItemHtml(), escapeHtml(), handleIncomingNotification(), initSignalR(), loadRecentNotifications(), loadUnreadCount(), markAndNavigate(), playSoftBeep() (+2 more)

### Community 75 - "UserSummaryDto"
Cohesion: 0.33
Nodes (5): BUG-34: Empty Shell DTO Subclasses ViewModel Violating Layer Isolation, UserSummaryDto, Email, FullName, RoleName

### Community 77 - "ResearchCreateViewModel"
Cohesion: 0.20
Nodes (9): ResearchCreateViewModel, AcademicSpecialty, AvailableCategories, CategoryId, CollegeId, CollegeName, DynamicAnswers, ErrorMessage (+1 more)

### Community 79 - "EnumExtensions"
Cohesion: 0.25
Nodes (5): EnumExtensions, ResearcherType, FacultyMember, PostgraduateStudent, Researcher

### Community 80 - "UserSummaryViewModel"
Cohesion: 0.29
Nodes (6): UserSummaryViewModel, Email, FullName, Id, RoleName, UserId

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
Nodes (6): UserRole, CollegeCoAdmin, Developer, Judge, Researcher, SuperAdmin

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
Cohesion: 0.25
Nodes (5): EvaluationDecision, Approved, ClosedDueToOtherJudge, ModificationsRequired, UnderEvaluation

### Community 94 - "3. Architecture Rules Compliance Scorecard"
Cohesion: 0.50
Nodes (4): 3.2 Detailed Audit: Rule 2 — Native ASP.NET Core Identity & Claims, 3.3 Detailed Audit: Rule 3 — Single Type Per File Principle, 3. Architecture Rules Compliance Scorecard, Scorecard Summary Table

### Community 95 - "ResetPasswordDto"
Cohesion: 0.40
Nodes (5): ResetPasswordDto, ConfirmPassword, Email, NewPassword, Token

### Community 96 - "OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin"
Cohesion: 0.29
Nodes (3): VerifyEmailDto, EmailOrUserId, Token

### Community 97 - "DateTime"
Cohesion: 0.17
Nodes (4): UpdateEmailVerificationCodeLength, AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers, ApplicationDbContextModelSnapshot

### Community 99 - "Comprehensive Code Review & Verified Defect Catalog: `researchProject`"
Cohesion: 0.33
Nodes (5): 1.1 Key Metrics & Remediation Status, 1.2 Breakdown by Severity, 1.3 Breakdown by Category, 1. Executive Summary & Statistics Tables, Comprehensive Code Review & Verified Defect Catalog: `researchProject`

### Community 124 - "AuditLog"
Cohesion: 0.12
Nodes (17): AuditLog, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 128 - "CollegeCoAdmin"
Cohesion: 0.14
Nodes (14): BUG-32: Input DTOs Used Directly as Models in ResearcherController Razor Views, BUG-35: EF Core Entities Used Directly as Presentation Models across Views, Category 7: Architecture Rule 4 — Separation Between ViewModels and DTOs, 1. مهام التنفيذ, 2. اختبارات المرحلة الثالثة, المرحلة الثالثة (Phase 3): موديول مدير النظام العام (الكليات، التصنيفات وحقولها، الـ Co-Admins، دعوات المحكمين), CollegeCoAdmin, College (+6 more)

### Community 131 - "ResearcherRepository"
Cohesion: 0.09
Nodes (11): BUG-16: Invalid `RollbackAsync` Invoked on Already Committed Database Transaction on Notification Failure, BUG-19: Missing College Isolation Validation Enables Cross-College Proposal Injection, BUG-20: N+1 Database Query Cascade in SuperAdmin Role Retrieval for Judge Invitations, Category 2: Concurrency, Transactions & Data Integrity, ResearchModificationDto, ModifiedProposalFile, ModifiedToolsFile, ResearcherNotes (+3 more)

### Community 132 - "خطة التنفيذ المرحلية للمنصة (Multi-Phase Implementation & Testing Plan)"
Cohesion: 0.15
Nodes (12): 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, 2. اختبارات المرحلة الثامنة, 2. اختبارات المرحلة الرابعة, 2. اختبارات المرحلة السابعة, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر (+4 more)

### Community 133 - "ResearcherResearchesBridge"
Cohesion: 0.20
Nodes (9): 1. Entity & Database Rules (ERD Compliance), ResearchStatusConstants, ResearcherResearchesBridge, Id, IsMainAuthor, Research, Researcher, ResearcherId (+1 more)

### Community 135 - "ResearchCreateDto"
Cohesion: 0.18
Nodes (11): ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId, CollegeId, DynamicAnswers, DynamicFields, InformedConsentFile (+3 more)

### Community 136 - "EmailVerificationCode"
Cohesion: 0.18
Nodes (11): EmailVerificationCode, AttemptsCount, Code, CreatedAt, Email, ExpiresAt, Id, IsUsed (+3 more)

### Community 141 - "AuditLogListViewModel"
Cohesion: 0.20
Nodes (10): AuditLogListViewModel, CurrentPage, FromDate, Logs, RoleFilter, SearchTerm, SuccessFilter, ToDate (+2 more)

### Community 143 - "JudgeToResearcherDto"
Cohesion: 0.33
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
- **544 isolated node(s):** `SuperAdmin`, `CollegeCoAdmin`, `Judge`, `Researcher`, `Developer` (+539 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 743 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **102 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `researchProject.Models.Constants` connect `researchProject.Services` to `AccountRepository.cs`, `HangfireAuditAndErrorHandlingTests.cs`, `ResearcherResearchesBridge`, `AuditLogFilter.cs`, `researchProject.Models.Enums`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **What connects `SuperAdmin`, `CollegeCoAdmin`, `Judge` to the rest of the system?**
  _544 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `IActionResult` be split into smaller, more focused modules?**
  _Cohesion score 0.12753623188405797 - nodes in this community are weakly interconnected._
- **Why does `researchProject.Models.Enums` connect `researchProject.Models.Enums` to `HangfireAuditAndErrorHandlingTests.cs`, `researchProject.Services`, `EnumExtensions`, `InvitationStatus`, `ResearchStatus`, `EthicsRating`, `EvaluationDecision`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **Should `Research` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._
- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `CollegeCoAdmin`, `ResearcherRepository`, `ResearcherResearchesBridge`, `AuditLogFilter.OnActionExecutionAsync`, `CertificateVerificationResultViewModel`, `EmailVerificationCode`, `Researcher`, `Judge`, `JudgeInvitation`, `ApplicationUser`, `EntityAndDatabaseRulesTests.Evaluation_MultipleJudgesForSameRound_Succeeds`, `Task`, `HttpGet`, `List`, `College`, `AccountRepository`, `ResearchAttachment`, `IEmailSender`, `system`, `SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType`, `ApplicationUser`, `Notification`, `ResearchCategory`, `AccountRepository`, `3. Architecture Rules Compliance Scorecard`, `AuditLog`?**
  _High betweenness centrality (0.032) - this node is a cross-community bridge._
- **Should `ResearchCreateDto` be split into smaller, more focused modules?**
  _Cohesion score 0.125 - nodes in this community are weakly interconnected._