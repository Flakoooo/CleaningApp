using CleaningAppWeb.Components;
using CleaningAppWeb.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CleaningAppWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    using var testConnection = new NpgsqlConnection(connectionString);
                    testConnection.Open();
                    testConnection.Close();
                    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
                    Console.WriteLine("Подключение к PostgreSQL установлено");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения к PostgreSQL: {ex.Message}");

#if DEBUG
                    Console.WriteLine("Переключение на In-Memory базу данных");
                    builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("CleaningAppFallback"));
#else
                    throw new InvalidOperationException("Не удалось подключиться к базе данных PostgreSQL", ex);
#endif
                }
            }
            else
            {
#if DEBUG
                Console.WriteLine("Строка подключения не указана. Переключение на In-Memory базу данных");
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("CleaningAppFallback"));
#else
                throw new InvalidOperationException("Не указана строка подключения к базе данных PostgreSQL");
#endif
            }

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
#if DEBUG
                dbContext.Database.EnsureCreated();
#else
                await dbContext.Database.MigrateAsync();
#endif

#if DEBUG
                await SeedData.Initialize(dbContext);
#endif
            }

            app.Run();
        }
    }
}
