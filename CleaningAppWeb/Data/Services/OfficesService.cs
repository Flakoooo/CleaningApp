using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class OfficesService(AppDbContext appDbContext)
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<ListDataResponse<OfficeDTO>> GetOfficesAsync(
            int page,
            int pageSize = 20,
            string? searchText = null
        )
        {
            var query = _appDbContext.Offices.AsNoTracking();

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
