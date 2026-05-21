using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class OfficesService(IDbContextFactory<AppDbContext> factory)
    {
        private readonly IDbContextFactory<AppDbContext> _factory = factory;

        public async Task<ListDataResponse<OfficeDTO>> GetOfficesAsync(
            int page,
            int pageSize = 20,
            string? searchText = null
        )
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            var query = dbContext.Offices.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(o => o.Address.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));

            int count = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => Office.ToDTO(o))
                .ToListAsync();

            return new ListDataResponse<OfficeDTO>(count, items);
        }
    }
}
