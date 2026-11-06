using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

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

            // 2. Seed SuperAdmin User
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@mis.edu.eg";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin@Mis2026Password!";
            var adminFullName = configuration["AdminUser:FullName"] ?? "مدير النظام العام للمنصة";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, RoleConstants.SuperAdmin);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingAdmin, RoleConstants.SuperAdmin))
                {
                    await userManager.AddToRoleAsync(existingAdmin, RoleConstants.SuperAdmin);
                }
            }

            // 3. Seed Default Colleges if empty
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

                // Seed Default Research Categories for the first college
                var medCollege = colleges.First(c => c.Code == "MED");
                var sampleFields = "[{\"Name\":\"SampleCount\",\"Label\":\"عدد العينات المستهدفة\",\"Type\":\"number\",\"IsRequired\":true},{\"Name\":\"AnimalSpecies\",\"Label\":\"فصيلة الكائن / العينة\",\"Type\":\"text\",\"IsRequired\":false},{\"Name\":\"ConsentMethod\",\"Label\":\"طريقة أخذ الإقرار المستنير\",\"Type\":\"dropdown\",\"Options\":[\"إقرار خطي موقع\",\"موافقة شفهية موثقة\",\"نموذج إلكتروني\"],\"IsRequired\":true}]";

                var categories = new List<ResearchCategory>
                {
                    new ResearchCategory 
                    { 
                        CollegeId = medCollege.Id, 
                        Name = "أبحاث سريرية وتجارب على البشر", 
                        Description = "يشمل التجارب الإكلينيكية، سحب العينات البيولوجية، والملاحظات الطبية المباشرة",
                        FieldsDefinitionJson = sampleFields
                    },
                    new ResearchCategory 
                    { 
                        CollegeId = medCollege.Id, 
                        Name = "أبحاث حيوانية وتجارب معملية", 
                        Description = "يشمل استخدام حيوانات التجارب في المعامل والمختبرات الطبية والبيولوجية",
                        FieldsDefinitionJson = sampleFields
                    },
                    new ResearchCategory 
                    { 
                        CollegeId = medCollege.Id, 
                        Name = "استبيانات ودراسات سلوكية واجتماعية", 
                        Description = "يشمل الاستبيانات والمقابلات الشخصية وجمع البيانات السلوكية عن الأفراد",
                        FieldsDefinitionJson = "[{\"Name\":\"TargetGroup\",\"Label\":\"الفئة المستهدفة بالاستبيان\",\"Type\":\"text\",\"IsRequired\":true},{\"Name\":\"SampleExpectedSize\",\"Label\":\"حجم العينة المتوقع\",\"Type\":\"number\",\"IsRequired\":true},{\"Name\":\"DataAnonymity\",\"Label\":\"ضمان سرية البيانات وتجهيل الهوية\",\"Type\":\"yesno\",\"IsRequired\":true}]"
                    }
                };

                await context.ResearchCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}
