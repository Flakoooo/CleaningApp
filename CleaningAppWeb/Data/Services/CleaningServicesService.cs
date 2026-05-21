using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class CleaningServicesService(IDbContextFactory<AppDbContext> factory)
    {
        private readonly IDbContextFactory<AppDbContext> _factory = factory;

        public async Task<List<ServiceDTO>> GetAvailableServices()
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            return await dbContext.Services
                .AsNoTracking()
                .Where(s => s.IsActive)
                .Select(s => Service.ToDTO(s))
                .ToListAsync();
        }
    }
}
