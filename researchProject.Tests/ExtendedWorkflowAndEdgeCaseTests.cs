using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using researchProject.Controllers;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.DTOs;
using researchProject.Models.Entities;
using researchProject.Services;
using researchProject.Services.Implementations;
using researchProject.Services.Interfaces;
using researchProject.Repositories.Implementations;
using researchProject.Repositories.Interfaces;
using Xunit;

namespace researchProject.Tests
{
    public class ExtendedWorkflowAndEdgeCaseTests
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
        public async Task SuperAdmin_CollegeIsolationBypass_ReturnsAllCollegesQueue()
        {
            var db = CreateInMemoryDb("SuperAdmin_Bypass_ScreeningQueue");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college1 = new College { Name = "كلية الطب", Code = "MED" };
            var college2 = new College { Name = "كلية العلوم", Code = "SCI" };
            db.Colleges.AddRange(college1, college2);
            await db.SaveChangesAsync();

            var cat1 = new ResearchCategory { Name = "سريري", CollegeId = college1.Id };
            var cat2 = new ResearchCategory { Name = "طبيعي", CollegeId = college2.Id };
            db.ResearchCategories.AddRange(cat1, cat2);
            await db.SaveChangesAsync();

            var r1 = new Research
            {
                Title = "بحث كلية الطب",
                CollegeId = college1.Id,
                CategoryId = cat1.Id,
                AcademicSpecialty = "طب",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            var r2 = new Research
            {
                Title = "بحث كلية العلوم",
                CollegeId = college2.Id,
                CategoryId = cat2.Id,
                AcademicSpecialty = "علوم",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.AddRange(r1, r2);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            // College 1 Co-Admin sees only College 1
            var col1Queue = await repo.GetScreeningQueueAsync(college1.Id);
            Assert.Single(col1Queue);
            Assert.Equal("بحث كلية الطب", col1Queue[0].Title);

            // SuperAdmin (collegeId == 0) sees all colleges
            var superAdminQueue = await repo.GetScreeningQueueAsync(0);
            Assert.Equal(2, superAdminQueue.Count);
        }

        [Fact]
        public async Task DynamicKPIs_CalculatedAccuratelyFromHistoryEvents()
        {
            var db = CreateInMemoryDb("DynamicKPIs_Calculation");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية الصيدلة", Code = "PHARM" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var cat = new ResearchCategory { Name = "فارما", CollegeId = college.Id };
            db.ResearchCategories.Add(cat);
            await db.SaveChangesAsync();

            var now = DateTime.UtcNow;

            // Research 1: submitted 20 hours ago, screened 10 hours ago (took 10 hours -> compliant)
            var r1 = new Research
            {
                Title = "بحث ممتثل للمهلة",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "صيدلة",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = now.AddHours(-20),
                ScreeningDueAt = now.AddHours(28)
            };
            // Research 2: submitted 60 hours ago, screened 2 hours ago (took 58 hours -> non-compliant, exceeded 48h)
            var r2 = new Research
            {
                Title = "بحث متأخر عن المهلة",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "صيدلة",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = now.AddHours(-60),
                ScreeningDueAt = now.AddHours(-12)
            };
            db.Researches.AddRange(r1, r2);
            await db.SaveChangesAsync();

            // History records
            db.ResearchStatusHistories.AddRange(
                new ResearchStatusHistory
                {
                    ResearchId = r1.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedAt = now.AddHours(-10),
                    ChangedByUserId = "coadmin-1",
                    Reason = "إسناد للمحكمين"
                },
                new ResearchStatusHistory
                {
                    ResearchId = r2.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedAt = now.AddHours(-2),
                    ChangedByUserId = "coadmin-1",
                    Reason = "إسناد متأخر للمحكمين"
                }
            );
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);
            var stats = await repo.GetCollegeStatisticsAsync(college.Id);

            // Total screened = 2. Total hours = 10 + 58 = 68. Avg = 34.0 hours
            Assert.Equal(34.0, stats.AverageScreeningHours);
            // 1 out of 2 compliant = 50.0%
            Assert.Equal(50.0, stats.SlaCompliancePercentage);
        }

        [Fact]
        public async Task UploadSignatureFile_ValidAndInvalidExtensions_HandledCorrectly()
        {
            var db = CreateInMemoryDb("UploadSignature_Validation");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();
            var envMock = new Mock<IWebHostEnvironment>();
            var tempDir = Path.Combine(Path.GetTempPath(), "test_webroot_" + Guid.NewGuid().ToString("N"));
            envMock.Setup(e => e.WebRootPath).Returns(tempDir);
            envMock.Setup(e => e.ContentRootPath).Returns(tempDir);

            var user = new ApplicationUser { Id = "coadmin-user-sig", FullName = "مدير الكلية" };
            db.Users.Add(user);
            var coAdmin = new CollegeCoAdmin { UserId = user.Id, CollegeId = 1 };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object, envMock.Object);

            // 1. Invalid file extension (.txt) -> Rejected
            var txtFileMock = new Mock<IFormFile>();
            txtFileMock.Setup(f => f.FileName).Returns("signature.txt");
            txtFileMock.Setup(f => f.Length).Returns(500);

            var (succ1, err1) = await repo.UploadSignatureFileAsync(user.Id, txtFileMock.Object);
            Assert.False(succ1);
            Assert.Contains("صورة", err1);

            // 2. Valid file extension (.png) -> Accepted
            var pngFileMock = new Mock<IFormFile>();
            pngFileMock.Setup(f => f.FileName).Returns("my_signature.png");
            pngFileMock.Setup(f => f.Length).Returns(2048);
            pngFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns(Task.CompletedTask);

            var (succ2, err2) = await repo.UploadSignatureFileAsync(user.Id, pngFileMock.Object);
            Assert.True(succ2, err2);

            var updatedCoAdmin = await db.CollegeCoAdmins.FirstOrDefaultAsync(a => a.UserId == user.Id);
            Assert.NotNull(updatedCoAdmin?.SignatureImagePath);
            Assert.EndsWith(".png", updatedCoAdmin?.SignatureImagePath);

            // Cleanup temp dir if created
            if (Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        [Fact]
        public async Task ThreeJudges_RoundClosingOutcome_UnanimousApprovalTransitionsToApproved()
        {
            var db = CreateInMemoryDb("ThreeJudges_RoundClosing");
            var emailMock = new Mock<IEmailSender>();
            var judgeRepo = new JudgeRepository(db, emailMock.Object);

            var college = new College { Name = "كلية الهندسة", Code = "ENG" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "بحث تقني متعدد المحكمين",
                CollegeId = college.Id,
                CategoryId = 1,
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var u1 = new ApplicationUser { Id = "u1", FullName = "محكم 1" };
            var u2 = new ApplicationUser { Id = "u2", FullName = "محكم 2" };
            var u3 = new ApplicationUser { Id = "u3", FullName = "محكم 3" };
            db.Users.AddRange(u1, u2, u3);
            await db.SaveChangesAsync();

            var j1 = new Judge { UserId = u1.Id, Specialty = "هندسة", AcademicTitle = "أستاذ", IsActive = true };
            var j2 = new Judge { UserId = u2.Id, Specialty = "هندسة", AcademicTitle = "أستاذ", IsActive = true };
            var j3 = new Judge { UserId = u3.Id, Specialty = "هندسة", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.AddRange(j1, j2, j3);
            await db.SaveChangesAsync();

            var e1 = new Evaluation { ResearchId = research.Id, JudgeId = j1.Id, RoundNumber = 1, Decision = EvaluationDecisionConstants.UnderEvaluation };
            var e2 = new Evaluation { ResearchId = research.Id, JudgeId = j2.Id, RoundNumber = 1, Decision = EvaluationDecisionConstants.UnderEvaluation };
            var e3 = new Evaluation { ResearchId = research.Id, JudgeId = j3.Id, RoundNumber = 1, Decision = EvaluationDecisionConstants.UnderEvaluation };
            db.Evaluations.AddRange(e1, e2, e3);
            await db.SaveChangesAsync();

            // Judge 1 submits Approved
            var res1 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = e1.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            }, u1.Id);
            Assert.True(res1.Succeeded, res1.ErrorMessage);

            // Per Requirement 11: First judge to submit evaluation closes round and immediately approves research
            var mid1 = await db.Researches.FindAsync(research.Id);
            Assert.Equal(ResearchStatusConstants.Approved, mid1?.WorkflowStatus);

            var e2Db = await db.Evaluations.FindAsync(e2.Id);
            var e3Db = await db.Evaluations.FindAsync(e3.Id);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, e2Db?.Decision);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, e3Db?.Decision);

            // Judge 2 attempt to submit is rejected because round is closed
            var res2 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = e2.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            }, u2.Id);
            Assert.False(res2.Succeeded);
            Assert.Contains("تم إغلاق التحكيم لهذا المقترح", res2.ErrorMessage);

            // Judge 3 attempt to submit is also rejected
            var res3 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = e3.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            }, u3.Id);
            Assert.False(res3.Succeeded);
            Assert.Contains("تم إغلاق التحكيم لهذا المقترح", res3.ErrorMessage);

            var finalRes = await db.Researches.FindAsync(research.Id);
            Assert.Equal(ResearchStatusConstants.Approved, finalRes?.WorkflowStatus);

            var history = await db.ResearchStatusHistories.FirstOrDefaultAsync(h => h.ResearchId == research.Id && h.NewStatus == ResearchStatusConstants.Approved);
            Assert.NotNull(history);
            Assert.Contains("إغلاق التحكيم لبقية المحكمين", history.Reason);
        }

        [Fact]
        public async Task ExportReportsToCsv_ReturnsUtf8CsvWithArabicHeadings()
        {
            var db = CreateInMemoryDb("ExportReports_CsvTest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var user = new ApplicationUser { Id = "coadmin-csv", FullName = "مدير الكلية" };
            db.Users.Add(user);
            var coAdmin = new CollegeCoAdmin { UserId = user.Id, CollegeId = college.Id };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var r1 = new Research
            {
                Title = "بحث تجريبي",
                CollegeId = college.Id,
                CategoryId = 1,
                WorkflowStatus = ResearchStatusConstants.Approved,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(r1);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);
            var hangfireMock = new Mock<IHangfireEmailDispatcher>();
            var controller = new CollegeCoAdminController(repo, emailMock.Object, hangfireMock.Object);

            // Mock User ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, RoleConstants.CollegeCoAdmin)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var actionResult = await controller.ExportReportsToCsv();
            var fileResult = Assert.IsType<FileContentResult>(actionResult);

            Assert.Equal("text/csv; charset=utf-8", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);

            var csvContent = Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("المؤشر,القيمة", csvContent);
            Assert.Contains("إجمالي المقترحات", csvContent);
            Assert.Contains("معتمد", csvContent);
        }

        [Fact]
        public async Task ThinController_ResearcherDownloadCertificate_DelegatesWithoutDirectFileIo()
        {
            var repoMock = new Mock<IResearcherRepository>();
            var certBytes = Encoding.UTF8.GetBytes("<html><body>شهادة اختبار</body></html>");
            repoMock.Setup(r => r.GetCertificateDownloadAsync(123, "researcher-id"))
                .ReturnsAsync((certBytes, "REC-MED-2026-111222"));

            var controller = new ResearcherController(repoMock.Object);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "researcher-id") };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
            };

            var actionResult = await controller.DownloadCertificate(123);
            var fileResult = Assert.IsType<FileContentResult>(actionResult);

            Assert.Equal("text/html", fileResult.ContentType);
            Assert.Equal("Certificate_REC-MED-2026-111222.html", fileResult.FileDownloadName);
            Assert.Equal(certBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ReassignJudges_WhenAssignedToJudges_Succeeds()
        {
            var db = CreateInMemoryDb("ReassignJudges_AssignedStatus");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية التمريض", Code = "NURSE" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var cat = new ResearchCategory { Name = "تمريض عام", CollegeId = college.Id };
            db.ResearchCategories.Add(cat);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "بحث تمريضي لإعادة الإسناد",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "تمريض",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var existingJudgeUser = new ApplicationUser { Id = "existing-j-u", FullName = "محكم أول" };
            db.Users.Add(existingJudgeUser);
            var existingJudge = new Judge { UserId = existingJudgeUser.Id, Specialty = "تمريض", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.Add(existingJudge);
            await db.SaveChangesAsync();

            db.Evaluations.Add(new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = existingJudge.Id,
                RoundNumber = 1,
                Decision = "قيد التقييم"
            });
            await db.SaveChangesAsync();

            var u = new ApplicationUser { Id = "judge-reassign-u", FullName = "محكم جديد" };
            db.Users.Add(u);
            var j = new Judge { UserId = u.Id, Specialty = "تمريض", AcademicTitle = "أستاذ مساعد", IsActive = true };
            db.Judges.Add(j);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var (succ, err) = await repo.AssignJudgesToResearchAsync(research.Id, college.Id, new List<int> { j.Id }, "coadmin-id");
            Assert.True(succ, err);

            var savedEval = await db.Evaluations.FirstOrDefaultAsync(e => e.ResearchId == research.Id && e.JudgeId == j.Id);
            Assert.NotNull(savedEval);
            Assert.Equal(1, savedEval!.RoundNumber); // Asserts BUG-04 fix: stays in round 1, does not corrupt round
        }

        [Fact]
        public async Task FullWorkflowCycle_MultiRounds_ModificationsToIssuance()
        {
            var db = CreateInMemoryDb("FullWorkflowCycle_MultiRounds");
            var emailMock = new Mock<IEmailSender>();
            var certServiceMock = new Mock<ICertificateService>();
            certServiceMock.Setup(c => c.GenerateSerialNumber("MED")).Returns("REC-MED-2026-777888");
            certServiceMock.Setup(c => c.GenerateCertificateFileAsync(It.IsAny<Research>(), It.IsAny<CollegeCoAdmin>(), It.IsAny<string>()))
                .ReturnsAsync("Cert_REC-MED-2026-777888.html");

            var fileStorageMock = new Mock<IFileStorageService>();
            fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(("stored_file.pdf", 2048, "uploads/attachments/stored_file.pdf"));

            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();
            var config = CreateTestConfig();

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var cat = new ResearchCategory { Name = "سريري", CollegeId = college.Id };
            db.ResearchCategories.Add(cat);
            await db.SaveChangesAsync();

            // 1. Researcher submits proposal
            var researcherUser = new ApplicationUser { Id = "res-user", FullName = "الباحث الأساسي", Email = "researcher@univ.edu.eg" };
            db.Users.Add(researcherUser);
            var researcherProfile = new Researcher
            {
                UserId = researcherUser.Id,
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ مساعد",
                Department = "جراحة",
                NationalId = "29012345678909"
            };
            db.Researchers.Add(researcherProfile);
            await db.SaveChangesAsync();

            var researcherRepo = new ResearcherRepository(db, fileStorageMock.Object, emailMock.Object, config, certServiceMock.Object);

            var dummyFile = new Mock<IFormFile>();
            dummyFile.Setup(f => f.FileName).Returns("proposal.pdf");
            dummyFile.Setup(f => f.Length).Returns(1024);

            var (createSucc, _, researchId) = await researcherRepo.CreateResearchWithAttachmentsAsync(new ResearchCreateDto
            {
                Title = "بحث جراحة سريرية متقدم",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "جراحة عامة",
                InformedConsentFile = dummyFile.Object,
                ResearchToolsFile = dummyFile.Object,
                ResearchProposalFile = dummyFile.Object
            }, researcherUser.Id);

            Assert.True(createSucc);
            Assert.NotNull(researchId);

            var r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.UnderFormalScreening, r?.WorkflowStatus);

            // 2. Co-Admin assigns two judges for Round 1
            var coAdminUser = new ApplicationUser { Id = "coadmin-user", FullName = "مدير الكلية", Email = "coadmin@univ.edu.eg" };
            var j1User = new ApplicationUser { Id = "j1-user", FullName = "المحكم الأول", Email = "j1@univ.edu.eg" };
            var j2User = new ApplicationUser { Id = "j2-user", FullName = "المحكم الثاني", Email = "j2@univ.edu.eg" };
            db.Users.AddRange(coAdminUser, j1User, j2User);

            var coAdminProfile = new CollegeCoAdmin
            {
                UserId = coAdminUser.Id,
                CollegeId = college.Id,
                SignatureImagePath = "~/uploads/signatures/sig.png"
            };
            db.CollegeCoAdmins.Add(coAdminProfile);

            var j1 = new Judge { UserId = j1User.Id, Specialty = "جراحة عامة", AcademicTitle = "أستاذ", IsActive = true };
            var j2 = new Judge { UserId = j2User.Id, Specialty = "جراحة عامة", AcademicTitle = "أستاذ", IsActive = true };
            db.Judges.AddRange(j1, j2);
            await db.SaveChangesAsync();

            var coAdminRepo = new CollegeCoAdminRepository(db, certServiceMock.Object, emailMock.Object, config, loggerMock.Object);
            var (assignSucc, assignErr) = await coAdminRepo.AssignJudgesToResearchAsync(researchId.Value, college.Id, new List<int> { j1.Id, j2.Id }, coAdminUser.Id);
            Assert.True(assignSucc, assignErr);

            r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.AssignedToJudges, r?.WorkflowStatus);

            // 3. Round 1 Reviews: Judge 2 requests modifications -> immediately transitions to ModificationsRequired and closes round
            var judgeRepo = new JudgeRepository(db, emailMock.Object);
            var round1Evals = await db.Evaluations.Where(e => e.ResearchId == researchId.Value && e.RoundNumber == 1).ToListAsync();
            var e1 = round1Evals.First(e => e.JudgeId == j1.Id);
            var e2 = round1Evals.First(e => e.JudgeId == j2.Id);

            var resR1 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = e2.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.NeedsModification,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.ModificationsRequired,
                ModificationNotes = "يرجى توضيح منهجية التعقيم وحماية المشاركين."
            }, j2User.Id);
            Assert.True(resR1.Succeeded, resR1.ErrorMessage);

            r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.ModificationsRequired, r?.WorkflowStatus);

            var e1Db = await db.Evaluations.FindAsync(e1.Id);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, e1Db?.Decision);

            // 4. Researcher submits modified attachments -> Starts Round 2 & returns to AssignedToJudges
            var modProposalFile = new Mock<IFormFile>();
            modProposalFile.Setup(f => f.FileName).Returns("proposal_v2.pdf");
            modProposalFile.Setup(f => f.Length).Returns(2048);

            var (modSucc, modErr) = await researcherRepo.SubmitModifiedAttachmentsAsync(new ResearchModificationDto
            {
                ResearchId = researchId.Value,
                ModifiedProposalFile = modProposalFile.Object,
                ResearcherNotes = "تم تعديل منهجية التعقيم وفق ملاحظات اللجنة."
            }, researcherUser.Id);
            Assert.True(modSucc, modErr);

            r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.AssignedToJudges, r?.WorkflowStatus);

            // 5. Round 2: Judge 1 Approves -> immediately transitions to Approved and closes round
            var round2Evals = await db.Evaluations.Where(e => e.ResearchId == researchId.Value && e.RoundNumber == 2).ToListAsync();
            Assert.Equal(2, round2Evals.Count);
            var e1_r2 = round2Evals.First(e => e.JudgeId == j1.Id);
            var e2_r2 = round2Evals.First(e => e.JudgeId == j2.Id);

            var resR2 = await judgeRepo.SubmitEvaluationAsync(new EvaluationSubmitDto
            {
                EvaluationId = e1_r2.Id,
                PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                RiskLevel = EthicsRatingConstants.Appropriate,
                InformedConsentRating = EthicsRatingConstants.Appropriate,
                Decision = EvaluationDecisionConstants.Approved
            }, j1User.Id);
            Assert.True(resR2.Succeeded, resR2.ErrorMessage);

            r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.Approved, r?.WorkflowStatus);

            var e2_r2Db = await db.Evaluations.FindAsync(e2_r2.Id);
            Assert.Equal(EvaluationDecisionConstants.ClosedDueToOtherJudge, e2_r2Db?.Decision);

            // 6. Co-Admin Issues Certificate -> Transitions to CertificateIssued
            var (issueSucc, issueErr, serialNum) = await coAdminRepo.IssueEthicalCertificateAsync(researchId.Value, coAdminUser.Id, "http://localhost");
            Assert.True(issueSucc, issueErr);
            Assert.Equal("REC-MED-2026-777888", serialNum);

            r = await db.Researches.FindAsync(researchId.Value);
            Assert.Equal(ResearchStatusConstants.CertificateIssued, r?.WorkflowStatus);
            Assert.Equal("REC-MED-2026-777888", r?.CertificateSerialNumber);
            Assert.NotNull(r?.CertificateIssuedAt);

            // Verify full audit history: 6 status transitions recorded
            var histories = await db.ResearchStatusHistories.Where(h => h.ResearchId == researchId.Value).OrderBy(h => h.Id).ToListAsync();
            Assert.Equal(6, histories.Count);
            Assert.Equal(ResearchStatusConstants.UnderFormalScreening, histories[0].NewStatus);
            Assert.Equal(ResearchStatusConstants.AssignedToJudges, histories[1].NewStatus);
            Assert.Equal(ResearchStatusConstants.ModificationsRequired, histories[2].NewStatus);
            Assert.Equal(ResearchStatusConstants.AssignedToJudges, histories[3].NewStatus);
            Assert.Equal(ResearchStatusConstants.Approved, histories[4].NewStatus);
            Assert.Equal(ResearchStatusConstants.CertificateIssued, histories[5].NewStatus);
        }

        [Fact]
        public async Task SuperAdmin_ExportReportsToCsv_ReturnsUtf8CsvWithCollegesAndCategories()
        {
            var db = CreateInMemoryDb("SuperAdmin_CsvTest");
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null!, null!, null!, null!);
            var emailMock = new Mock<IEmailSender>();

            var college = new College { Name = "كلية طب الأسنان", Code = "DENT" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var cat = new ResearchCategory { Name = "علاج جذور", CollegeId = college.Id };
            db.ResearchCategories.Add(cat);
            await db.SaveChangesAsync();

            var r = new Research
            {
                Title = "بحث طب أسنان متقدم",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "طب أسنان",
                WorkflowStatus = ResearchStatusConstants.Approved,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(r);
            await db.SaveChangesAsync();

            var repo = new SuperAdminRepository(db, userManagerMock.Object, roleManagerMock.Object, emailMock.Object);
            var hangfireMock = new Mock<IHangfireEmailDispatcher>();
            var controller = new SuperAdminController(repo, emailMock.Object, hangfireMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-id"),
                new Claim(ClaimTypes.Role, RoleConstants.SuperAdmin)
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
            };

            var actionResult = await controller.ExportReportsToCsv();
            var fileResult = Assert.IsType<FileContentResult>(actionResult);

            Assert.Equal("text/csv; charset=utf-8", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);

            var content = Encoding.UTF8.GetString(fileResult.FileContents);
            Assert.Contains("إجمالي المقترحات بالجامعة", content);
            Assert.Contains("كلية طب الأسنان", content);
            Assert.Contains("علاج جذور", content);
        }

        [Fact]
        public async Task AttachmentDownload_FromDatabase_ReturnsStreamAndMatchesBytes()
        {
            var db = CreateInMemoryDb("AttachmentDownload_DbTest");
            var fileStorageMock = new Mock<IFileStorageService>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var authorUser = new ApplicationUser { Id = "author-usr", FullName = "باحث", Email = "author@univ.edu.eg" };
            db.Users.Add(authorUser);
            await db.SaveChangesAsync();

            var researcher = new Researcher
            {
                UserId = authorUser.Id,
                CollegeId = college.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ",
                Department = "جراحة",
                NationalId = "29012345678901"
            };
            db.Researchers.Add(researcher);

            var research = new Research
            {
                Title = "بحث اختبار تنزيل المرفق",
                CollegeId = college.Id,
                AcademicSpecialty = "جراحة",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow
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

            var expectedBytes = Encoding.UTF8.GetBytes("محتوى ملف الموافقة المستنيرة المخزن بقاعدة البيانات");
            var attachment = new ResearchAttachment
            {
                ResearchId = research.Id,
                AttachmentType = AttachmentTypeConstants.InformedConsent,
                Version = 1,
                OriginalFileName = "consent.pdf",
                StoredFileName = "consent_db_test.pdf",
                FilePath = "uploads/attachments/Research_88/consent_db_test.pdf",
                ContentType = "application/pdf",
                FileSize = expectedBytes.Length,
                UploadedAt = DateTime.UtcNow
            };
            db.ResearchAttachments.Add(attachment);
            await db.SaveChangesAsync();

            fileStorageMock.Setup(f => f.GetFileStreamAsync("consent_db_test.pdf", $"Research_{research.Id}"))
                .ReturnsAsync(new MemoryStream(expectedBytes));

            userManagerMock.Setup(m => m.FindByIdAsync(authorUser.Id)).ReturnsAsync(authorUser);
            userManagerMock.Setup(m => m.IsInRoleAsync(authorUser, RoleConstants.Researcher)).ReturnsAsync(true);
            userManagerMock.Setup(m => m.IsInRoleAsync(authorUser, RoleConstants.SuperAdmin)).ReturnsAsync(false);
            userManagerMock.Setup(m => m.IsInRoleAsync(authorUser, RoleConstants.CollegeCoAdmin)).ReturnsAsync(false);
            userManagerMock.Setup(m => m.IsInRoleAsync(authorUser, RoleConstants.Judge)).ReturnsAsync(false);

            var attService = new AttachmentService(db, fileStorageMock.Object, userManagerMock.Object);

            var (isAuthorized, fileExists, fileStream, contentType, fileName) = await attService.GetAttachmentDownloadAsync(attachment.Id, authorUser.Id);

            Assert.True(isAuthorized);
            Assert.True(fileExists);
            Assert.NotNull(fileStream);
            Assert.Equal("application/pdf", contentType);
            Assert.Equal("consent.pdf", fileName);

            using var ms = new MemoryStream();
            await fileStream!.CopyToAsync(ms);
            var downloadedBytes = ms.ToArray();
            Assert.Equal(expectedBytes, downloadedBytes);
        }

        [Fact]
        public async Task SuggestJudges_ExcludesJudgesFromOtherColleges()
        {
            var db = CreateInMemoryDb("SuggestJudges_CollegeIsolationTest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var collegeMed = new College { Name = "كلية الطب", Code = "MED" };
            var collegePharm = new College { Name = "كلية الصيدلة", Code = "PHARM" };
            db.Colleges.AddRange(collegeMed, collegePharm);
            await db.SaveChangesAsync();

            var uMed = new ApplicationUser { Id = "u-med-judge", FullName = "محكم طب", Email = "jmed@univ.edu.eg" };
            var uPharm = new ApplicationUser { Id = "u-pharm-judge", FullName = "محكم صيدلة", Email = "jpharm@univ.edu.eg" };
            var uAuthor = new ApplicationUser { Id = "u-med-author", FullName = "باحث طب", Email = "authormed@univ.edu.eg" };
            db.Users.AddRange(uMed, uPharm, uAuthor);
            await db.SaveChangesAsync();

            var jMed = new Judge { UserId = uMed.Id, Specialty = "علم الأدوية", AcademicTitle = "أستاذ", CollegeId = collegeMed.Id, IsActive = true };
            var jPharm = new Judge { UserId = uPharm.Id, Specialty = "علم الأدوية", AcademicTitle = "أستاذ", CollegeId = collegePharm.Id, IsActive = true };
            db.Judges.AddRange(jMed, jPharm);

            var researcher = new Researcher
            {
                UserId = uAuthor.Id,
                CollegeId = collegeMed.Id,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ",
                Department = "فارماكولوجي",
                NationalId = "29012345678902"
            };
            db.Researchers.Add(researcher);

            var research = new Research
            {
                Title = "بحث تجارب الأدوية بكلية الطب",
                CollegeId = collegeMed.Id,
                AcademicSpecialty = "علم الأدوية",
                WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                SubmittedAt = DateTime.UtcNow
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

            var suggestions = await repo.SuggestJudgesAsync(research.Id, collegeMed.Id);

            // Must contain ONLY the judge from CollegeMed, and strictly EXCLUDE CollegePharm
            Assert.Single(suggestions);
            Assert.Equal(jMed.Id, suggestions[0].Id);
            Assert.DoesNotContain(suggestions, j => j.Id == jPharm.Id);
        }

        [Fact]
        public async Task InviteJudge_BindsInvitationToCoAdminCollege()
        {
            var db = CreateInMemoryDb("InviteJudge_CollegeBindingTest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var collegeDent = new College { Name = "كلية طب الأسنان", Code = "DENT" };
            db.Colleges.Add(collegeDent);
            await db.SaveChangesAsync();

            var coAdminUser = new ApplicationUser { Id = "coadmin-dent", FullName = "مدير أسنان", Email = "coadmin.dent@univ.edu.eg" };
            db.Users.Add(coAdminUser);
            await db.SaveChangesAsync();

            var coAdmin = new CollegeCoAdmin
            {
                UserId = coAdminUser.Id,
                CollegeId = collegeDent.Id,
                IsActive = true
            };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            var (succeeded, errorMessage, rawToken, _) = await repo.InviteJudgeAsync("outside.judge@external.edu", "جراحة الفم", coAdminUser.Id);

            Assert.True(succeeded);
            Assert.NotNull(rawToken);

            var invitation = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Email == "outside.judge@external.edu");
            Assert.NotNull(invitation);
            Assert.Equal(collegeDent.Id, invitation.CollegeId);
            Assert.Equal("outside.judge@external.edu", invitation.Email);
        }

        [Fact]
        public async Task CreateResearch_RejectsEmpty0ByteFileWithFriendlyError()
        {
            var db = CreateInMemoryDb("CreateResearch_EmptyFileTest");
            var fileStorageMock = new Mock<IFileStorageService>();
            var emailMock = new Mock<IEmailSender>();
            var certMock = new Mock<ICertificateService>();

            var college = new College { Name = "كلية العلوم", Code = "SCI" };
            var cat = new ResearchCategory { Name = "أبحاث تجريبية", College = college };
            db.Colleges.Add(college);
            db.ResearchCategories.Add(cat);
            var user = new ApplicationUser { Id = "researcher-user-empty", FullName = "منى بكير", Email = "mona@univ.edu.eg" };
            db.Users.Add(user);
            var researcher = new Researcher
            {
                UserId = user.Id,
                College = college,
                ResearcherType = ResearcherTypeConstants.FacultyMember,
                AcademicDegree = "أستاذ مساعد",
                Department = "كيمياء",
                NationalId = "29801011234567"
            };
            db.Researchers.Add(researcher);
            await db.SaveChangesAsync();

            var repo = new ResearcherRepository(db, fileStorageMock.Object, emailMock.Object, CreateTestConfig(), certMock.Object);

            var validFile = new Mock<IFormFile>();
            validFile.Setup(f => f.FileName).Returns("valid.pdf");
            validFile.Setup(f => f.Length).Returns(1024);

            var emptyFile = new Mock<IFormFile>();
            emptyFile.Setup(f => f.FileName).Returns("empty.docx");
            emptyFile.Setup(f => f.Length).Returns(0); // 0-byte file!

            var dto = new ResearchCreateDto
            {
                Title = "بحث تجريبي",
                CollegeId = college.Id,
                CategoryId = cat.Id,
                AcademicSpecialty = "كيمياء حيوية",
                InformedConsentFile = validFile.Object,
                ResearchToolsFile = emptyFile.Object, // Empty!
                ResearchProposalFile = validFile.Object
            };

            var (succeeded, errorMsg, researchId) = await repo.CreateResearchWithAttachmentsAsync(dto, user.Id);

            Assert.False(succeeded);
            Assert.Null(researchId);
            Assert.Contains("أدوات وأجهزة البحث", errorMsg);
            Assert.Contains("فارغ", errorMsg);
        }

        [Fact]
        public async Task InviteJudge_ExternalEmailNormalizationAndDuplicateSuperseding()
        {
            var db = CreateInMemoryDb("InviteJudge_NormalizationTest");
            var certMock = new Mock<ICertificateService>();
            var emailMock = new Mock<IEmailSender>();
            var loggerMock = new Mock<ILogger<CollegeCoAdminRepository>>();

            var college = new College { Name = "كلية الصيدلة", Code = "PHARM" };
            db.Colleges.Add(college);
            var user = new ApplicationUser { Id = "admin-pharm", FullName = "أحمد رفعت", Email = "ahmed.refaat@univ.edu.eg" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var coAdmin = new CollegeCoAdmin { UserId = user.Id, CollegeId = college.Id, IsActive = true };
            db.CollegeCoAdmins.Add(coAdmin);
            await db.SaveChangesAsync();

            var repo = new CollegeCoAdminRepository(db, certMock.Object, emailMock.Object, CreateTestConfig(), loggerMock.Object);

            // First invitation with uppercase spaces
            var (succ1, _, rawToken1, _) = await repo.InviteJudgeAsync("  AntigravityUser45@Gmail.Com  ", "علوم انسان", user.Id);
            Assert.True(succ1);

            var inv1 = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Email == "antigravityuser45@gmail.com");
            Assert.NotNull(inv1);
            Assert.Equal(InvitationStatusConstants.Pending, inv1.Status);
            Assert.Equal(college.Id, inv1.CollegeId);

            // Second invitation to same email supersedes the old one
            var (succ2, _, rawToken2, _) = await repo.InviteJudgeAsync("antigravityuser45@gmail.com", "علوم انسان", user.Id);
            Assert.True(succ2);

            var oldInv = await db.JudgeInvitations.FindAsync(inv1.Id);
            Assert.Equal("Superseded", oldInv?.Status);

            var newInv = await db.JudgeInvitations.FirstOrDefaultAsync(i => i.Email == "antigravityuser45@gmail.com" && i.Status == InvitationStatusConstants.Pending);
            Assert.NotNull(newInv);
            Assert.NotEqual(inv1.Id, newInv.Id);
        }

        [Fact]
        public async Task EmailSender_ArchivesLocally_WhenCredentialsSimulatedOrUnconfigured()
        {
            var loggerMock = new Mock<ILogger<EmailSender>>();
            var tempDir = Path.Combine(Path.GetTempPath(), "test_mail_" + Guid.NewGuid().ToString("N"));
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.ContentRootPath).Returns(tempDir);

            var inMemoryConfig = new Dictionary<string, string?>
            {
                { "Smtp:Host", "smtp.gmail.com" },
                { "Smtp:Port", "587" },
                { "Smtp:User", "YOUR_GMAIL_ADDRESS@gmail.com" },
                { "Smtp:Password", "YOUR_GMAIL_APP_PASSWORD" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

            var sender = new EmailSender(loggerMock.Object, config, envMock.Object);

            var testEmail = "antigravityuser45@gmail.com";
            var testSubj = "دعوة تحكيم اختبار";
            var testBody = "<p>رابط الدعوة التجريبي: https://localhost:7274/test</p>";

            await sender.SendEmailAsync(testEmail, testSubj, testBody);

            var sentDir = Path.Combine(tempDir, "App_Data", "SentEmails");
            Assert.True(Directory.Exists(sentDir));

            var files = Directory.GetFiles(sentDir, "*.html");
            Assert.Single(files);

            var fileContent = await File.ReadAllTextAsync(files[0]);
            Assert.Contains(testEmail, fileContent);
            Assert.Contains("رابط الدعوة التجريبي", fileContent);
            Assert.Contains("محاكاة محلية", fileContent);

            // Cleanup
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}