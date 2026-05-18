using CleaningAppWeb.Domain.Entities;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            if (!await context.Users.AnyAsync())
            {
                var users = new (string login, string password, RoleType role, string firstName, string lastName, string patronymic, string phone)[]
                {
                    ("vlasovvv", "12345678", RoleType.Officer, "Вадим", "Власов", "Владимирович", "+7-(912)-345-67-89"),
                    ("officer1", "12345678", RoleType.Officer, "Работник", "Офисный", "Тестовый", "+7-(923)-232-32-32"),
                    ("ivanii", "87654321", RoleType.Cleaner, "Иван", "Иванов", "Иванович", "+7-(998)-765-43-21"),
                    ("cleaner1", "12345678", RoleType.Cleaner, "Работник", "Клининга", "Тестовый", "+7-(909)-080-70-60")
                };

                foreach (var (login, password, role, firstName, lastName, patronymic, phone) in users)
                {
                    var user = User.Create(login, role, firstName, lastName, patronymic, phone);

                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        Console.WriteLine($"Ошибка создания пользователя {login}: {errors}");
                    }
                }
            }

            if (!await context.Offices.AnyAsync())
            {
                var office1 = Guid.NewGuid();
                var office2 = Guid.NewGuid();

                await context.Offices.AddRangeAsync(
                    new Office { Id = office1, Address = "улица Володарского, 38", Name = "Технологический" },
                    new Office { Id = office2, Address = "улица Мельникайте, 70", Name = "ИСОУ" }
                );

                await context.SaveChangesAsync();

                await context.Rooms.AddRangeAsync(
                    new Room { OfficeId = office1, RoomNumber = 100 },
                    new Room { OfficeId = office1, RoomNumber = 101 },
                    new Room { OfficeId = office1, RoomNumber = 103 },
                    new Room { OfficeId = office1, RoomNumber = 216 },
                    new Room { OfficeId = office1, RoomNumber = 512 },
                    new Room { OfficeId = office2, RoomNumber = 1104 },
                    new Room { OfficeId = office2, RoomNumber = 306 },
                    new Room { OfficeId = office2, RoomNumber = 512 },
                    new Room { OfficeId = office2, RoomNumber = 1508 }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
