# Graph Report - researchProject  (2026-11-18)

## Corpus Check
- 240 files · ~124,216 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 1802 nodes · 4522 edges · 160 communities (67 shown, 93 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 727 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `51135afc`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- IActionResult
- HangfireAuditAndErrorHandlingTests.cs
- Research
- IActionResult
- Evaluation
- Fact
- AuditLogFilter.OnActionExecutionAsync
- CertificateVerificationResultViewModel
- ResearcherSignUpViewModel
- researchProject.Tests
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- Researcher
- http
- ResearchModificationDto
- SignUp.cshtml
- ApplicationDbContext
- JudgeInvitation
- HangfireEmailDispatcher.SendEmailWithRollbackAsync
- ResearchCreateDto
- Judge
- SeoMetadataViewModel
- AccountRepository
- AuditLogViewModel
- SuperAdminRepository
- HttpGet
- _DynamicCategoryFieldsPartial.cshtml
- site.js
- _ManageCollegesPartial.cshtml
- _ManageCoAdminsPartial.cshtml
- SuperAdmin/_ManageCategoriesPartial.cshtml
- system
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
- researchProject.Services
- _ReportsPartial.cshtml
- AcceptInvitationDto
- CertificateService
- CollegeCoAdminRepository
- AccountRepository
- ResearchAttachment
- JudgeRepository
- system
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)
- SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType
- 4. Comprehensive Cross-Reference Index
- NotificationHub
- Program.cs
- ApplicationUser
- NotificationService
- Notification
- DateTime
- HangfireAuditAndErrorHandlingTests.HangfireEmailDispatcher_SecondFailure_RollsBackCoAdminAccount
- NotificationViewModel
- ResearcherSignUpDto
- UserProfileViewModel
- AccountController
- notifications.js
- DbSeeder.cs
- College
- NotificationAndSignalRTests.cs
- EnumExtensions
- researchProject.Models.Enums
- ErrorController
- AttachmentType
- InvitationStatus
- UserRole
- ResearchStatus
- ResearchModificationViewModel
- VerifyEmailViewModel
- 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs
- OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin
- EthicsRating
- EvaluationDecision
- UserSummaryViewModel
- Fact
- UserSummaryDto
- _ResearchStatusTrackerPartial.cshtml
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
- AuditLog
- ResetPasswordDto
- Program.Main
- researchProject.Models.ViewModels.AuditLogListViewModel
- 3. Architecture Rules Compliance Scorecard
- JudgeToResearcherDto
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
3. `SuperAdminController` - 49 edges
4. `College` - 49 edges
5. `ResearchCategory` - 47 edges
6. `Judge` - 46 edges
7. `SuperAdminRepository` - 44 edges
8. `researchProject.Models.Entities` - 41 edges
9. `ApplicationUser` - 39 edges
10. `CollegeCoAdminRepository` - 38 edges

## Surprising Connections (you probably didn't know these)
- `BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges` --references--> `Evaluation`  [INFERRED]
  BUGS.md → researchProject/Models/Entities/Evaluation.cs
- `3.3 Detailed Audit: Rule 3 — Single Type Per File Principle` --references--> `UserSummaryViewModel`  [INFERRED]
  BUGS.md → researchProject/Models/ViewModels/UserSummaryViewModel.cs
- `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)` --references--> `SuperAdminController`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Controllers/SuperAdminController.cs
- `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs` --references--> `SuperAdminController`  [INFERRED]
  BUGS.md → researchProject/Controllers/SuperAdminController.cs
- `1. مهام التنفيذ` --references--> `SuperAdminController`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Controllers/SuperAdminController.cs

## Import Cycles
- None detected.

## Communities (160 total, 93 thin omitted)

### Community 1 - "HangfireAuditAndErrorHandlingTests.cs"
Cohesion: 0.15
Nodes (5): researchProject.Services, researchProject.Tests, researchProject.Repositories.Implementations, Microsoft.AspNetCore.Identity, researchProject.Models.Constants

### Community 2 - "Research"
Cohesion: 0.10
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 3 - "IActionResult"
Cohesion: 0.11
Nodes (3): BUG-15: Missing `ModelState.IsValid` Validation in `SuperAdminController.SaveCategory`, NotificationController, SuperAdminController

