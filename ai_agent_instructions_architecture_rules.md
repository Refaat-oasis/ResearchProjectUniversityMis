# AI Agent Instructions & Project Rules (`AGENTS.md`)

You are an expert ASP.NET Core MVC & SQL Server Architect working on the **Research Ethics & Academic Integrity E-Platform (المنصة الإلكترونية للجان الفرعية للنزاهة الأكاديمية وأخلاقيات البحث العلمي)**. You must strictly adhere to every architectural, database, security, and UI rule below whenever generating or modifying code. The product scope comes from `project_idea_documentation.md`; the delivery order comes from `multi_phase_implementation_testing_plan.md`.

---

## 0. Identity Model Summary (READ FIRST)

* There is **ONE user class**: `ApplicationUser : IdentityUser` (table `AspNetUsers`). It is the **only** place for email, password hash, phone number, lockout, and login data.
* Actors are distinguished by **Identity Roles**: `SuperAdmin`, `CollegeCoAdmin`, `Judge`, `Researcher`. One user **may hold several roles** (e.g. `Researcher` + `Judge`).
* `Researcher`, `CollegeCoAdmin`, and `Judge` are **profile tables** (domain data only) linked 1:0..1 to `ApplicationUser` via a unique `UserId`.
* **`SuperAdmin` has NO class and NO table**; it is a role on a seeded account.
* **NEVER** create separate login/user/password tables or password/email/hash columns per actor.
* The system has **exactly four roles**: `SuperAdmin`, `CollegeCoAdmin` (one or more per college), `Judge`, `Researcher`. The administrative titles in the product brief (committee secretary/coordinator, technical examiner, committee head) are all covered by `CollegeCoAdmin`. Do not add roles for them.
* **The `SuperAdmin` takes no action in final approval or certificate issuance** (Section 8). It may view them only.

---

## 1. Entity & Database Rules (ERD Compliance)

1. **9 Core Domain Tables:** Never add, remove, or change the logical relationships of these tables beyond what is stated here:
   * `College` ($1:M$ with `CollegeCoAdmin`, `Researcher`, `ResearchCategory`)
   * `CollegeCoAdmin` ($1:1$ with `ApplicationUser` via `UserId`; belongs to one `College`)
   * `Researcher` ($1:1$ with `ApplicationUser` via `UserId`; $1:M$ with `ResearcherResearchesBridge`)
   * `ResearchCategory` ($1:M$ with `Research`)
   * `Research` ($1:M$ with `ResearcherResearchesBridge`, `ResearchAttachment`, **`Evaluation`**, and `ResearchStatusHistory`)
   * `ResearcherResearchesBridge`
   * `ResearchAttachment`
   * `Judge` ($1:1$ with `ApplicationUser` via `UserId`; $1:M$ with `Evaluation`)
   * `Evaluation`
2. **2 Supporting tables (allowed in addition):**
   * `JudgeInvitation` (Section 3).
   * `ResearchStatusHistory` (`ResearchId`, `OldStatus`, `NewStatus`, `ChangedByUserId`, `ChangedAt`, `Reason`).
3. **ASP.NET Identity tables** are infrastructure and are not counted among the 9 core tables.
4. **Evaluation = one row per (Research, Judge, Round):**
   * A research may have **several judges per round**, and several rounds over time.
   * Enforce `HasIndex(e => new { e.ResearchId, e.JudgeId, e.RoundNumber }).IsUnique()`. No unique index on `ResearchId` alone.
   * The **current round** of a research is its highest `RoundNumber`.
