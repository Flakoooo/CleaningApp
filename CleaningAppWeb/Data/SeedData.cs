using CleaningAppWeb.Domain.Entities;
using CleaningAppWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                await context.Users.AddRangeAsync(
                    User.Create("vlasovvv", "12345678", RoleType.Officer, "Вадим", "Власов", "Владимирович", "+7-(912)-345-67-89"),
                    User.Create("officer1", "12345678", RoleType.Officer, "Работник", "Офисный", "Тестовый", "+7-(923)-232-32-32"),
                    User.Create("ivanii", "87654321", RoleType.Cleaner, "Иван", "Иванов", "Иванович", "+7-(998)-765-43-21"),
                    User.Create("cleaner1", "12345678", RoleType.Cleaner, "Работник", "Клининга", "Тестовый", "+7-(909)-080-70-60")
                );

                await context.SaveChangesAsync();
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
