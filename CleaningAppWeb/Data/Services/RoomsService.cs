using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;

namespace CleaningAppWeb.Data.Services
{
    public class RoomsService(AppDbContext appDbContext)
    {
        private readonly AppDbContext _appDbContext = appDbContext;

        public async Task<ListDataResponse<RoomDTO>> GetRoomsAsync(
            int page,
            int pageSize = 20,
            Guid? officeId = null,
            string? searchText = null
        )
        {
            IEnumerable<Room> query = [];
            if (officeId.HasValue)
                query = _appDbContext.Rooms.Where(r => r.OfficeId == officeId.Value);
            else
                query = _appDbContext.Rooms.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(r => r.IsActive && r.RoomNumber.ToString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase));

            int count = query.Count();

            var roomsDTO = query.Skip((page - 1) * pageSize).Take(pageSize).Select(Room.ToDTO).ToList();

            return new ListDataResponse<RoomDTO>(count, roomsDTO);
        }
    }
}
