using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using researchProject.Models.Constants;
using researchProject.Models.Entities;

namespace researchProject.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and migrations applied
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Seed Roles
            string[] roles = 
            { 
                RoleConstants.SuperAdmin, 
                RoleConstants.CollegeCoAdmin, 
                RoleConstants.Judge, 
                RoleConstants.Researcher 
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Helper to create or ensure users
            async Task<ApplicationUser> EnsureUserAsync(string email, string fullName, string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true
                    };
                    var res = await userManager.CreateAsync(user, password);
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
                else
                {
                    if (!await userManager.IsInRoleAsync(user, role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
                return user;
            }

            string defaultPassword = configuration["AdminUser:DefaultPassword"] ?? "Mis@2026Password!";

            // Seed SuperAdmin
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@mis.edu.eg";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin@Mis2026Password!";
            var superAdminUser = await EnsureUserAsync(adminEmail, "أ.د. رفعت عبد الحميد - مدير المنصة الجامعية", adminPassword, RoleConstants.SuperAdmin);

            // Seed College Co-Admins
            var coAdminMedUser = await EnsureUserAsync("coadmin.med@mis.edu.eg", "د. منى الشريف - منسق أخلاقيات الطب", defaultPassword, RoleConstants.CollegeCoAdmin);
            var coAdminPharmUser = await EnsureUserAsync("coadmin.pharm@mis.edu.eg", "د. خالد النجار - منسق أخلاقيات الصيدلة", defaultPassword, RoleConstants.CollegeCoAdmin);
            var coAdminSciUser = await EnsureUserAsync("coadmin.sci@mis.edu.eg", "د. سمير الباز - منسق أخلاقيات العلوم", defaultPassword, RoleConstants.CollegeCoAdmin);

            // Seed Judges
            var judgeAhmedUser = await EnsureUserAsync("judge.ahmed@mis.edu.eg", "أ.د. أحمد فاروق رضوان", defaultPassword, RoleConstants.Judge);
            var judgeSarahUser = await EnsureUserAsync("judge.sarah@mis.edu.eg", "أ.د. سارة المنشاوي", defaultPassword, RoleConstants.Judge);
            var judgeMahmoudUser = await EnsureUserAsync("judge.mahmoud@mis.edu.eg", "أ.د. محمود يوسف غنيم", defaultPassword, RoleConstants.Judge);
            var judgeHodaUser = await EnsureUserAsync("judge.hoda@mis.edu.eg", "د. هدى عبد الرازق إبراهيم", defaultPassword, RoleConstants.Judge);

            // Seed Researchers
            var researcherTarekUser = await EnsureUserAsync("researcher.tarek@mis.edu.eg", "د. طارق الحسين علي", defaultPassword, RoleConstants.Researcher);
            var researcherFatimaUser = await EnsureUserAsync("researcher.fatima@mis.edu.eg", "د. فاطمة الزهراء كمال", defaultPassword, RoleConstants.Researcher);
            var researcherAmrUser = await EnsureUserAsync("researcher.amr@mis.edu.eg", "الباحث عمرو الشناوي", defaultPassword, RoleConstants.Researcher);

            // 3. Seed Colleges
            if (!await context.Colleges.AnyAsync())
            {
                var colleges = new List<College>
                {
                    new College { Name = "كلية الطب البشري", Code = "MED" },
                    new College { Name = "كلية الصيدلة", Code = "PHARM" },
                    new College { Name = "كلية التمريض", Code = "NURSE" },
                    new College { Name = "كلية العلوم", Code = "SCI" },
                    new College { Name = "كلية الهندسة والحاسبات", Code = "ENG" }
                };
                await context.Colleges.AddRangeAsync(colleges);
                await context.SaveChangesAsync();
            }

            var medCol = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "MED") ?? await context.Colleges.FirstAsync();
            var pharmCol = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "PHARM") ?? medCol;
            var nurseCol = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "NURSE") ?? medCol;
            var sciCol = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "SCI") ?? medCol;

            // 4. Seed Research Categories
            if (!await context.ResearchCategories.AnyAsync(c => c.CollegeId == medCol.Id))
            {
                var sampleFieldsMed = "[{\"Name\":\"SampleCount\",\"Label\":\"عدد الحالات المستهدفة بالدراسة\",\"Type\":\"number\",\"IsRequired\":true},{\"Name\":\"ConsentMethod\",\"Label\":\"طريقة أخذ الموافقة المستنيرة\",\"Type\":\"dropdown\",\"Options\":[\"إقرار كتابي موقع\",\"موافقة خطية من ولي الأمر\",\"نموذج رقمي موثق\"],\"IsRequired\":true},{\"Name\":\"InvasiveProcedures\",\"Label\":\"هل يتضمن البحث إجراءات تداخلية أو سحب عينات دم؟\",\"Type\":\"yesno\",\"IsRequired\":true}]";

                var medCategories = new List<ResearchCategory>
                {
                    new ResearchCategory 
                    { 
                        CollegeId = medCol.Id, 
                        Name = "أبحاث سريرية وتجارب على البشر", 
                        Description = "يشمل التجارب الإكلينيكية، سحب العينات البيولوجية، والملاحظات الطبية المباشرة",
                        FieldsDefinitionJson = sampleFieldsMed
                    },
                    new ResearchCategory 
                    { 
                        CollegeId = medCol.Id, 
                        Name = "أبحاث معملية وحيوانات تجارب", 
                        Description = "يشمل استخدام حيوانات التجارب في المعامل والمختبرات الطبية والبيولوجية",
                        FieldsDefinitionJson = sampleFieldsMed
                    }
                };
                await context.ResearchCategories.AddRangeAsync(medCategories);
            }

            if (!await context.ResearchCategories.AnyAsync(c => c.CollegeId == pharmCol.Id))
            {
                var pharmFields = "[{\"Name\":\"DrugCompound\",\"Label\":\"اسم المادة الفعالة أو المركب المستهدف\",\"Type\":\"text\",\"IsRequired\":true},{\"Name\":\"DosageRoute\",\"Label\":\"طريقة الإعطاء والتطبيق\",\"Type\":\"dropdown\",\"Options\":[\"عن طريق الفم\",\"حقن وريدي\",\"تطبيق موضعي\"],\"IsRequired\":true}]";
                await context.ResearchCategories.AddAsync(new ResearchCategory
                {
                    CollegeId = pharmCol.Id,
                    Name = "دراسات التكافؤ الحيوي والسموم الصيدلانية",
                    Description = "تقييم فاعلية ومأمونية المستحضرات الدوائية والمستخلصات الكيميائية",
                    FieldsDefinitionJson = pharmFields
                });
            }

            if (!await context.ResearchCategories.AnyAsync(c => c.CollegeId == sciCol.Id))
            {
                var sciFields = "[{\"Name\":\"OrganismType\",\"Label\":\"نوع العينات الحيوية أو السلالات\",\"Type\":\"text\",\"IsRequired\":true},{\"Name\":\"BiosafetyLevel\",\"Label\":\"مستوى الأمان الحيوي للمختبر\",\"Type\":\"dropdown\",\"Options\":[\"BSL-1\",\"BSL-2\",\"BSL-3\"],\"IsRequired\":true}]";
                await context.ResearchCategories.AddAsync(new ResearchCategory
                {
                    CollegeId = sciCol.Id,
                    Name = "أبحاث الجينوم والتقنية الحيوية",
                    Description = "دراسات التنميط الجيني، الطفرات البكتيرية، والتطبيقات الحيوية",
                    FieldsDefinitionJson = sciFields
                });
            }

            if (!await context.ResearchCategories.AnyAsync(c => c.CollegeId == nurseCol.Id))
            {
                var nurseFields = "[{\"Name\":\"TargetAudience\",\"Label\":\"الفئة المستهدفة من المرضى\",\"Type\":\"text\",\"IsRequired\":true},{\"Name\":\"InterventionType\",\"Label\":\"طبيعة التدخل التمريضي\",\"Type\":\"dropdown\",\"Options\":[\"تثقيف صحي\",\"متابعة سريرية\",\"دعم نفسي وسلوكي\"],\"IsRequired\":true}]";
                await context.ResearchCategories.AddAsync(new ResearchCategory
                {
                    CollegeId = nurseCol.Id,
                    Name = "استبيانات وتدخلات تمريضية وسلوكية",
                    Description = "دراسات تطوير الرعاية التمريضية وجودة حياة المرضى",
                    FieldsDefinitionJson = nurseFields
                });
            }
            await context.SaveChangesAsync();

            // 5. Seed College Co-Admins entities
            if (!await context.CollegeCoAdmins.AnyAsync(a => a.UserId == coAdminMedUser.Id))
            {
                context.CollegeCoAdmins.Add(new CollegeCoAdmin { UserId = coAdminMedUser.Id, CollegeId = medCol.Id });
            }
            if (!await context.CollegeCoAdmins.AnyAsync(a => a.UserId == coAdminPharmUser.Id))
            {
                context.CollegeCoAdmins.Add(new CollegeCoAdmin { UserId = coAdminPharmUser.Id, CollegeId = pharmCol.Id });
            }
            if (!await context.CollegeCoAdmins.AnyAsync(a => a.UserId == coAdminSciUser.Id))
            {
                context.CollegeCoAdmins.Add(new CollegeCoAdmin { UserId = coAdminSciUser.Id, CollegeId = sciCol.Id });
            }
            await context.SaveChangesAsync();

            // 6. Seed Judges entities
            if (!await context.Judges.AnyAsync(j => j.UserId == judgeAhmedUser.Id))
            {
                context.Judges.Add(new Judge
                {
                    UserId = judgeAhmedUser.Id,
                    Specialty = "جراحة عامة وأخلاقيات التجارب السريرية",
                    AcademicTitle = "أستاذ دكتور",
                    IsActive = true
                });
            }
            if (!await context.Judges.AnyAsync(j => j.UserId == judgeSarahUser.Id))
            {
                context.Judges.Add(new Judge
                {
                    UserId = judgeSarahUser.Id,
                    Specialty = "علم الأدوية والسموم التجريبية",
                    AcademicTitle = "أستاذ مساعد",
                    IsActive = true
                });
            }
            if (!await context.Judges.AnyAsync(j => j.UserId == judgeMahmoudUser.Id))
            {
                context.Judges.Add(new Judge
                {
                    UserId = judgeMahmoudUser.Id,
                    Specialty = "كيمياء حيوية وبيولوجيا جزيئية",
                    AcademicTitle = "أستاذ دكتور",
                    IsActive = true
                });
            }
            if (!await context.Judges.AnyAsync(j => j.UserId == judgeHodaUser.Id))
            {
                context.Judges.Add(new Judge
                {
                    UserId = judgeHodaUser.Id,
                    Specialty = "تمريض باطني وصحة عامة",
                    AcademicTitle = "مدرس دكتور",
                    IsActive = true
                });
            }
            await context.SaveChangesAsync();

            var judgeAhmed = await context.Judges.FirstAsync(j => j.UserId == judgeAhmedUser.Id);
            var judgeSarah = await context.Judges.FirstAsync(j => j.UserId == judgeSarahUser.Id);
            var judgeMahmoud = await context.Judges.FirstAsync(j => j.UserId == judgeMahmoudUser.Id);
            var judgeHoda = await context.Judges.FirstAsync(j => j.UserId == judgeHodaUser.Id);

            // 7. Seed Researchers entities
            if (!await context.Researchers.AnyAsync(r => r.UserId == researcherTarekUser.Id))
            {
                context.Researchers.Add(new Researcher
                {
                    UserId = researcherTarekUser.Id,
                    CollegeId = medCol.Id,
                    ResearcherType = ResearcherTypeConstants.FacultyMember,
                    AcademicDegree = "دكتوراه",
                    Department = "قسم الباطنة العامة",
                    NationalId = "28501011234567"
                });
            }
            if (!await context.Researchers.AnyAsync(r => r.UserId == researcherFatimaUser.Id))
            {
                context.Researchers.Add(new Researcher
                {
                    UserId = researcherFatimaUser.Id,
                    CollegeId = pharmCol.Id,
                    ResearcherType = ResearcherTypeConstants.Researcher,
                    AcademicDegree = "ماجستير",
                    Department = "قسم الأدوية والسموم",
                    NationalId = "29002021234568"
                });
            }
            if (!await context.Researchers.AnyAsync(r => r.UserId == researcherAmrUser.Id))
            {
                context.Researchers.Add(new Researcher
                {
                    UserId = researcherAmrUser.Id,
                    CollegeId = sciCol.Id,
                    ResearcherType = ResearcherTypeConstants.PostgraduateStudent,
                    AcademicDegree = "بكالوريوس",
                    Department = "قسم علم الحيوان والمناعة",
                    NationalId = "29503031234569"
                });
            }
            await context.SaveChangesAsync();

            var resTarek = await context.Researchers.FirstAsync(r => r.UserId == researcherTarekUser.Id);
            var resFatima = await context.Researchers.FirstAsync(r => r.UserId == researcherFatimaUser.Id);
            var resAmr = await context.Researchers.FirstAsync(r => r.UserId == researcherAmrUser.Id);

            var catMedClinical = await context.ResearchCategories.FirstOrDefaultAsync(c => c.CollegeId == medCol.Id) ?? await context.ResearchCategories.FirstAsync();
            var catPharmTox = await context.ResearchCategories.FirstOrDefaultAsync(c => c.CollegeId == pharmCol.Id) ?? catMedClinical;
            var catSciGen = await context.ResearchCategories.FirstOrDefaultAsync(c => c.CollegeId == sciCol.Id) ?? catMedClinical;
            var catNurse = await context.ResearchCategories.FirstOrDefaultAsync(c => c.CollegeId == nurseCol.Id) ?? catMedClinical;

            // 8. Seed Researches Across Multiple Stages (if no researches exist)
            if (!await context.Researches.AnyAsync())
            {
                // Research 1: Under Formal Screening (في طابور الفرز بالطب)
                var r1 = new Research
                {
                    Title = "تقييم فاعلية بروتوكول علاجي جديد قائم على الأجسام المضادة الموجهة في علاج أورام الرئة",
                    CollegeId = medCol.Id,
                    CategoryId = catMedClinical.Id,
                    AcademicSpecialty = "أورام وباطنة سريرية",
                    WorkflowStatus = ResearchStatusConstants.UnderFormalScreening,
                    SubmittedAt = DateTime.UtcNow.AddHours(-18),
                    ScreeningDueAt = DateTime.UtcNow.AddHours(30),
                    DynamicFieldsJson = "{\"SampleCount\":120,\"ConsentMethod\":\"إقرار كتابي موقع\",\"InvasiveProcedures\":\"نعم\"}"
                };
                context.Researches.Add(r1);
                await context.SaveChangesAsync();

                context.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
                {
                    ResearchId = r1.Id,
                    ResearcherId = resTarek.Id,
                    IsMainAuthor = true
                });

                context.ResearchAttachments.Add(new ResearchAttachment
                {
                    ResearchId = r1.Id,
                    AttachmentType = AttachmentTypeConstants.ResearchProposal,
                    OriginalFileName = "خطة_البحث_المفصلة.pdf",
                    StoredFileName = "r1_proposal.pdf",
                    FileSize = 1024 * 350,
                    UploadedAt = r1.SubmittedAt
                });
                context.ResearchAttachments.Add(new ResearchAttachment
                {
                    ResearchId = r1.Id,
                    AttachmentType = AttachmentTypeConstants.InformedConsent,
                    OriginalFileName = "نموذج_الإقرار_المستنير_للمرضى.pdf",
                    StoredFileName = "r1_consent.pdf",
                    FileSize = 1024 * 120,
                    UploadedAt = r1.SubmittedAt
                });

                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r1.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherTarekUser.Id,
                    ChangedAt = r1.SubmittedAt,
                    Reason = "تم تقديم المقترح بنجاح عبر المنصة، ودخل مرحلة الفرز الإداري بالكلية."
                });

                // Research 2: Assigned to Judges (في مرحلة التحكيم بالصيدلة)
                var r2 = new Research
                {
                    Title = "دراسة السمية الخلوية والمطفرة لمركبات كيميائية جديدة مشتقة من الفلافونويدات على خطوط خلايا سرطانية",
                    CollegeId = pharmCol.Id,
                    CategoryId = catPharmTox.Id,
                    AcademicSpecialty = "علم الأدوية والسموم التجريبية",
                    WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                    SubmittedAt = DateTime.UtcNow.AddDays(-6),
                    ScreeningDueAt = DateTime.UtcNow.AddDays(-4),
                    ScreeningNotes = "اجتاز الفحص الشكلي واستوفى جميع المتطلبات الإدارية وموافقة رئيس القسم.",
                    DynamicFieldsJson = "{\"DrugCompound\":\"Flavonoid-Derivative-7B\",\"DosageRoute\":\"حقن وريدي\"}"
                };
                context.Researches.Add(r2);
                await context.SaveChangesAsync();

                context.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
                {
                    ResearchId = r2.Id,
                    ResearcherId = resFatima.Id,
                    IsMainAuthor = true
                });

                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r2.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherFatimaUser.Id,
                    ChangedAt = r2.SubmittedAt,
                    Reason = "تقديم أولي للمقترح."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r2.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = coAdminPharmUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-5),
                    Reason = "اجتياز الفرز الشكلي وإسناد المقترح لاثنين من المحكمين المعتمدين."
                });

                // Evaluations for R2:
                // Judge Sarah submitted Approved
                context.Evaluations.Add(new Evaluation
                {
                    ResearchId = r2.Id,
                    JudgeId = judgeSarah.Id,
                    RoundNumber = 1,
                    Decision = EvaluationDecisionConstants.Approved,
                    PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                    RiskLevel = "مخاطر منخفضة ومبررة",
                    InformedConsentRating = EthicsRatingConstants.Appropriate,
                    Recommendations = "تم استيفاء معايير السلامة المعملية ومبررات التجارب على الخلايا.",
                    IsDigitallySigned = true,
                    SignedAt = DateTime.UtcNow.AddDays(-2),
                    SubmittedAt = DateTime.UtcNow.AddDays(-2)
                });
                // Judge Mahmoud evaluation pending
                context.Evaluations.Add(new Evaluation
                {
                    ResearchId = r2.Id,
                    JudgeId = judgeMahmoud.Id,
                    RoundNumber = 1,
                    Decision = EvaluationDecisionConstants.UnderEvaluation,
                    IsDigitallySigned = false
                });

                // Research 3: Modifications Required (مطلوب تعديلات بالعلوم)
                var r3 = new Research
                {
                    Title = "التنميط الجيني للطفرات المرتبطة بمقاومة المضادات الحيوية في بكتيريا المستشفيات",
                    CollegeId = sciCol.Id,
                    CategoryId = catSciGen.Id,
                    AcademicSpecialty = "ميكروبيولوجيا وجينات جزيئية",
                    WorkflowStatus = ResearchStatusConstants.ModificationsRequired,
                    SubmittedAt = DateTime.UtcNow.AddDays(-12),
                    ScreeningDueAt = DateTime.UtcNow.AddDays(-10),
                    ScreeningNotes = "مستوفٍ للشروط الشكلية وتم تمريره لهيئة التحكيم.",
                    DynamicFieldsJson = "{\"OrganismType\":\"Klebsiella pneumoniae & Pseudomonas aeruginosa\",\"BiosafetyLevel\":\"BSL-2\"}"
                };
                context.Researches.Add(r3);
                await context.SaveChangesAsync();

                context.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
                {
                    ResearchId = r3.Id,
                    ResearcherId = resAmr.Id,
                    IsMainAuthor = true
                });

                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r3.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherAmrUser.Id,
                    ChangedAt = r3.SubmittedAt,
                    Reason = "تقديم المقترح."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r3.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = coAdminSciUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-11),
                    Reason = "إحالة للمحكمين."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r3.Id,
                    OldStatus = ResearchStatusConstants.AssignedToJudges,
                    NewStatus = ResearchStatusConstants.ModificationsRequired,
                    ChangedByUserId = judgeMahmoudUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-4),
                    Reason = "ملاحظات التعديل المطلوبة من هيئة التحكيم:\n• يرجى إعادة صياغة نموذج الموافقة وإضافة تدابير التخلص الآمن من النفايات البيولوجية المصابة."
                });

                context.Evaluations.Add(new Evaluation
                {
                    ResearchId = r3.Id,
                    JudgeId = judgeMahmoud.Id,
                    RoundNumber = 1,
                    Decision = EvaluationDecisionConstants.ModificationsRequired,
                    PrivacyProtectionRating = EthicsRatingConstants.NeedsModification,
                    RiskLevel = "مخاطر حيوية تتطلب احتياطات مشددة",
                    InformedConsentRating = EthicsRatingConstants.NeedsModification,
                    ModificationNotes = "• يرجى توضيح آليات التخلص الآمن من المخلفات البكتيرية المقاومة للمضادات.\n• استكمال نموذج خطة احتواء المخاطر الحيوية وفق معايير BSL-2.",
                    IsDigitallySigned = true,
                    SignedAt = DateTime.UtcNow.AddDays(-4),
                    SubmittedAt = DateTime.UtcNow.AddDays(-4)
                });

                // Research 4: Approved (معتمد وجاهز لإصدار الشهادة بالتمريض)
                var r4 = new Research
                {
                    Title = "تأثير برنامج دعم تمريضي نفسي سلوكي على جودة حياة مرضى الفشل الكلوي المزمن",
                    CollegeId = nurseCol.Id,
                    CategoryId = catNurse.Id,
                    AcademicSpecialty = "تمريض باطني وجودة رعاية صحية",
                    WorkflowStatus = ResearchStatusConstants.Approved,
                    SubmittedAt = DateTime.UtcNow.AddDays(-16),
                    ScreeningDueAt = DateTime.UtcNow.AddDays(-14),
                    ScreeningNotes = "مستوفٍ ومعتمد إدارياً.",
                    DynamicFieldsJson = "{\"TargetAudience\":\"مرضى الغسيل الكلوي المزمن بمستشفى الجامعة\",\"InterventionType\":\"دعم نفسي وسلوكي\"}"
                };
                context.Researches.Add(r4);
                await context.SaveChangesAsync();

                context.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
                {
                    ResearchId = r4.Id,
                    ResearcherId = resTarek.Id,
                    IsMainAuthor = true
                });

                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r4.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherTarekUser.Id,
                    ChangedAt = r4.SubmittedAt,
                    Reason = "تقديم البحث."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r4.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = coAdminMedUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-15),
                    Reason = "إحالة للمحكمين."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r4.Id,
                    OldStatus = ResearchStatusConstants.AssignedToJudges,
                    NewStatus = ResearchStatusConstants.Approved,
                    ChangedByUserId = judgeHodaUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-2),
                    Reason = "موافقة كاملة من هيئة التحكيم، والمقترح بانتظار توقيع وإصدار الشهادة الأخلاقية."
                });

                context.Evaluations.Add(new Evaluation
                {
                    ResearchId = r4.Id,
                    JudgeId = judgeHoda.Id,
                    RoundNumber = 1,
                    Decision = EvaluationDecisionConstants.Approved,
                    PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                    RiskLevel = "مخاطر منعدمة (دراسة تدريبية وسلوكية)",
                    InformedConsentRating = EthicsRatingConstants.Appropriate,
                    Recommendations = "المقترح ذو قيمة تطبيقية متميزة ويحقق أعلى معايير أخلاقيات الرعاية الصحية.",
                    IsDigitallySigned = true,
                    SignedAt = DateTime.UtcNow.AddDays(-2),
                    SubmittedAt = DateTime.UtcNow.AddDays(-2)
                });

                // Research 5: Certificate Issued (صادر الشهادة وموثق بالطب)
                var r5 = new Research
                {
                    Title = "دراسة سريرية مقارنة لمأمونية العقاقير البيولوجية الحديثة لدى مرضى السكري من النوع الثاني",
                    CollegeId = medCol.Id,
                    CategoryId = catMedClinical.Id,
                    AcademicSpecialty = "غدد صماء وتجارب دوائية سريرية",
                    WorkflowStatus = ResearchStatusConstants.CertificateIssued,
                    SubmittedAt = DateTime.UtcNow.AddDays(-28),
                    ScreeningDueAt = DateTime.UtcNow.AddDays(-26),
                    ScreeningNotes = "مستوفٍ بالكامل لمتطلبات لجان الأخلاقيات.",
                    CertificateSerialNumber = "REC-MED-2026-0001",
                    CertificateIssuedAt = DateTime.UtcNow.AddDays(-6),
                    CertificateQrCodeUrl = "/Public/VerifyCertificate?serialNumber=REC-MED-2026-0001",
                    DynamicFieldsJson = "{\"SampleCount\":200,\"ConsentMethod\":\"إقرار كتابي موقع\",\"InvasiveProcedures\":\"نعم\"}"
                };
                context.Researches.Add(r5);
                await context.SaveChangesAsync();

                context.ResearcherResearchesBridge.Add(new ResearcherResearchesBridge
                {
                    ResearchId = r5.Id,
                    ResearcherId = resTarek.Id,
                    IsMainAuthor = true
                });

                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r5.Id,
                    OldStatus = null,
                    NewStatus = ResearchStatusConstants.UnderFormalScreening,
                    ChangedByUserId = researcherTarekUser.Id,
                    ChangedAt = r5.SubmittedAt,
                    Reason = "تقديم البحث."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r5.Id,
                    OldStatus = ResearchStatusConstants.UnderFormalScreening,
                    NewStatus = ResearchStatusConstants.AssignedToJudges,
                    ChangedByUserId = coAdminMedUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-27),
                    Reason = "إحالة للمحكمين."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r5.Id,
                    OldStatus = ResearchStatusConstants.AssignedToJudges,
                    NewStatus = ResearchStatusConstants.Approved,
                    ChangedByUserId = judgeAhmedUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-8),
                    Reason = "موافقة كاملة من هيئة التحكيم."
                });
                context.ResearchStatusHistories.Add(new ResearchStatusHistory
                {
                    ResearchId = r5.Id,
                    OldStatus = ResearchStatusConstants.Approved,
                    NewStatus = ResearchStatusConstants.CertificateIssued,
                    ChangedByUserId = coAdminMedUser.Id,
                    ChangedAt = DateTime.UtcNow.AddDays(-6),
                    Reason = "تم اعتماد البحث رسمياً وإصدار الشهادة الأخلاقية الرقمية برقم: REC-MED-2026-0001"
                });

                context.Evaluations.Add(new Evaluation
                {
                    ResearchId = r5.Id,
                    JudgeId = judgeAhmed.Id,
                    RoundNumber = 1,
                    Decision = EvaluationDecisionConstants.Approved,
                    PrivacyProtectionRating = EthicsRatingConstants.Appropriate,
                    RiskLevel = "مخاطر علاجية مضبوطة وفق البروتوكول الطبي",
                    InformedConsentRating = EthicsRatingConstants.Appropriate,
                    Recommendations = "التوصية بالموافقة وإصدار شهادة الاعتماد الأخلاقي.",
                    IsDigitallySigned = true,
                    SignedAt = DateTime.UtcNow.AddDays(-8),
                    SubmittedAt = DateTime.UtcNow.AddDays(-8)
                });

                await context.SaveChangesAsync();

                // 9. Seed Sample In-App Notifications for each user
                var sampleNotifications = new List<Notification>
                {
                    // SuperAdmin Notifications
                    new Notification
                    {
                        UserId = superAdminUser.Id,
                        Title = "تقرير الجودة الشهري جاهز",
                        Message = "تم تحديث مؤشرات الأداء والاعتماد الأكاديمي لكافة كليات الجامعة.",
                        TargetUrl = "/SuperAdmin/Reports",
                        Type = "SystemReport",
                        Icon = "bi-graph-up-arrow",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    },
                    new Notification
                    {
                        UserId = superAdminUser.Id,
                        Title = "إصدار شهادة أخلاقية جديدة",
                        Message = "أصدرت كلية الطب البشري الشهادة الرقمية المعتمدة رقم REC-MED-2026-0001.",
                        TargetUrl = "/SuperAdmin/Certificates",
                        Type = "CertificateIssued",
                        Icon = "bi-patch-check",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-5)
                    },

                    // CoAdmin Med Notifications
                    new Notification
                    {
                        UserId = coAdminMedUser.Id,
                        Title = "مقترح جديد يتطلب الفرز الإداري",
                        Message = $"تم تقديم مقترح بحثي جديد: '{r1.Title}'. يرجى المراجعة وتعيين المحكمين.",
                        TargetUrl = $"/CollegeCoAdmin/Review/{r1.Id}",
                        Type = "NewSubmission",
                        Icon = "bi-inbox",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-18)
                    },

                    // Judge Mahmoud Notifications
                    new Notification
                    {
                        UserId = judgeMahmoudUser.Id,
                        Title = "تكليف بتحكيم مقترح بحثي جديد",
                        Message = $"تم تكليفكم بتحكيم المقترح: '{r2.Title}'. يرجى استكمال استمارة التحكيم.",
                        TargetUrl = $"/Judge/AssignedResearches",
                        Type = "JudgeAssignment",
                        Icon = "bi-mortarboard",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },

                    // Researcher Amr Notifications
                    new Notification
                    {
                        UserId = researcherAmrUser.Id,
                        Title = "مطلوب استيفاء تعديلات أخلاقية",
                        Message = $"قررت هيئة التحكيم طلب تعديلات وملاحظات على مقترحكم '{r3.Title}'.",
                        TargetUrl = $"/Researcher/Modify/{r3.Id}",
                        Type = "ModificationRequired",
                        Icon = "bi-pencil-square",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },

                    // Researcher Tarek Notifications
                    new Notification
                    {
                        UserId = researcherTarekUser.Id,
                        Title = "صدور الشهادة الأخلاقية المعتمدة",
                        Message = "مبارك! تم إصدار الشهادة الأخلاقية الرقمية رقم REC-MED-2026-0001 لمقترحكم.",
                        TargetUrl = "/Researcher/Certificates",
                        Type = "CertificateIssued",
                        Icon = "bi-award",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-6)
                    }
                };

                await context.Notifications.AddRangeAsync(sampleNotifications);
                await context.SaveChangesAsync();
            }
        }
    }
}