### Community 4 - "Evaluation"
Cohesion: 0.05
Nodes (41): 10. Security & Data Isolation, 6. Research Status Workflow (Strict), 1. مهام التنفيذ, 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الخامسة, 2. اختبارات المرحلة الرابعة, 2. اختبارات المرحلة السابعة (+33 more)

### Community 5 - "Fact"
Cohesion: 0.14
Nodes (10): ResearchCategory, College, CollegeId, Description, FieldsDefinitionJson, Id, IsActive, Name (+2 more)

### Community 7 - "CertificateVerificationResultViewModel"
Cohesion: 0.12
Nodes (13): 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, PublicController, CertificateVerificationResultViewModel, CollegeName, IssueDate, IsValid, Message (+5 more)

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
Cohesion: 0.14
Nodes (15): 1. مهام التنفيذ, 2. اختبارات المرحلة الثانية, المرحلة الثانية (Phase 2): الدخول الموحد والتسجيل بالبريد المؤسسي والواجهة العربية, Researcher, AcademicDegree, College, CollegeId, Department (+7 more)

### Community 12 - "http"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 13 - "ResearchModificationDto"
Cohesion: 0.40
Nodes (5): ResearchModificationDto, ModifiedProposalFile, ModifiedToolsFile, ResearcherNotes, ResearchId

### Community 14 - "SignUp.cshtml"
Cohesion: 0.40
Nodes (4): researchProject.Models.Enums, researchProject.Models.ViewModels.ResearcherSignUpViewModel, College, researchProject.Models.Entities

### Community 15 - "ApplicationDbContext"
Cohesion: 0.07
Nodes (27): ApplicationDbContext, AuditLogs, CollegeCoAdmins, Colleges, Departments, EmailVerificationCodes, Evaluations, JudgeInvitations (+19 more)

### Community 16 - "JudgeInvitation"
Cohesion: 0.15
Nodes (13): JudgeInvitation, AcceptedAt, College, CollegeId, CreatedAt, Email, ExpiresAt, Id (+5 more)

### Community 17 - "HangfireEmailDispatcher.SendEmailWithRollbackAsync"
Cohesion: 0.18
Nodes (6): EmailJobType, CoAdminCredentials, JudgeInvitation, HangfireEmailDispatcher, IHangfireEmailDispatcher, INotificationService

### Community 18 - "ResearchCreateDto"
Cohesion: 0.05
Nodes (41): BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions, BUG-28: Direct Form Data Key Iteration and Dynamic Field Parsing in Controller Action, BUG-29: Controllers Executing Direct Domain Profile Resolution via Repositories, Category 5: Architecture Rule 1 — Thin Controllers Principle, ResearchCreateDto, AcademicSpecialty, AdditionalDocumentFile, CategoryId (+33 more)

### Community 19 - "Judge"
Cohesion: 0.06
Nodes (41): 0. Identity Model Summary (READ FIRST), 11. Seeding & Configuration, 1. Entity & Database Rules (ERD Compliance), 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 5. UI/UX, RTL, Right Sidebar & Partial Views, 9. Reports & Statistics, AI Agent Instructions & Project Rules (`AGENTS.md`), 1. مهام التنفيذ (+33 more)

### Community 20 - "SeoMetadataViewModel"
Cohesion: 0.09
Nodes (15): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId, SeoMetadataViewModel, Author, CanonicalUrl (+7 more)

### Community 21 - "AccountRepository"
Cohesion: 0.09
Nodes (3): AccountRepository, UnconfirmedEmailCleanupService, IUnconfirmedEmailCleanupService

### Community 22 - "AuditLogViewModel"
Cohesion: 0.06
Nodes (28): DeveloperController, AuditLogListViewModel, CurrentPage, FromDate, Logs, RoleFilter, SearchTerm, SuccessFilter (+20 more)

### Community 23 - "SuperAdminRepository"
Cohesion: 0.07
Nodes (3): SuperAdminRepository, ISuperAdminRepository, IEmailSender

### Community 24 - "HttpGet"
Cohesion: 0.11
Nodes (3): BUG-11: SuperAdmin Proposal Creation Fails with Null Profile and Hardcoded College Fallback, ResearcherController, IResearcherRepository

