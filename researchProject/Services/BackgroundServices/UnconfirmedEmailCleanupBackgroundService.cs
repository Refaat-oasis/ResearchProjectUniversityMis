using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using researchProject.Repositories.Interfaces;

namespace researchProject.Services.BackgroundServices
{
    public class UnconfirmedEmailCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnconfirmedEmailCleanupBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
        private readonly TimeSpan _unconfirmedThreshold = TimeSpan.FromDays(1);

        public UnconfirmedEmailCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<UnconfirmedEmailCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("بدء تشغيل خدمة التنظيف الدوري للحسابات غير المفعلة التي تجاوزت 24 ساعة.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var accountRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
                        int deletedCount = await accountRepo.CleanupUnconfirmedAccountsAsync(_unconfirmedThreshold);
                        if (deletedCount > 0)
                        {
                            _logger.LogInformation("تم حذف {Count} حساب غير مؤكد تجاوز 24 ساعة بنجاح.", deletedCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "حدث خطأ أثناء تنفيذ فحص تنظيف الحسابات غير المفعلة.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("تم إيقاف خدمة التنظيف الدوري للحسابات غير المفعلة.");
        }
    }
}

