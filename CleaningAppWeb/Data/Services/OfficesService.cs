using CleaningAppWeb.Domain.DTOs;

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
            var query = _appDbContext.Offices.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(o => o.Address.Contains(searchText, StringComparison.CurrentCultureIgnoreCase));

            int count = query.Count();

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var officesDTO = query.Select(o => new OfficeDTO
            {
                Id = o.Id,
                Address = o.Address,
                Name = o.Name
            }).ToList();

            return new ListDataResponse<OfficeDTO>(count, officesDTO);
        }
    }
}