### Community 26 - "site.js"
Cohesion: 0.18
Nodes (6): initCustomDatalists(), initDedicatedDropdowns(), filterOptions(), getItems(), initUniversalTablePagination(), resolveInitialRoute()

### Community 30 - "system"
Cohesion: 0.07
Nodes (5): researchProject.Models.ViewModels, researchProject.Models.DTOs, researchProject.Models.Entities, ResendVerificationCodeDto, EmailOrUserId

### Community 32 - "Login.cshtml"
Cohesion: 0.50
Nodes (3): researchProject.Models.ViewModels.LoginViewModel, College, researchProject.Models.Entities

### Community 41 - "researchProject.Services"
Cohesion: 0.10
Nodes (11): researchProject.Repositories.Interfaces, researchProject.Controllers, researchProject.Services.Interfaces, researchProject.Models.Constants, AttachmentTypeConstants, EthicsRatingConstants, EvaluationDecisionConstants, InvitationStatusConstants (+3 more)

### Community 52 - "AcceptInvitationDto"
Cohesion: 0.29
Nodes (7): BUG-06: Deceptive Success and Password Desynchronization on Logged-Out Existing User Judge Invitation Acceptance, AcceptInvitationDto, AcademicTitle, ConfirmPassword, FullName, Password, Token

### Community 53 - "CertificateService"
Cohesion: 0.12
Nodes (15): 2. Master Catalog of Verified Bugs, BUG-21: Concurrent OTP Brute-Force Race Condition Bypassing 5-Attempt Lockout Limit, BUG-22: Stored Cross-Site Scripting (XSS) and Raw HTML Injection in Generated Certificates, BUG-23: Arbitrary File Disclosure via Path Traversal in Certificate Signatures and File Streaming, BUG-24: Insecure Storage of Authentication Verification OTP Codes in Plaintext in Database, BUG-25: Arithmetic Overflow `Math.Abs(int.MinValue)` in Certificate Serial Number Generation, BUG-30: Authentication Cookie and Claims Not Refreshed upon Accepting Judge Role, BUG-32: Input DTOs Used Directly as Models in ResearcherController Razor Views (+7 more)

### Community 54 - "CollegeCoAdminRepository"
Cohesion: 0.14
Nodes (13): BUG-02: Missing Validation on Inactive/Invalid Judges Traps Research in `AssignedToJudges` with 0 Evaluations (Deadlock), BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges, BUG-04: Round Number Corruption and Previous Judge Lockout on Research Re-assignment, BUG-05: Assigning Judges During `ModificationsRequired` State Locks Researcher Out, BUG-07: Premature Verification OTP Invalidation (Burned) Before Password Complexity Check, BUG-08: Digital Certificate Issuance Bypasses Mandatory Co-Admin Signature Image Requirement (Specification Conflict), BUG-09: Unhandled `FormatException` in `ResendVerificationCode` When Passing `UserId` Instead of Email, BUG-12: Certificate Archive Date Range Filter Truncates Matches on Target End Date (+5 more)

### Community 56 - "ResearchAttachment"
Cohesion: 0.09
Nodes (18): BUG-16: Invalid `RollbackAsync` Invoked on Already Committed Database Transaction on Notification Failure, BUG-18: Race Condition and Lost Updates in Concurrent Multi-Judge Evaluation Submissions, BUG-19: Missing College Isolation Validation Enables Cross-College Proposal Injection, BUG-20: N+1 Database Query Cascade in SuperAdmin Role Retrieval for Judge Invitations, Category 2: Concurrency, Transactions & Data Integrity, ResearchAttachment, AttachmentType, ContentType (+10 more)

### Community 57 - "JudgeRepository"
Cohesion: 0.16
Nodes (12): 7. Evaluation (Review) & Round Closing, BUG-10: Arbitrary Evaluation Decision Value Bypass Defaulting to Modifications Required, BUG-17: Unhandled SignalR Exception in Judge Evaluation Transaction Rolls Back Official Evaluation, EvaluationSubmitDto, Decision, EvaluationId, InformedConsentRating, ModificationNotes (+4 more)

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

