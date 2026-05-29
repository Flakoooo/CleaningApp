using CleaningAppWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class ExpiredApplicationsService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<ExpiredApplicationsService> logger
    ) : BackgroundService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly ILogger<ExpiredApplicationsService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddHours(now.Hour + 1);
                var delay = nextRun - now;

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    await MarkExpiredApplications(stoppingToken);
            }
        }

        private async Task MarkExpiredApplications(CancellationToken stoppingToken)
        {
            await using var dbContext = await _dbFactory.CreateDbContextAsync(stoppingToken);

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);
            var currentTime = now.TimeOfDay;

            var expired = await dbContext.Applications
                .Where(a => a.Status == CleaningApplicationStatus.Waiting &&
                            a.ExecutorId == null &&
                            a.CleaningDate <= today &&
                            (a.CleaningDate < today || a.CleaningTime.ToTimeSpan() <= currentTime))
                .ToListAsync(stoppingToken);

            if (expired.Count > 0)
            {
                foreach (var app in expired)
                {
                    app.Status = CleaningApplicationStatus.Expired;
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation(
                            "Заявка {Id} просрочена (не взята в работу к {CleaningTime})", 
                            app.Id, app.CleaningTime
                        );
                }
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
