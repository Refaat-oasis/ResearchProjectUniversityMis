# Graph Report - researchProject  (2026-11-18)

## Corpus Check
- 239 files · ~123,915 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 5 file(s) not represented in the graph (top: (none) 2, .css 2, .ico 1)

## Summary
- 1805 nodes · 4520 edges · 164 communities (67 shown, 97 thin omitted)
- Extraction: 84% EXTRACTED · 16% INFERRED · 0% AMBIGUOUS · INFERRED: 726 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `41fedab1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- researchProject.Migrations
- microsoft_entityframeworkcore
- Research
- IActionResult
- Evaluation
- Fact
- ILogger
- PublicController
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
- SystemStatisticsViewModel
- ResearchStatusHistory
- SeoMetadataViewModel
- SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType
- AuditLogViewModel
- Task
- HttpGet
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
- DateTime
- CertificateService
- CollegeCoAdminRepository
- AccountRepository
- ResearchAttachment
- JudgeRepository
- EmailSender
- المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي
- ResearcherRepository
- AuditLogListViewModel
- 4. Comprehensive Cross-Reference Index
- IFileStorageService
- Program.cs
- AttachmentService
- Notification
- Judge
- MigrationBuilder
- Controller
- AccountRepository
- ResearcherSignUpDto
- UserProfileViewModel
- AccountController
- notifications.js
- system
- ResearchCategory
- CollegeCoAdmin
- ISuperAdminRepository.GetAllCollegesAsync
- EnumExtensions
- ProfileViewModel
- ResearchCreateDto
- researchProject.Models.Enums
- ErrorController
- AttachmentType
- InvitationStatus
- UserRole
- ResearchStatus
- VerifyEmailViewModel
- ResearchModificationDto
- 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs
- CertificateVerificationResultViewModel
- EthicsRating
- EvaluationDecision
- UserSummaryViewModel
- IEmailSender
- NewRequirementsAndEnhancementsTests
- _ResearchStatusTrackerPartial.cshtml
- AuditLogFilter.cs
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
- EmailVerificationCode
- SuperAdmin/Categories.cshtml
- SuperAdmin/Certificates.cshtml
- List<researchProject.Models.Entities.CollegeCoAdmin>
- List<researchProject.Models.Entities.College>
- _ManageResearchersPartial.cshtml
- SuperAdmin/Reports.cshtml
- Researchers.cshtml
- AuditLog
- AuditLogs.cshtml
- ResearchCreateViewModel
- 2. Master Catalog of Verified Bugs
- JudgeToResearcherDto
- IReportExportService
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
1. `AccountRepository` - 61 edges
2. `ApplicationDbContext` - 56 edges
3. `College` - 47 edges
4. `SuperAdminController` - 47 edges
5. `ResearchCategory` - 47 edges
6. `Judge` - 46 edges
7. `SuperAdminRepository` - 44 edges
8. `ApplicationUser` - 39 edges
9. `researchProject.Models.Entities` - 39 edges
10. `CollegeCoAdminRepository` - 38 edges

## Surprising Connections (you probably didn't know these)
- `1. مهام التنفيذ` --references--> `ApplicationUser`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ApplicationUser.cs
- `BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions` --references--> `IReportExportService`  [INFERRED]
  BUGS.md → researchProject/Services/IReportExportService.cs
- `2. اختبارات المرحلة السابعة` --references--> `ResearchStatusHistory`  [INFERRED]
  multi_phase_implementation_testing_plan.md → researchProject/Models/Entities/ResearchStatusHistory.cs
- `7. Evaluation (Review) & Round Closing` --references--> `JudgeRepository`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Repositories/Implementations/JudgeRepository.cs
- `1. Entity & Database Rules (ERD Compliance)` --references--> `College`  [INFERRED]
  ai_agent_instructions_architecture_rules.md → researchProject/Models/Entities/College.cs

## Import Cycles
- None detected.

## Communities (164 total, 97 thin omitted)

### Community 1 - "microsoft_entityframeworkcore"
Cohesion: 0.12
Nodes (14): researchProject.Data, researchProject.Tests, researchProject.Services.Interfaces, researchProject.Hubs, researchProject.Services.Implementations, researchProject.Models.Constants, Microsoft.AspNetCore.Identity, AttachmentTypeConstants (+6 more)

### Community 2 - "Research"
Cohesion: 0.10
Nodes (21): Research, AcademicSpecialty, Category, CategoryId, CertificateFilePath, CertificateIssuedAt, CertificateQrCodeUrl, CertificateSerialNumber (+13 more)

### Community 4 - "Evaluation"
Cohesion: 0.07
Nodes (29): BUG-02: Missing Validation on Inactive/Invalid Judges Traps Research in `AssignedToJudges` with 0 Evaluations (Deadlock), BUG-03: Zombie Research Deadlock on Modifications Resubmission with Deactivated Judges, BUG-04: Round Number Corruption and Previous Judge Lockout on Research Re-assignment, BUG-05: Assigning Judges During `ModificationsRequired` State Locks Researcher Out, BUG-07: Premature Verification OTP Invalidation (Burned) Before Password Complexity Check, BUG-09: Unhandled `FormatException` in `ResendVerificationCode` When Passing `UserId` Instead of Email, BUG-12: Certificate Archive Date Range Filter Truncates Matches on Target End Date, BUG-13: Unhandled `NullReferenceException` in Judge Specialty Recommendation Ordering (+21 more)

### Community 5 - "Fact"
Cohesion: 0.12
Nodes (4): DeveloperController, EightFeaturesAndEnhancementsTests, EntityAndDatabaseRulesTests, ExtendedWorkflowAndEdgeCaseTests

### Community 7 - "PublicController"
Cohesion: 0.20
Nodes (7): 8. Certificate, Digital Signature & Archive, 1. مهام التنفيذ, 2. اختبارات المرحلة السابعة, المرحلة السابعة (Phase 7): الاعتماد النهائي وإصدار الشهادة والتحقق, PublicController, IPublicRepository, ICertificateService

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

### Community 13 - "CollegeCoAdminController"
Cohesion: 0.11
Nodes (3): 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository), CollegeCoAdminController, ICollegeCoAdminRepository

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
Cohesion: 0.17
Nodes (6): EmailJobType, CoAdminCredentials, JudgeInvitation, HangfireEmailDispatcher, IHangfireEmailDispatcher, HangfireAuditAndErrorHandlingTests

### Community 18 - "SystemStatisticsViewModel"
Cohesion: 0.12
Nodes (15): SystemStatisticsViewModel, ApprovedCount, AssignedToJudgesCount, AverageJudgingDays, AverageScreeningHours, CertificatesIssuedCount, ModificationsRequiredCount, ResearchesByCategory (+7 more)

### Community 19 - "ResearchStatusHistory"
Cohesion: 0.07
Nodes (33): 0. Identity Model Summary (READ FIRST), 10. Security & Data Isolation, 11. Seeding & Configuration, 1. Entity & Database Rules (ERD Compliance), 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution, 5. UI/UX, RTL, Right Sidebar & Partial Views, 6. Research Status Workflow (Strict), 7. Evaluation (Review) & Round Closing (+25 more)

### Community 20 - "SeoMetadataViewModel"
Cohesion: 0.09
Nodes (15): researchProject.Models, HomeController, ErrorViewModel, RequestId, ShowRequestId, SeoMetadataViewModel, Author, CanonicalUrl (+7 more)

### Community 21 - "SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType"
Cohesion: 0.14
Nodes (10): ChangePasswordDto, ConfirmPassword, CurrentPassword, NewPassword, UpdateProfileDto, AcademicDegree, FullName, PhoneNumber (+2 more)

### Community 22 - "AuditLogViewModel"
Cohesion: 0.11
Nodes (17): AuditLogViewModel, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 24 - "HttpGet"
Cohesion: 0.11
Nodes (3): BUG-11: SuperAdmin Proposal Creation Fails with Null Profile and Hardcoded College Fallback, ResearcherController, IResearcherRepository

### Community 26 - "site.js"
Cohesion: 0.18
Nodes (6): initCustomDatalists(), initDedicatedDropdowns(), filterOptions(), getItems(), initUniversalTablePagination(), resolveInitialRoute()

### Community 30 - "system_threading_tasks"
Cohesion: 0.11
Nodes (3): researchProject.Models.ViewModels, researchProject.Repositories.Interfaces, researchProject.Models.Entities

### Community 32 - "Login.cshtml"
Cohesion: 0.40
Nodes (4): researchProject.Models.ViewModels.LoginViewModel, College, researchProject.Models.Entities, researchProject.Models.Enums

### Community 52 - "DateTime"
Cohesion: 0.23
Nodes (3): AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers, ApplicationDbContextModelSnapshot

### Community 53 - "CertificateService"
Cohesion: 0.22
Nodes (7): BUG-21: Concurrent OTP Brute-Force Race Condition Bypassing 5-Attempt Lockout Limit, BUG-22: Stored Cross-Site Scripting (XSS) and Raw HTML Injection in Generated Certificates, BUG-23: Arbitrary File Disclosure via Path Traversal in Certificate Signatures and File Streaming, BUG-24: Insecure Storage of Authentication Verification OTP Codes in Plaintext in Database, BUG-25: Arithmetic Overflow `Math.Abs(int.MinValue)` in Certificate Serial Number Generation, Category 3: Security & Input Validation, CertificateService

### Community 54 - "CollegeCoAdminRepository"
Cohesion: 0.16
Nodes (13): BUG-08: Digital Certificate Issuance Bypasses Mandatory Co-Admin Signature Image Requirement (Specification Conflict), College, Code, CollegeCoAdmins, Departments, Id, Name, ResearchCategories (+5 more)

### Community 55 - "AccountRepository"
Cohesion: 0.21
Nodes (6): 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up), ApplicationUser, CreatedAt, FullName, IsActive, OtpVerificationAndDatalistTests

### Community 56 - "ResearchAttachment"
Cohesion: 0.17
Nodes (12): ResearchAttachment, AttachmentType, ContentType, FilePath, FileSize, Id, OriginalFileName, Research (+4 more)

### Community 57 - "JudgeRepository"
Cohesion: 0.14
Nodes (16): BUG-10: Arbitrary Evaluation Decision Value Bypass Defaulting to Modifications Required, BUG-16: Invalid `RollbackAsync` Invoked on Already Committed Database Transaction on Notification Failure, BUG-17: Unhandled SignalR Exception in Judge Evaluation Transaction Rolls Back Official Evaluation, BUG-18: Race Condition and Lost Updates in Concurrent Multi-Judge Evaluation Submissions, BUG-19: Missing College Isolation Validation Enables Cross-College Proposal Injection, BUG-20: N+1 Database Query Cascade in SuperAdmin Role Retrieval for Judge Invitations, Category 2: Concurrency, Transactions & Data Integrity, EvaluationSubmitDto (+8 more)

### Community 59 - "المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي"
Cohesion: 0.25
Nodes (7): ResearchProjectUniversityMis, 🏛️ البوابات الرئيسية وأدوار النظام الأربعة, 🧪 التحقق والاختبارات الآلية, 🚀 التشغيل المحلي, 🛠️ التقنيات والمعايير المستخدمة, المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي, 🔄 مسار دورة العمل الرقمية (Workflow Statuses)

### Community 61 - "AuditLogListViewModel"
Cohesion: 0.15
Nodes (10): AuditLogListViewModel, CurrentPage, FromDate, Logs, RoleFilter, SearchTerm, SuccessFilter, ToDate (+2 more)

### Community 62 - "4. Comprehensive Cross-Reference Index"
Cohesion: 0.12
Nodes (17): 4. Comprehensive Cross-Reference Index, BUG-33: EF Core Domain Entities Bound Directly as Controller Action Input Parameters (Mass Assignment), Excluded / Resolved Items Reference Table, CategorySaveDto, CollegeId, Description, FieldsDefinitionJson, Id (+9 more)

### Community 65 - "AttachmentService"
Cohesion: 0.15
Nodes (8): BUG-01: Missing Attachment Download Endpoint and UI Action Links (Functional Blocker), AttachmentController, VerifyEmailDto, EmailOrUserId, Token, AttachmentService, IAttachmentService, AttachmentAndSecurityFixTests

### Community 66 - "Notification"
Cohesion: 0.06
Nodes (31): BUG-26: Redundant Group Subscription in `NotificationHub` Desynchronized from `Clients.User` Dispatch, BUG-37: Artificial Unit Test Scenario Concealing Round Incrementation Defect, BUG-38: Overly Broad SignalR Test Mocks Resulting in False-Positive Verification, BUG-39: Test Suite Omits Primary 6-Digit OTP Verification Flow, Category 4: SignalR & Real-Time Communication, Category 8: Test Suite Gaps & Blind Spots, NotificationHub, Notification (+23 more)

### Community 67 - "Judge"
Cohesion: 0.09
Nodes (24): 1. مهام التنفيذ, 1. مهام التنفيذ, 1. مهام التنفيذ, 2. اختبارات المرحلة الأولى, 2. اختبارات المرحلة الثالثة, 2. اختبارات المرحلة الثامنة, 2. اختبارات المرحلة السادسة, المرحلة الأولى (Phase 1): Identity والكلاسات وقاعدة البيانات (+16 more)

### Community 68 - "MigrationBuilder"
Cohesion: 0.10
Nodes (10): InitialCreate, init1, AddDepartments, AddEmailVerificationCodeTable, UpdateEmailVerificationCodeLength, AddNationalIdUniqueAndUserCreatedAt, AddIsActiveToCategoriesAndUsers, AddFileDataAndCollegeToJudges (+2 more)

### Community 69 - "Controller"
Cohesion: 0.19
Nodes (3): 3.1 Detailed Audit: Rule 1 — Thin Controllers Principle, JudgeController, IJudgeRepository

### Community 70 - "AccountRepository"
Cohesion: 0.09
Nodes (5): ApplicationUser, BUG-36: Complete Absence of Test Coverage (0%) for Core Security, Auth, Storage, and Controllers, AccountRepository, UnconfirmedEmailCleanupService, IUnconfirmedEmailCleanupService

### Community 71 - "ResearcherSignUpDto"
Cohesion: 0.18
Nodes (11): ResearcherSignUpDto, AcademicDegree, CollegeId, ConfirmPassword, Department, Email, FullName, NationalId (+3 more)

### Community 72 - "UserProfileViewModel"
Cohesion: 0.10
Nodes (20): UserProfileViewModel, AvailableColleges, CanBecomeResearcher, Email, FullName, IsCollegeCoAdmin, IsJudge, IsResearcher (+12 more)

### Community 74 - "notifications.js"
Cohesion: 0.36
Nodes (10): buildNotificationItemHtml(), escapeHtml(), handleIncomingNotification(), initSignalR(), loadRecentNotifications(), loadUnreadCount(), markAndNavigate(), playSoftBeep() (+2 more)

### Community 75 - "system"
Cohesion: 0.11
Nodes (4): researchProject.Models.DTOs, researchProject.Models.Entities, ResendVerificationCodeDto, EmailOrUserId

### Community 76 - "ResearchCategory"
Cohesion: 0.14
Nodes (9): ResearchCategory, College, CollegeId, Description, FieldsDefinitionJson, Id, IsActive, Name (+1 more)

### Community 77 - "CollegeCoAdmin"
Cohesion: 0.17
Nodes (11): BUG-32: Input DTOs Used Directly as Models in ResearcherController Razor Views, BUG-35: EF Core Entities Used Directly as Presentation Models across Views, Category 7: Architecture Rule 4 — Separation Between ViewModels and DTOs, CollegeCoAdmin, College, CollegeId, Id, IsActive (+3 more)

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

### Community 88 - "VerifyEmailViewModel"
Cohesion: 0.12
Nodes (15): ResearchModificationViewModel, CurrentRound, ErrorMessage, ModificationNotes, Research, ResearcherNotes, ResearchId, Title (+7 more)

### Community 89 - "ResearchModificationDto"
Cohesion: 0.22
Nodes (5): ResearchModificationDto, ModifiedProposalFile, ModifiedToolsFile, ResearcherNotes, ResearchId

### Community 90 - "3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs"
Cohesion: 0.11
Nodes (19): 3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs, BUG-06: Deceptive Success and Password Desynchronization on Logged-Out Existing User Judge Invitation Acceptance, AcceptInvitationDto, AcademicTitle, ConfirmPassword, FullName, Password, Token (+11 more)

### Community 91 - "CertificateVerificationResultViewModel"
Cohesion: 0.25
Nodes (7): CertificateVerificationResultViewModel, CollegeName, IssueDate, IsValid, Message, ResearchTitle, SerialNumber

### Community 92 - "EthicsRating"
Cohesion: 0.29
Nodes (4): EthicsRating, Appropriate, Inappropriate, NeedsModification

### Community 93 - "EvaluationDecision"
Cohesion: 0.29
Nodes (6): EnumExtensions, EvaluationDecision, Approved, ClosedDueToOtherJudge, ModificationsRequired, UnderEvaluation

### Community 94 - "UserSummaryViewModel"
Cohesion: 0.09
Nodes (20): 1.1 Key Metrics & Remediation Status, 1.2 Breakdown by Severity, 1.3 Breakdown by Category, 1. Executive Summary & Statistics Tables, 3.2 Detailed Audit: Rule 2 — Native ASP.NET Core Identity & Claims, 3.3 Detailed Audit: Rule 3 — Single Type Per File Principle, 3. Architecture Rules Compliance Scorecard, BUG-34: Empty Shell DTO Subclasses ViewModel Violating Layer Isolation (+12 more)

### Community 114 - "EmailVerificationCode"
Cohesion: 0.18
Nodes (11): EmailVerificationCode, AttemptsCount, Code, CreatedAt, Email, ExpiresAt, Id, IsUsed (+3 more)

### Community 124 - "AuditLog"
Cohesion: 0.12
Nodes (17): AuditLog, ActionName, ControllerName, Description, ExceptionDetails, ExecutionDurationMs, HttpMethod, Id (+9 more)

### Community 131 - "ResearchCreateViewModel"
Cohesion: 0.20
Nodes (9): ResearchCreateViewModel, AcademicSpecialty, AvailableCategories, CategoryId, CollegeId, CollegeName, DynamicAnswers, ErrorMessage (+1 more)

### Community 135 - "2. Master Catalog of Verified Bugs"
Cohesion: 0.22
Nodes (9): 2. Master Catalog of Verified Bugs, BUG-27: Direct CSV Report Construction, Formatting, and Encoding inside Controller Actions, BUG-28: Direct Form Data Key Iteration and Dynamic Field Parsing in Controller Action, BUG-29: Controllers Executing Direct Domain Profile Resolution via Repositories, BUG-30: Authentication Cookie and Claims Not Refreshed upon Accepting Judge Role, Category 5: Architecture Rule 1 — Thin Controllers Principle, Category 6: Architecture Rule 2 — Native Identity & Claims, EXCLUDED-01 (Formerly BUG-14): Uncontrolled Cascading Polling Timers in Research Status Tracker Partial View (+1 more)

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
- **547 isolated node(s):** `Code`, `CollegeCoAdmins`, `Departments`, `Id`, `Name` (+542 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 748 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **97 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Judge` connect `Judge` to `Evaluation`, `Controller`, `المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي`, `Researcher`, `ResearchCategory`, `CollegeCoAdmin`, `CollegeCoAdminController`, `ApplicationDbContext`, `ResearchStatusHistory`, `system_threading_tasks`, `Task`, `CollegeCoAdminRepository`, `AccountRepository`, `SevenEnhancementsAndFixesTests.ProfileManagement_ResearcherAndJudge_CanUpdateAcademicDegreeAndResearcherType`, `JudgeRepository`, `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs`, `UserSummaryViewModel`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **What connects `Code`, `CollegeCoAdmins`, `Departments` to the rest of the system?**
  _547 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `microsoft_entityframeworkcore` be split into smaller, more focused modules?**
  _Cohesion score 0.11717171717171718 - nodes in this community are weakly interconnected._
- **Why does `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs` connect `3.4 Detailed Audit: Rule 4 — Separation Between ViewModels and DTOs` to `Research`, `IActionResult`, `Evaluation`, `Judge`, `ResearcherSignUpDto`, `AccountController`, `ResearchCategory`, `ResearchCreateDto`, `CollegeCoAdminRepository`, `HttpGet`, `ResearchModificationDto`, `UserSummaryViewModel`?**
  _High betweenness centrality (0.043) - this node is a cross-community bridge._
- **Should `Research` be split into smaller, more focused modules?**
  _Cohesion score 0.09523809523809523 - nodes in this community are weakly interconnected._
- **Why does `researchProject.Models.Enums` connect `researchProject.Models.Enums` to `microsoft_entityframeworkcore`, `EnumExtensions`, `InvitationStatus`, `ResearchStatus`, `EthicsRating`?**
  _High betweenness centrality (0.042) - this node is a cross-community bridge._
- **Should `IActionResult` be split into smaller, more focused modules?**
  _Cohesion score 0.10062893081761007 - nodes in this community are weakly interconnected._