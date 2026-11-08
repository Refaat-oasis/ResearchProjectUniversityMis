using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Repositories.Implementations;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using Xunit;

namespace researchProject.Tests
{
    public class WorkflowAndBusinessLogicTests
    {
        private ApplicationDbContext CreateInMemoryDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationDbContext(options);
        }

        private IConfiguration CreateTestConfig()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Auth:AllowedEmailDomains:0", "univ.edu.eg" },
                { "Auth:AllowedEmailDomains:1", "mis.edu.eg" },
                { "Screening:SlaHours", "48" },
                { "Evaluation:MinJudgesPerResearch", "1" },
                { "FileStorage:RootPath", "App_Data/UploadedFiles" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task ResearcherSignUp_AnyEmailDomain_AllowsRegistrationSuccessfully()
        {
            var db = CreateInMemoryDb("SignUp_AnyDomainAllowed");
            var config = CreateTestConfig();
            var emailSenderMock = new Mock<IEmailSender>();

            var college = new College { Name = "كلية العلوم", Code = "SCI" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("token-test-12345");

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null!, null!, null!, null!);

            var repo = new AccountRepository(
                userManagerMock.Object,
                signInManagerMock.Object,
                roleManagerMock.Object,
                db,
                config,
                emailSenderMock.Object);

            // Per Requirement 8: Registration is open to any email (e.g. gmail.com)
            var dto = new ResearcherSignUpDto
            {
                FullName = "د. باحث مصطفى",
                Email = "researcher@gmail.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ مساعد",
                Department = "الكيمياء",
                NationalId = "29012345678901",
                PhoneNumber = "01012345678"
            };

            var (succeeded, errorMessage, confirmationToken, userId) = await repo.SignUpResearcherAsync(dto);

            Assert.True(succeeded, errorMessage);
            Assert.NotNull(confirmationToken);
            Assert.Equal(6, confirmationToken!.Length);
            Assert.True(int.TryParse(confirmationToken, out _), "Confirmation code must be 6 numeric digits");
            var researcherInDb = await db.Researchers.FirstOrDefaultAsync(r => r.Department == "الكيمياء");
            Assert.NotNull(researcherInDb);
        }

        [Fact]
        public async Task SuggestJudges_StrictlyExcludesAuthorAndRanksBySpecialty()
        {
            var db = CreateInMemoryDb("SuggestJudges_ConflictOfInterest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var category = new ResearchCategory { Name = "أبحاث سريرية", CollegeId = college.Id };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var authorUser = new ApplicationUser { Id = "user-author", FullName = "الباحث د. أحمد", Email = "ahmed@univ.edu.eg" };
            var judge1User = new ApplicationUser { Id = "user-judge1", FullName = "المحكم د. محمود", Email = "judge1@univ.edu.eg" };
            var judge2User = new ApplicationUser { Id = "user-judge2", FullName = "المحكم د. مصطفى", Email = "judge2@univ.edu.eg" };
            db.Users.AddRange(authorUser, judge1User, judge2User);
            await db.SaveChangesAsync();

            var authorJudgeProfile = new Judge { UserId = authorUser.Id, Specialty = "طب سريري", AcademicTitle = "أستاذ", IsActive = true };
            var judge1Profile = new Judge { UserId = judge1User.Id, Specialty = "طب سريري", AcademicTitle = "أستاذ", IsActive = true };
            var judge2Profile = new Judge { UserId = judge2User.Id, Specialty = "علوم اجتماعية", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.AddRange(authorJudgeProfile, judge1Profile, judge2Profile);
            await db.SaveChangesAsync();

            var researcher = new Researcher
            {
                UserId = authorUser.Id,
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ",
                Department = "باطنة",
                NationalId = "29012345678901"
            };
            db.Researchers.Add(researcher);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "بحث سريري متقدم",
                CollegeId = college.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "طب سريري",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var bridge = new ResearcherResearchesBridge
            {
                ResearchId = research.Id,
                ResearcherId = researcher.Id,
                IsMainAuthor = true
            };
            db.ResearcherResearchesBridge.Add(bridge);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var suggestions = await repo.SuggestJudgesAsync(research.Id, college.Id);

            Assert.DoesNotContain(suggestions, j => j.UserId == authorUser.Id);
            Assert.Equal(2, suggestions.Count);
            Assert.Equal("طب سريري", suggestions[0].Specialty);
            Assert.Equal("علوم اجتماعية", suggestions[1].Specialty);
        }

        [Fact]
        public async Task AssignJudges_AuthorAsJudge_RejectsWithConflictOfInterestError()
        {
            var db = CreateInMemoryDb("AssignJudges_ConflictOfInterest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية الصيدلة", Code = "PHARM" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var category = new ResearchCategory { Name = "أبحاث دوائية", CollegeId = college.Id };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var authorUser = new ApplicationUser { Id = "author-id", FullName = "د. علي صيدلي", Email = "ali@univ.edu.eg" };
            db.Users.Add(authorUser);
            await db.SaveChangesAsync();

            var researcher = new Researcher
            {
                UserId = authorUser.Id,
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ",
                Department = "فارما",
                NationalId = "29012345678902"
            };
            db.Researchers.Add(researcher);

            var judgeProfile = new Judge
            {
                UserId = authorUser.Id,
                Specialty = "علم الأدوية",
                AcademicTitle = "أستاذ دكتور",
                IsActive = true
            };
            db.Judges.Add(judgeProfile);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "تأثير دواء س على الخلايا",
                CollegeId = college.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "علم الأدوية",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            db.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
            {
                ResearchId = research.Id,
                ResearcherId = researcher.Id,
                IsMainAuthor = true
            });
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var (succeeded, errorMessage) = await repo.AssignJudgesToResearchAsync(research.Id, college.Id, new List<int> { judgeProfile.Id }, "coadmin-id");

            Assert.False(succeeded);
            Assert.Contains("تضارب مصالح", errorMessage);
        }

        [Fact]
        public async Task JudgeEvaluation_AllJudgesApprove_TransitionsToApproved()
        {
            var db = CreateInMemoryDb("JudgeEvaluation_AllApprove");
            var emailMock = new Mock<IEmailSender>();
            var judgeRepo = new JudgeRepository(db, emailMock.Object);

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "دراسة أخلاقية مكتملة",
                CollegeId = college.Id,
                CategoryId = 1,
                AcademicSpecialty = "طب",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var judge1User = new ApplicationUser { Id = "judge1-user", FullName = "المحكم الأول" };
            var judge2User = new ApplicationUser { Id = "judge2-user", FullName = "المحكم الثاني" };
            db.Users.AddRange(judge1User, judge2User);
            await db.SaveChangesAsync();

            var j1 = new Judge { UserId = judge1User.Id, Specialty = "طب", AcademicTitle = "أستاذ", IsActive = true };
            var j2 = new Judge { UserId = judge2User.Id, Specialty = "طب", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.AddRange(j1, j2);
            await db.SaveChangesAsync();

            var eval1 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = j1.Id,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };
            var eval2 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = j2.Id,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };
            db.Evaluations.AddRange(eval1, eval2);
            await db.SaveChangesAsync();

            var dto1 = new EvaluationSubmitDto
            {
                EvaluationId = eval1.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            };
            var res1 = await judgeRepo.SubmitEvaluationAsync(dto1, judge1User.Id);
            Assert.True(res1.Succeeded, res1.ErrorMessage);

            // Per Requirement 11: Judge 1's submission closes the round and immediately approves research
            var rFinal = await db.Researches.FindAsync(research.Id);
            Assert.Equal(ResearchStatusConstants.Approved, rFinal?.WorkflowStatus);

            // Other judge's evaluation is marked as ClosedDueToOtherJudge
            var eval2Db = await db.Evaluations.FindAsync(eval2.Id);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, eval2Db?.Decision);

            // Subsequent submission by Judge 2 is rejected because round is closed
            var dto2 = new EvaluationSubmitDto
            {
                EvaluationId = eval2.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            };
            var res2 = await judgeRepo.SubmitEvaluationAsync(dto2, judge2User.Id);
            Assert.False(res2.Succeeded);
            Assert.Contains("تم إغلاق التحكيم لهذا المقترح", res2.ErrorMessage);
        }

        [Fact]
        public async Task JudgeEvaluation_OneJudgeRequiresModifications_TransitionsToModificationsRequired()
        {
            var db = CreateInMemoryDb("JudgeEvaluation_OneRequiresMod");
            var emailMock = new Mock<IEmailSender>();
            var judgeRepo = new JudgeRepository(db, emailMock.Object);

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "دراسة تحتاج تعديلات",
                CollegeId = college.Id,
                CategoryId = 1,
                AcademicSpecialty = "طب",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var judge1User = new ApplicationUser { Id = "judge1-user-mod", FullName = "المحكم الأول" };
            var judge2User = new ApplicationUser { Id = "judge2-user-mod", FullName = "المحكم الثاني" };
            db.Users.AddRange(judge1User, judge2User);
            await db.SaveChangesAsync();

            var j1 = new Judge { UserId = judge1User.Id, Specialty = "طب", AcademicTitle = "أستاذ", IsActive = true };
            var j2 = new Judge { UserId = judge2User.Id, Specialty = "طب", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.AddRange(j1, j2);
            await db.SaveChangesAsync();

            var eval1 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = j1.Id,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };
            var eval2 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = j2.Id,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };
            db.Evaluations.AddRange(eval1, eval2);
            await db.SaveChangesAsync();

            // Per Requirement 11: Judge 1 requires modifications -> closes round immediately
            var res1 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = eval1.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.NeedsModification,
                InformedConsentRating = EthicsRatingConstants.NeedsModification,
                Decision = EvaluationDecisionConstants.ModificationsRequired,
                ModificationNotes = "يرجى تعديل صياغة الإقرار المستنير وتحديد المخاطر بدقة."
            }, judge1User.Id);
            Assert.True(res1.Succeeded, res1.ErrorMessage);

            var rFinal = await db.Researches.FindAsync(research.Id);
            Assert.Equal(ResearchStatusConstants.ModificationsRequired, rFinal?.WorkflowStatus);

            var eval2Db = await db.Evaluations.FindAsync(eval2.Id);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, eval2Db?.Decision);

            // Judge 2 attempt to evaluate is rejected
            var res2 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = eval2.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            }, judge2User.Id);
            Assert.False(res2.Succeeded);
            Assert.Contains("تم إغلاق التحكيم لهذا المقترح", res2.ErrorMessage);
        }

        [Fact]
        public async Task IssueCertificate_WithoutSignature_SucceedsBecauseCoAdminSignatureNotRequired()
        {
            var db = CreateInMemoryDb("IssueCert_NoSignature");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية التمريض", Code = "NURSE" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var category = new ResearchCategory { Name = "أبحاث التمريض", CollegeId = college.Id };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var coAdminUser = new ApplicationUser { Id = "coadmin-nosig", FullName = "مدير الكلية" };
            db.Users.Add(coAdminUser);
            await db.SaveChangesAsync();

            // Per Requirement 4: CoAdmin does not need to upload signature image
            var coAdmin = new CollegeCoAdmin
            {
                UserId = coAdminUser.Id,
                CollegeId = college.Id,
                SignatureImagePath = null
            };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "بحث تمريض معتمد",
                CollegeId = college.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "تمريض",
                WorkflowStatus = ResearchStatusConstants.Approved,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            certMock.Setup(c => c.GenerateSerialNumber(It.IsAny<string>())).Returns("REC-NURSE-2026-0001");
            certMock.Setup(c => c.GenerateCertificateFileAsync(It.IsAny<Research>(), It.IsAny<CollegeCoAdmin>(), It.IsAny<string>())).ReturnsAsync("cert_nurse.pdf");

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var (succeeded, errorMessage, serialNumber) = await repo.IssueEthicalCertificateAsync(research.Id, coAdminUser.Id, "http://localhost");

            Assert.True(succeeded, errorMessage);
            Assert.Equal("REC-NURSE-2026-0001", serialNumber);
            var updatedResearch = await db.Researches.FindAsync(research.Id);
            Assert.Equal(ResearchStatusConstants.CertificateIssued, updatedResearch?.WorkflowStatus);
        }

        [Fact]
        public async Task IssueCertificate_OtherCollegeCoAdmin_RejectsWithCollegeIsolationError()
        {
            var db = CreateInMemoryDb("IssueCert_OtherCollege");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college1 = new College { Name = "كلية الطب", Code = "MED" };
            var college2 = new College { Name = "كلية الهندسة", Code = "ENG" };
            db.Colleges.AddRange(college1, college2);
            await db.SaveChangesAsync();

            var category = new ResearchCategory { Name = "أبحاث طبية", CollegeId = college1.Id };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var coAdminUser = new ApplicationUser { Id = "coadmin-eng", FullName = "مدير كلية الهندسة" };
            db.Users.Add(coAdminUser);
            await db.SaveChangesAsync();

            var coAdmin = new CollegeCoAdmin
            {
                UserId = coAdminUser.Id,
                CollegeId = college2.Id,
                SignatureImagePath = "uploads/sig.png"
            };
            db.CollegeCoAdmins.Add(coAdmin);

            var research = new Research
            {
                Title = "بحث طب بشري معتمد",
                CollegeId = college1.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "طب",
                WorkflowStatus = ResearchStatusConstants.Approved,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var (succeeded, errorMessage, _) = await repo.IssueEthicalCertificateAsync(research.Id, coAdminUser.Id, "http://localhost");

            Assert.False(succeeded);
            Assert.Contains("يتبع كلية أخرى", errorMessage);
        }

        [Fact]
        public async Task PublicVerifyCertificate_ValidSerial_ReturnsMinimalDataOnly()
        {
            var db = CreateInMemoryDb("PublicVerify_Valid");
            var college = new College { Name = "كلية الهندسة", Code = "ENG" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "تطبيقات الذكاء الاصطناعي في الطب",
                CollegeId = college.Id,
                CategoryId = 1,
                AcademicSpecialty = "ذكاء اصطناعي",
                WorkflowStatus = ResearchStatusConstants.CertificateIssued,
                CertificateSerialNumber = "REC-ENG-2026-999888",
                CertificateIssuedAt = DateTime.UtcNow,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var publicRepo = new PublicRepository(db);

            var result = await publicRepo.VerifyCertificateAsync("REC-ENG-2026-999888");

            Assert.True(result.IsValid);
            Assert.Equal("REC-ENG-2026-999888", result.SerialNumber);
            Assert.Equal("تطبيقات الذكاء الاصطناعي في الطب", result.ResearchTitle);
            Assert.Equal("كلية الهندسة", result.CollegeName);
        }

        [Fact]
        public async Task PublicVerifyCertificate_InvalidSerial_ReturnsInvalidWithoutDataLeak()
        {
            var db = CreateInMemoryDb("PublicVerify_Invalid");
            var publicRepo = new PublicRepository(db);

            var result = await publicRepo.VerifyCertificateAsync("REC-FAKE-0000");

            Assert.False(result.IsValid);
            Assert.Null(result.ResearchTitle);
            Assert.Null(result.CollegeName);
        }

        [Fact]
        public async Task Researcher_CreateResearch_ValidatesRequiredDynamicFields()
        {
            var db = CreateInMemoryDb("Researcher_DynamicFieldsValidation");
            var fileStorageMock = new Mock<IFileStorageService>();
            fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(("mock_file.pdf", 1024));

            var emailMock = new Mock<IEmailSender>();
            var config = CreateTestConfig();

            var college = new College { Name = "كلية العلوم", Code = "SCI" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var user = new ApplicationUser { Id = "researcher-user-1", FullName = "باحث علوم", Email = "sci@univ.edu.eg" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var researcher = new Researcher
            {
                UserId = user.Id,
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.Researcher,
                AcademicDegree = "دكتوراه",
                Department = "بيولوجي",
                NationalId = "29012345678903"
            };
            db.Researchers.Add(researcher);
            await db.SaveChangesAsync();

            // Category has a required dynamic field: "SampleSpecies"
            var category = new ResearchCategory
            {
                Name = "تجارب بيولوجية",
                CollegeId = college.Id,
                FieldsDefinitionJson = "[{\"Name\":\"SampleSpecies\",\"Label\":\"فصيلة العينة البيولوجية\",\"Type\":\"text\",\"IsRequired\":true}]"
            };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var repo = new ResearcherRepository(db, fileStorageMock.Object, emailMock.Object, config);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("proposal.pdf");
            fileMock.Setup(f => f.Length).Returns(1024);

            var dto = new ResearchCreateDto
            {
                Title = "دراسة بيولوجية تجريبية",
                CollegeId = college.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "بيولوجيا جزيئية",
                InformedConsentFile = fileMock.Object,
                ResearchToolsFile = fileMock.Object,
                ResearchProposalFile = fileMock.Object,
                DynamicAnswers = new Dictionary<string, string>() // Missing required SampleSpecies!
            };

            // 1. Missing required field -> fails
            var (succeeded1, error1, _) = await repo.CreateResearchWithAttachmentsAsync(dto, user.Id);
            Assert.False(succeeded1);
            Assert.Contains("إلزامي", error1);

            // 2. Providing required field -> succeeds
            dto.DynamicAnswers["SampleSpecies"] = "سلالة فئران تجارب سويسرية";
            var (succeeded2, error2, researchId) = await repo.CreateResearchWithAttachmentsAsync(dto, user.Id);
            Assert.True(succeeded2, error2);
            Assert.NotNull(researchId);

            var createdResearch = await db.Researches.FindAsync(researchId);
            Assert.Equal(ResearchStatusConstants.UnderFormalScreening, createdResearch?.WorkflowStatus);
            Assert.NotNull(createdResearch?.ScreeningDueAt);
            Assert.True(createdResearch?.ScreeningDueAt > createdResearch?.SubmittedAt);
        }

        [Fact]
        public async Task SuperAdmin_InviteJudge_StoresSha256HashNotRawToken()
        {
            var db = CreateInMemoryDb("SuperAdmin_InviteJudge_Hash");
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var emailMock = new Mock<IEmailSender>();

            var repo = new SuperAdminRepository(db, userManagerMock.Object, roleManagerMock.Object, emailMock.Object);

            var (succeeded, _, rawToken) = await repo.InviteJudgeAsync("judge.external@med.org", "طب أسنان", "admin-id");

            Assert.True(succeeded);
            Assert.NotNull(rawToken);

            var savedInvitation = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Email == "judge.external@med.org");
            Assert.NotNull(savedInvitation);

            // Raw token MUST NOT be stored in DB
            Assert.NotEqual(rawToken, savedInvitation.TokenHash);
            Assert.Equal(64, savedInvitation.TokenHash.Length); // 64 hex chars for SHA-256
            Assert.Equal(InvitationStatusConstants.Pending, savedInvitation.Status);
        }
    }
}
