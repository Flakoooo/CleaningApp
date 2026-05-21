using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class RoomsService(IDbContextFactory<AppDbContext> factory)
    {
        private readonly IDbContextFactory<AppDbContext> _factory = factory;

        public async Task<ListDataResponse<RoomDTO>> GetRoomsAsync(
            int page,
            int pageSize = 20,
            Guid? officeId = null,
            string? searchText = null
        )
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            var query = dbContext.Rooms
                .AsNoTracking()
                .Where(r => !officeId.HasValue || r.OfficeId == officeId.Value);

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(r => r.IsActive && r.RoomNumber.ToString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase));

            int count = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => Room.ToDTO(r))
                .ToListAsync();

            return new ListDataResponse<RoomDTO>(count, items);
        }
    }
}