### Community 60 - "4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)"
Cohesion: 0.23
Nodes (4): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), 3.1 Detailed Audit: Rule 1 — Thin Controllers Principle, JudgeController, IJudgeRepository

### Community 61 - "SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType"
Cohesion: 0.15
Nodes (10): ChangePasswordDto, ConfirmPassword, CurrentPassword, NewPassword, UpdateProfileDto, AcademicDegree, FullName, PhoneNumber (+2 more)

### Community 62 - "4. Comprehensive Cross-Reference Index"
Cohesion: 0.07
Nodes (23): 4. Comprehensive Cross-Reference Index, BUG-01: Missing Attachment Download Endpoint and UI Action Links (Functional Blocker), BUG-33: EF Core Domain Entities Bound Directly as Controller Action Input Parameters (Mass Assignment), BUG-36: Complete Absence of Test Coverage (0%) for Core Security, Auth, Storage, and Controllers, Excluded / Resolved Items Reference Table, AttachmentController, CategorySaveDto, CollegeId (+15 more)

### Community 63 - "NotificationHub"
Cohesion: 0.18
Nodes (7): BUG-26: Redundant Group Subscription in `NotificationHub` Desynchronized from `Clients.User` Dispatch, BUG-37: Artificial Unit Test Scenario Concealing Round Incrementation Defect, BUG-38: Overly Broad SignalR Test Mocks Resulting in False-Positive Verification, BUG-39: Test Suite Omits Primary 6-Digit OTP Verification Flow, Category 4: SignalR & Real-Time Communication, Category 8: Test Suite Gaps & Blind Spots, NotificationHub

### Community 64 - "Program.cs"
Cohesion: 0.12
Nodes (4): researchProject, researchProject.Models.Entities, Program, ApplicationUser

### Community 65 - "ApplicationUser"
Cohesion: 0.18
Nodes (10): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), 1. مهام التنفيذ, 2. اختبارات المرحلة الثامنة, المرحلة الثامنة (Phase 8): التقارير والأرشيف والتحسينات والنشر, ApplicationUser, CreatedAt, FullName, IsActive (+2 more)

### Community 67 - "Notification"
Cohesion: 0.18
Nodes (11): Notification, CreatedAt, Icon, Id, IsRead, Message, TargetUrl, Title (+3 more)

### Community 68 - "DateTime"
Cohesion: 0.08
Nodes (13): InitialCreate, init1, AddDepartments, AddEmailVerificationCodeTable, UpdateEmailVerificationCodeLength, AddNationalIdUniqueAndUserCreatedAt, AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers (+5 more)

### Community 70 - "NotificationViewModel"
Cohesion: 0.20
Nodes (10): NotificationViewModel, CreatedAt, Icon, Id, IsRead, Message, TargetUrl, TimeAgo (+2 more)

### Community 71 - "ResearcherSignUpDto"
Cohesion: 0.18
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 72 - "UserProfileViewModel"
Cohesion: 0.06
Nodes (33): ProfileViewModel, AcademicDegree, CollegeName, Department, Email, FullName, HasSignature, NationalId (+25 more)

### Community 74 - "notifications.js"
Cohesion: 0.36
Nodes (10): buildNotificationItemHtml(), escapeHtml(), handleIncomingNotification(), initSignalR(), loadRecentNotifications(), loadUnreadCount(), markAndNavigate(), playSoftBeep() (+2 more)

### Community 76 - "College"
Cohesion: 0.12
Nodes (14): College, Code, CollegeCoAdmins, Departments, Id, Name, ResearchCategories, Researchers (+6 more)

### Community 79 - "EnumExtensions"
Cohesion: 0.29
Nodes (4): ResearcherType, FacultyMember, PostgraduateStudent, Researcher

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

### Community 89 - "VerifyEmailViewModel"
Cohesion: 0.25
Nodes (7): VerifyEmailViewModel, EmailOrUserId, ErrorMessage, HasPendingCode, SuccessMessage, Token, ValidityMinutes

### Community 90 - "3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs"
Cohesion: 0.29
Nodes (7): 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs, ForgotPasswordDto, Email, LoginDto, Email, Password, RememberMe

