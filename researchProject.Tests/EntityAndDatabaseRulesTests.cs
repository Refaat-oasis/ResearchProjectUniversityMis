using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using Xunit;

namespace researchProject.Tests
{
    public class EntityAndDatabaseRulesTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void EntityClasses_ContainOnlyScalarAndNavigationProperties_NoMethods()
        {
            var entityTypes = new[]
            {
                typeof(College),
                typeof(CollegeCoAdmin),
                typeof(Researcher),
                typeof(ResearchCategory),
                typeof(Research),
                typeof(ResearcherResearchesBridge),
                typeof(ResearchAttachment),
                typeof(Judge),
                typeof(Evaluation),
                typeof(JudgeInvitation),
                typeof(ResearchStatusHistory)
            };

            foreach (var type in entityTypes)
            {
                // Verify no custom methods defined in domain entity classes
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | 
                                             System.Reflection.BindingFlags.Instance | 
                                             System.Reflection.BindingFlags.DeclaredOnly)
                                  .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                                  .ToList();

                Assert.Empty(methods);
            }
        }

        [Fact]
        public void ResearchStatusConstants_HasExactlyFiveArabicStatuses()
        {
            Assert.Equal("قيد المراجعة الشكلية", ResearchStatusConstants.UnderFormalScreening);
            Assert.Equal("مسند للمحكمين", ResearchStatusConstants.AssignedToJudges);
            Assert.Equal("تعديلات مطلوبة", ResearchStatusConstants.ModificationsRequired);
            Assert.Equal("معتمد", ResearchStatusConstants.Approved);
            Assert.Equal("صادر الشهادة", ResearchStatusConstants.CertificateIssued);
        }

        [Fact]
        public async Task Evaluation_MultipleJudgesForSameRound_Succeeds()
        {
            var db = GetInMemoryDbContext("Evaluation_MultiJudge_Success");

            var college = new College { Name = "كلية الطب", Code = "MED" };
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var category = new ResearchCategory { Name = "أبحاث سريرية", CollegeId = college.Id };
            db.ResearchCategories.Add(category);
            await db.SaveChangesAsync();

            var research = new Research
            {
                Title = "بحث سريري تجريبي",
                CollegeId = college.Id,
                CategoryId = category.Id,
                AcademicSpecialty = "طب سريري",
                WorkflowStatus = ResearchStatusConstants.AssignedToJudges,
                SubmittedAt = DateTime.UtcNow,
                ScreeningDueAt = DateTime.UtcNow.AddHours(48)
            };
            db.Researches.Add(research);
            await db.SaveChangesAsync();

            var eval1 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = 1,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };
            var eval2 = new Evaluation
            {
                ResearchId = research.Id,
                JudgeId = 2,
                RoundNumber = 1,
                Decision = EvaluationDecisionConstants.UnderEvaluation
            };

            db.Evaluations.AddRange(eval1, eval2);
            await db.SaveChangesAsync();

            var savedEvals = await db.Evaluations.Where(e => e.ResearchId == research.Id && e.RoundNumber == 1).ToListAsync();
            Assert.Equal(2, savedEvals.Count);
        }

        [Fact]
        public void ArabicDataAnnotations_ValidateRequiredFields()
        {
            var user = new ApplicationUser { FullName = "" };
            var validationContext = new ValidationContext(user) { MemberName = "FullName" };
            var results = new System.Collections.Generic.List<ValidationResult>();

            bool isValid = Validator.TryValidateProperty(user.FullName, validationContext, results);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage == "الاسم الكامل مطلوب");
        }
    }
}
