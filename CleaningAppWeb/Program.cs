using CleaningAppWeb.Auth;
using CleaningAppWeb.Components;
using CleaningAppWeb.Data;
using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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

            builder.Services.AddRazorPages();

            builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

            builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
            builder.Services.AddCascadingAuthenticationState();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomClaimsPrincipalFactory>();

            builder.Services.AddScoped<OfficesService>();
            builder.Services.AddScoped<RoomsService>();
            builder.Services.AddScoped<CleaningServicesService>();
            builder.Services.AddScoped<CleaningApplicationsService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorPages();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
#if DEBUG
                dbContext.Database.EnsureCreated();
                await SeedData.Initialize(dbContext, scope.ServiceProvider);
#else
                await dbContext.Database.MigrateAsync();
#endif
            }

            app.Run();
        }
    }
}