### Community 91 - "OtpVerificationAndDatalistTests.AccountController_VerifyEmailToken_PostWithValidOtp_RedirectsToLogin"
Cohesion: 0.29
Nodes (3): VerifyEmailDto, EmailOrUserId, Token

### Community 92 - "EthicsRating"
Cohesion: 0.29
Nodes (4): EthicsRating, Appropriate, Inappropriate, NeedsModification

### Community 93 - "EvaluationDecision"
Cohesion: 0.29
Nodes (6): EnumExtensions, EvaluationDecision, Approved, ClosedDueToOtherJudge, ModificationsRequired, UnderEvaluation

### Community 94 - "UserSummaryViewModel"
Cohesion: 0.29
Nodes (6): UserSummaryViewModel, Email, FullName, Id, RoleName, UserId

### Community 96 - "Fact"
Cohesion: 0.19
Nodes (3): EightFeaturesAndEnhancementsTests, EntityAndDatabaseRulesTests, NewRequirementsAndEnhancementsTests

### Community 97 - "UserSummaryDto"
Cohesion: 0.20
Nodes (9): 1.1 Key Metrics & Remediation Status, 1.2 Breakdown by Severity, 1.3 Breakdown by Category, 1. Executive Summary & Statistics Tables, BUG-34: Empty Shell DTO Subclasses ViewModel Violating Layer Isolation, UserSummaryDto, Email, FullName (+1 more)

### Community 124 - "AuditLog"
Cohesion: 0.12
Nodes (17): AuditLog, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 125 - "ResetPasswordDto"
Cohesion: 0.40
Nodes (5): ResetPasswordDto, ConfirmPassword, Email, NewPassword, Token

### Community 133 - "3. Architecture Rules Compliance Scorecard"
Cohesion: 0.33
Nodes (5): 3.2 Detailed Audit: Rule 2 — Native ASP.NET Core Identity & Claims, 3.3 Detailed Audit: Rule 3 — Single Type Per File Principle, 3. Architecture Rules Compliance Scorecard, Comprehensive Code Review & Verified Defect Catalog: `researchProject`, Scorecard Summary Table

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
- **549 isolated node(s):** `researchProject.Models.ViewModels.LoginViewModel`, `researchProject.Models.Entities`, `College`, `researchProject.Models.ViewModels.ResearcherSignUpViewModel`, `researchProject.Models.Entities` (+544 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 749 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **93 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ApplicationDbContext` connect `ApplicationDbContext` to `3. Architecture Rules Compliance Scorecard`, `Fact`, `AuditLogFilter.OnActionExecutionAsync`, `CertificateVerificationResultViewModel`, `Researcher`, `JudgeInvitation`, `HangfireEmailDispatcher.SendEmailWithRollbackAsync`, `Judge`, `SuperAdminRepository`, `List`, `CollegeCoAdminRepository`, `AccountRepository`, `ResearchAttachment`, `JudgeRepository`, `system`, `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)`, `SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType`, `ApplicationUser`, `NotificationService`, `Notification`, `HangfireAuditAndErrorHandlingTests.HangfireEmailDispatcher_SecondFailure_RollsBackCoAdminAccount`, `College`, `Fact`, `AuditLog`?**
  _High betweenness centrality (0.037) - this node is a cross-community bridge._
- **What connects `researchProject.Models.ViewModels.LoginViewModel`, `researchProject.Models.Entities`, `College` to the rest of the system?**
  _549 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `IActionResult` be split into smaller, more focused modules?**
  _Cohesion score 0.10884353741496598 - nodes in this community are weakly interconnected._
- **Why does `Evaluation` connect `Evaluation` to `Research`, `DateTime`, `Fact`, `College`, `Judge`, `CertificateService`, `CollegeCoAdminRepository`, `AccountRepository`, `ResearchAttachment`, `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs`, `4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)`, `system`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **Should `HangfireAuditAndErrorHandlingTests.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.14589371980676327 - nodes in this community are weakly interconnected._
- **Why does `Notification` connect `Notification` to `ApplicationUser`, `NotificationService`, `DateTime`, `ApplicationDbContext`, `HangfireEmailDispatcher.SendEmailWithRollbackAsync`, `AccountRepository`, `system`?**
  _High betweenness centrality (0.032) - this node is a cross-community bridge._
- **Should `Research` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._