5. **Profile table rules:** `UserId` with `HasIndex(x => x.UserId).IsUnique()` on `Researcher`, `CollegeCoAdmin`, `Judge`. No `Email`/`Password` columns in them. Email, phone, and full name come from `ApplicationUser` (`Email`, `PhoneNumber`, `FullName`).
6. **Scalar columns that may be added to the core tables** (if not already present). No other structural changes:
   * `Researcher`: `ResearcherType` (values from `ResearcherTypeConstants`), `AcademicDegree`, `Department`, `NationalId`, `CollegeId`.
   * `Judge`: `Specialty`, `AcademicTitle`, `IsActive`.
   * `CollegeCoAdmin`: `SignatureImagePath` (the Co-Admin's signature image, stamped on certificates they issue).
   * `ResearchCategory`: `FieldsDefinitionJson` (the extra form fields for this research type).
   * `Research`: `AcademicSpecialty`, `DynamicFieldsJson` (the researcher's answers to the category fields), `SubmittedAt`, `ScreeningDueAt`, `ScreeningNotes`, `CertificateSerialNumber`, `CertificateQrCodeUrl`, `CertificateIssuedAt`, `CertificateFilePath`.
   * `ResearchAttachment`: `AttachmentType` (values from `AttachmentTypeConstants`), `Version`, `OriginalFileName`, `StoredFileName`, `FileSize`.
   * `Evaluation`: `RoundNumber`, `Decision` (`EvaluationDecisionConstants`), `PrivacyProtectionRating`, `RiskLevel`, `InformedConsentRating` (values from `EthicsRatingConstants`), `Recommendations`, `ModificationNotes`, `IsDigitallySigned`, `SignedAt`, `SubmittedAt`.
7. **Entity class constraints (CRITICAL):**
   * Entity classes in `Models/Entities/` may contain **only** scalar properties, foreign-key properties, navigation properties, and **Arabic Data Annotations** (`[Display]`, `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`).
   * **No** business logic, methods, calculated properties, or English validation messages in entities.
   * Configure relationships, foreign keys, indexes, and delete behavior (`Restrict` by default) in `ApplicationDbContext.OnModelCreating` with Fluent API.
8. `ApplicationDbContext` inherits `IdentityDbContext<ApplicationUser>`.

```csharp
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; }
}
```

**Constants** live in `Models/Constants/`: `ResearchStatusConstants` (Section 6), `RoleConstants`, `InvitationStatusConstants` (`Pending`, `Accepted`, `Expired`, `Revoked`, in Arabic), `ResearcherTypeConstants` (`عضو هيئة تدريس`, `باحث`, `طالب دراسات عليا`), `AttachmentTypeConstants` (`الموافقة المستنيرة`, `أدوات البحث`, `خطة البحث`, `مستند إضافي`, `ملف معدل`), `EvaluationDecisionConstants` (`قيد التقييم`, `موافق`, `طلب تعديلات`), `EthicsRatingConstants` (`مناسب`, `يحتاج تعديل`, `غير مناسب`). `EvaluationDecisionConstants` is separate from research statuses and must never be stored in `Research.WorkflowStatus`.

---

## 2. Authentication Rules (ASP.NET Core Identity, Single Login, Single Sign-Up)

1. Use `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, `RoleManager<IdentityRole>`. **Never** hand-roll password hashing or check passwords against profile tables.
2. **Single Login Page (`Views/Account/Login.cshtml`):** one page for all actors. `AccountRepository` uses `SignInManager.PasswordSignInAsync` with lockout enabled. The cookie carries all the user's role claims. If the user has several roles, open the highest-priority dashboard (`SuperAdmin` > `CollegeCoAdmin` > `Judge` > `Researcher`); the sidebar exposes the rest.
3. **Single Sign-Up Page (`Views/Account/SignUp.cshtml`):** Researchers only. Fields: `College`, `FullName`, `Email`, `Password`, `ResearcherType`, `AcademicDegree`, `Department`, `NationalId`, `PhoneNumber`.
   * **Institutional email (SSO requirement):** the email domain must be in configuration key `Auth:AllowedEmailDomains`. Reject other domains with an Arabic message. Do not hardcode domains.
   * **Email confirmation is required** (`RequireConfirmedEmail = true`). Send the confirmation link through `IEmailSender`; the account cannot log in before confirming.
   * Creating the `ApplicationUser`, the `Researcher` role, and the `Researcher` profile happens in **one transaction**.
4. `CollegeCoAdmin` accounts are **not** self-registered; only the **Super Admin** adds them:
   * New email: create the user with a generated temporary password, mark `EmailConfirmed = true`, email it, and force a password change on first login.
   * Existing user (e.g. a researcher): add the `CollegeCoAdmin` role to the same account.
   * Create the `CollegeCoAdmin` profile linked to the selected `College`.
5. `Judge` accounts are created **only** through the invitation flow (Section 3). The institutional-domain restriction does **not** apply to judges (they may be external experts). Accepting an invitation proves email ownership, so set `EmailConfirmed = true`.
6. **Claims:** authorization relies on role claims. Do not rely on a single `CollegeId` claim. Repositories resolve the profile (`Researcher`, `CollegeCoAdmin`, `Judge`) from `ClaimTypes.NameIdentifier` whenever they need `CollegeId`, `ResearcherId`, or `JudgeId`.
7. **Future SSO:** real SSO (OpenID Connect with the university's Microsoft/Google tenant) is added later as an external login on the same `ApplicationUser`. Do not build it before Phase 8.

---

## 3. Judges: Dual Role, Invitations, Scope, Conflict of Interest, Distribution

1. **A Researcher can also be a Judge** with the **same account**: both roles on one `ApplicationUser`, a row in both profile tables. Never create a second account for the same person.
2. **Becoming a Judge requires the person's consent** via invitation. Never silently attach the `Judge` role to an existing user.
3. **Who invites:** `CollegeCoAdmin` and `SuperAdmin`. The judge pool is university-wide. **Deactivation** (`Judge.IsActive = false`) is `SuperAdmin` only and keeps evaluation history.
4. **`JudgeInvitation`:** `Id`, `Email`, `TokenHash`, `InvitedByUserId`, `Specialty` (optional), `CreatedAt`, `ExpiresAt` (default 7 days), `AcceptedAt`, `Status`.
   * Generate the token with `RandomNumberGenerator` (32 bytes). Store only its **SHA-256 hash**; the raw token exists only in the emailed link.
   * Reject used, expired, or revoked tokens.
5. **Flow:**
   1. The inviter types an email **or** picks an existing researcher (search by name/email). Both use the same mechanism.
   2. **Existing user:** the link opens a confirmation page. The user must be signed in and the signed-in email must equal the invitation email. On acceptance add the `Judge` role and create the `Judge` profile.
   3. **New user:** the link opens `Views/Account/AcceptJudgeInvitation.cshtml` (full public view) to enter name and password; create the `ApplicationUser` with the `Judge` role and the `Judge` profile.
   4. Mark the invitation `Accepted`.
6. **Assignment scoping (CRITICAL):** the `Judge` role does **not** grant access to all researches. A judge sees **only** researches with an `Evaluation` row carrying their own `JudgeId`.
7. **Conflict of interest (CRITICAL):** a judge must **never** evaluate a research they authored (present in `ResearcherResearchesBridge`). Enforce in **all** places:
   * `SuggestJudgesAsync` and `GetActiveJudgesAsync(researchId)` exclude such judges.
   * `AssignJudgesToResearchAsync` rejects them with an Arabic error.
   * `JudgeRepository` excludes such researches from lists and **re-checks on every action** (open, submit, approve).

```csharp
bool isAuthor = await _db.ResearcherResearchesBridge
    .AnyAsync(b => b.ResearchId == researchId && b.Researcher.UserId == judge.UserId);
if (isAuthor) return Result.Fail("لا يمكن إسناد البحث لأحد باحثيه");
```

8. **Automatic distribution (suggestion + confirmation):** `CollegeCoAdminRepository.SuggestJudgesAsync(researchId)` returns active judges ranked by (1) `Judge.Specialty` equal to `Research.AcademicSpecialty`, then (2) lowest count of open evaluations; always excluding authors and inactive judges. The Co-Admin confirms one or more judges in one click. Specialties come from the configuration list `Specialties`, so matching is exact. Number of judges per research is `Evaluation:MinJudgesPerResearch` (default 1).
9. **Independent review:** while a round is open, a judge cannot see other judges' evaluations. The researcher never sees judge identities; modification notes are shown without names.

---

## 4. Architecture: Dedicated Controller & Repository Per Actor (NO Generic Repository)

* **No `IGenericRepository<T>`.** Each actor has its own Controller and Repository. Controllers never inject `ApplicationDbContext`, `UserManager`, or `SignInManager`; they inject only their own repository (Identity managers are used inside repositories).

| Actor | Controller / Repository | Responsibilities |
|---|---|---|
| Authentication | `AccountController` / `IAccountRepository` | Login (SignInManager), Researcher sign-up with institutional-domain check and email confirmation, sign-out, confirm email, accept judge invitation. |
| Super Admin | `SuperAdminController` / `ISuperAdminRepository` (`[Authorize(Roles="SuperAdmin")]`) | Colleges; research categories **and their dynamic field definitions**; Co-Admins; judge invitations and activation; sees all researches/evaluations/attachments/history/certificates; may perform any Co-Admin, Judge, or Researcher action **except final approval and certificate issuance (view only)**; reports, statistics, and certificate archive. |
| College Co-Admin | `CollegeCoAdminController` / `ICollegeCoAdminRepository` (Co-Admin and SuperAdmin) | Screening of **own college** researches (24–48 h target), `ScreeningNotes`, judge invitations, judge suggestions and assignment, final approval and certificate issuance (**exclusive to the Co-Admin of the research's college**; the issuing actions must be `[Authorize(Roles="CollegeCoAdmin")]` only, and hidden from SuperAdmin), college statistics. |
| Judge | `JudgeController` / `IJudgeRepository` (Judge and SuperAdmin) | Assigned researches only, ethics evaluation form, decision and digital signature. |
| Researcher | `ResearcherController` / `IResearcherRepository` (Researcher and SuperAdmin) | Dynamic submission form, attachments, status tracker, modification uploads, certificates. Own researches only. |
| Public visitor | `PublicController` / `IPublicRepository` (`[AllowAnonymous]`) | Certificate verification page opened from the QR code. Returns minimal data only. |

* **Shared services** (not entity repositories): `IEmailSender`, `ICertificateService` (serial number, QR code, PDF), `IFileStorageService` (attachments outside `wwwroot`).

---

## 5. UI/UX, RTL, Right Sidebar & Partial Views

1. **100% Arabic & RTL:** all layouts, labels, alerts, buttons, table headers, validation messages, and emails in Arabic with `dir="rtl"`; use `bootstrap.rtl.min.css`.
2. **Right Sidebar (`_RightSidebarPartial.cshtml`):** fixed on the **right**, with `#main-content-container` on the left.
   * `SuperAdmin`: all sections.
   * Others: the sections of **every role the user holds** (e.g. researcher-judge sees both), grouped under clear headings via `User.IsInRole(...)`. The three portals of the brief map to: Researcher section, Judge/Committee section, Admin section.
3. **Partial Views first:** every feature screen is a Partial View (`_NamePartial.cshtml`) loaded by AJAX into `#main-content-container`. The only full views are `_Layout.cshtml`, `Login.cshtml`, `SignUp.cshtml`, `AcceptJudgeInvitation.cshtml`, and the public certificate verification page.
   * After injecting a partial with a `<form>`, call `$.validator.unobtrusive.parse('#form-id')`.
   * Send the anti-forgery token with every AJAX POST.
   * Direct URLs and refresh must still work (a `#hash` loads the matching partial).
4. **Dynamic submission form (mechanism only):** the concrete extra fields per research type are **not yet defined by the project owner**. Build the definition/rendering/validation mechanism and do **not** invent fields; they will be entered later from the Super Admin screen.
   **Behavior:** `_DynamicCategoryFieldsPartial.cshtml` renders the inputs defined in the selected category's `FieldsDefinitionJson` (types: text, long text, number, date, dropdown, yes/no, required flag, Arabic label). Validate the answers on the server against the same definition, then save them to `Research.DynamicFieldsJson`.
5. **Status tracker:** `_ResearchStatusTrackerPartial.cshtml` shows the five stages visually and refreshes by polling every 30 seconds (SignalR is optional later).

---

## 6. Research Status Workflow (Strict)

`Research.WorkflowStatus` may only use these 5 exact Arabic strings across all C#, SQL, and Razor:

```csharp
public static class ResearchStatusConstants
{
    public const string UnderFormalScreening  = "قيد المراجعة الشكلية";
    public const string AssignedToJudges      = "مسند للمحكمين";
    public const string ModificationsRequired = "تعديلات مطلوبة";
    public const string Approved              = "معتمد";
    public const string CertificateIssued     = "صادر الشهادة";
}
```

**Allowed transitions.** Each one inserts a `ResearchStatusHistory` row with the acting `UserId` and sends the listed email:

| From | To | Triggered by | Email |
|---|---|---|---|
| (new) | `قيد المراجعة الشكلية` | Researcher submits; set `SubmittedAt` and `ScreeningDueAt = SubmittedAt + Screening:SlaHours` (default 48) | Researcher (receipt), Co-Admin (new) |
| `قيد المراجعة الشكلية` | `مسند للمحكمين` | Co-Admin confirms judges; create round 1 `Evaluation` rows (`Decision = قيد التقييم`) | Judges |
| `مسند للمحكمين` | `تعديلات مطلوبة` | Round closes and at least one judge chose `طلب تعديلات` | Researcher |
| `تعديلات مطلوبة` | `مسند للمحكمين` | Researcher uploads modified documents; create round N+1 rows for the previous judges (Co-Admin may reassign if a judge became inactive) | Judges |
| `مسند للمحكمين` | `معتمد` | Round closes and **all** judges chose `موافق` | Researcher, Co-Admin |
| `معتمد` | `صادر الشهادة` | Co-Admin of the research's college performs final approval and issuance (never SuperAdmin) | Researcher (certificate PDF) |

Reject any other transition.

---

## 7. Evaluation (Review) & Round Closing

**There is no voting.** Each judge submits an independent review. The outcome rule is: if **all** judges of the round approve, the research is approved; if **any** judge requests modifications, the research needs more work.

1. The evaluation form captures the ethics criteria from the brief: `PrivacyProtectionRating` (حماية الخصوصية), `RiskLevel` (نسبة المخاطر), `InformedConsentRating` (الموافقة المستنيرة), plus `Recommendations`, `ModificationNotes`, and the `Decision`.
2. Submitting sets `SubmittedAt`, `IsDigitallySigned = true`, `SignedAt`. `طلب تعديلات` requires `ModificationNotes`.
3. After each submission `JudgeRepository` calls the private `TryCloseRoundAsync(researchId)` inside the **same transaction**. When all evaluations of the current round are submitted, apply the transition rules in Section 6 (all `موافق` → `معتمد`, otherwise → `تعديلات مطلوبة` with merged anonymous notes).
4. A judge cannot edit a submitted evaluation, and cannot submit for a round that is not current.

---

## 8. Certificate, Digital Signature & Archive

1. **Final approval = issuance.** `IssueEthicalCertificateAsync` (in `CollegeCoAdminRepository` **only**; there is no SuperAdmin version) works only on researches with status `معتمد`. It generates a unique visible `CertificateSerialNumber`, a verification URL/QR (`CertificateQrCodeUrl`), and the PDF through `ICertificateService`, stores `CertificateFilePath` and `CertificateIssuedAt`, and sets `صادر الشهادة`.
2. **Digital signature:** stamp the signature image (`SignatureImagePath`) of the issuing Co-Admin on the certificate. Block issuance with a clear Arabic message if the issuing Co-Admin has not uploaded a signature.
3. Email the PDF to every researcher in `ResearcherResearchesBridge` for the research and make it downloadable from the Researcher portal. Email failures are logged and **must not roll back** the issuance.
4. **Public verification:** `PublicController` shows whether a serial number is valid and minimal data only (research title, college, issue date). Never expose attachments or evaluations.
5. **Archive:** `SuperAdminRepository.SearchCertificatesArchiveAsync` searches by serial number, researcher, college, and date range.

---

## 9. Reports & Statistics

`SuperAdminRepository` (all colleges) and `CollegeCoAdminRepository` (own college only) provide: counts by status, college, and category; average days in screening and in judging; the percentage screened within `Screening:SlaHours`; certificates issued per period. Allow export to Excel and PDF.

---

## 10. Security & Data Isolation

1. **College isolation:** `[Authorize(Roles=...)]` is not enough. Every `CollegeCoAdminRepository` query filters by the current Co-Admin's `CollegeId`. Only `SuperAdmin` bypasses it.
2. **Ownership checks** in every repository method that receives an id from the client (`researchId`, `evaluationId`, `attachmentId`): verify the user is an author, the assigned judge, a same-college Co-Admin, or SuperAdmin.
3. **Attachments:** store **outside `wwwroot`**, validate extension/MIME and size, use server-generated file names, and serve only through an authorized action.
4. Anti-forgery on all POST actions; Identity lockout enabled; HTTPS enforced; secrets (SMTP, seed admin) only in configuration or user-secrets.
5. **Audit:** every status change is written to `ResearchStatusHistory`.

---

## 11. Seeding & Configuration

Seed at startup (`DbSeeder`, called from `Program.cs`):
* The four roles.
* One `SuperAdmin` user (credentials from configuration/user-secrets, never in source).

Configuration keys: `Auth:AllowedEmailDomains`, `Screening:SlaHours` (default 48), `Evaluation:MinJudgesPerResearch` (default 1), `Specialties` (list), SMTP settings, file-storage root path.
