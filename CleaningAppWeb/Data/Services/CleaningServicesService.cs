using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data.Services
{
    public class CleaningServicesService(AppDbContext appDbContext)
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<List<ServiceDTO>> GetAvailableServices() => await _appDbContext.Services
            .Where(s => s.IsActive)
            .Select(s => Service.ToDTO(s))
            .ToListAsync();
    }
}
