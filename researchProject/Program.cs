using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using researchProject.Data;
using researchProject.Hubs;
using researchProject.Models.Constants;
using researchProject.Models.Entities;
using researchProject.Repositories.Implementations;
using researchProject.Repositories.Interfaces;
using researchProject.Services;
using researchProject.Services.Implementations;
using researchProject.Services.Interfaces;

namespace researchProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Database Context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=DESKTOP-I633H4F;Database=ResearchEthicsPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            // 3. Policy-Based Authorization (User Rule 2)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole(RoleConstants.SuperAdmin));
                options.AddPolicy("CollegeCoAdminOnly", policy => policy.RequireRole(RoleConstants.CollegeCoAdmin));
                options.AddPolicy("JudgeOnly", policy => policy.RequireRole(RoleConstants.Judge));
                options.AddPolicy("ResearcherOnly", policy => policy.RequireRole(RoleConstants.Researcher));
                options.AddPolicy("CoAdminOrSuperAdmin", policy => policy.RequireRole(RoleConstants.CollegeCoAdmin, RoleConstants.SuperAdmin));
                options.AddPolicy("JudgeOrSuperAdmin", policy => policy.RequireRole(RoleConstants.Judge, RoleConstants.SuperAdmin));
                options.AddPolicy("ResearcherOrSuperAdmin", policy => policy.RequireRole(RoleConstants.Researcher, RoleConstants.SuperAdmin));
            });

            // 4. Shared Domain Services
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IAttachmentService, AttachmentService>();
            builder.Services.AddScoped<IReportExportService, ReportExportService>();
            builder.Services.AddSignalR();

            // 4. Dedicated Repositories Per Actor
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
            builder.Services.AddScoped<ICollegeCoAdminRepository, CollegeCoAdminRepository>();
            builder.Services.AddScoped<IJudgeRepository, JudgeRepository>();
            builder.Services.AddScoped<IResearcherRepository, ResearcherRepository>();
            builder.Services.AddScoped<IPublicRepository, PublicRepository>();

            // 5. MVC with Views & Anti-Forgery
            builder.Services.AddControllersWithViews(options =>
            {
                // Auto anti-forgery can be enabled or used per action
            });
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            var app = builder.Build();

            // 6. Database Migration / Seeding
            try
            {
                await DbSeeder.SeedAsync(app.Services, app.Configuration);
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "حدث خطأ أثناء تهيئة قاعدة البيانات والتلقيم الأولي (Seeding).");
            }

            // 7. Request Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<NotificationHub>("/hubs/notifications");

            await app.RunAsync();
        }
    }
}
