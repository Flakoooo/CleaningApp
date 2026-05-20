using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Entities;
using CleaningAppWeb.Domain.Enums;
using CleaningAppWeb.Domain.Relations;
using CleaningAppWeb.Domain.Requests;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CleaningAppWeb.Data.Services
{
    public class CleaningApplicationsService(
        AuthenticationStateProvider provider,
        AppDbContext appDbContext
    )
    {
        private readonly AppDbContext _appDbContext = appDbContext;
        private readonly AuthenticationStateProvider _provider = provider;

        public async Task<ListDataResponse<CleaningApplicationListElement>> GetApplicationsAsync(
            int page,
            int pageSize = 20,
            HashSet<CleaningApplicationStatus>? selectedStatuses = null,
            HashSet<Guid>? selectedServices = null,
            int? roomsCount = null,
            HashSet<Guid>? selectedOffices = null,
            HashSet<TimeOnly>? selectedTime = null
        )
        {
            var currentUser = (await _provider.GetAuthenticationStateAsync()).User;
            if (!Guid.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return new ListDataResponse<CleaningApplicationListElement>(0, []);

            if (!Enum.TryParse(currentUser.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty, true, out RoleType userRole))
                return new ListDataResponse<CleaningApplicationListElement>(0, []);

            var query = _appDbContext.Applications.Where(ca => userRole == RoleType.Officer 
                ? ca.InitiatorId == userId 
                : (ca.ExecutorId == userId || ca.ExecutorId == null)
            );

            if (selectedStatuses?.Count > 0)
                query = query.Where(ca => selectedStatuses.Contains(ca.Status));

            if (selectedServices?.Count > 0)
                query = query.Where(ca => ca.ApplicationServices.Any(s => selectedServices.Contains(s.ServiceId)));

            if (roomsCount > 0)
                query = query.Where(ca => ca.ApplicationRooms.Count == roomsCount);

            if (selectedOffices?.Count > 0)
                query = query.Where(ca => selectedOffices.Contains(ca.OfficeId));

            if (selectedTime?.Count > 0)
                query = query.Where(ca => selectedTime.Contains(ca.CleaningTime));

            int count = query.Count();

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var queryDTO = new List<CleaningApplicationListElement>();
            foreach (var ca in query)
            {
                var application = CleaningApplication.ToListElement(ca);
                if (application is not null) queryDTO.Add(application);
            }

            return new ListDataResponse<CleaningApplicationListElement>(count, queryDTO);
        }

        public async Task<CleaningApplicationDTO?> GetApplicationAsync(
            Guid applicationdId
        )
        {
            var application = await _appDbContext.Applications.FirstOrDefaultAsync(ca => ca.Id == applicationdId);
            if (application is null) return null;

            var applicationsRoomsIds = application.ApplicationRooms.Select(ar => ar.RoomId).ToHashSet();
            var roomsDTO = await _appDbContext.Rooms.Where(r => r.IsActive && applicationsRoomsIds.Contains(r.Id))
                                                    .Select(r => Room.ToDTO(r))
                                                    .ToListAsync();

            var applicationsServicesIds = application.ApplicationServices.Select(aser => aser.ServiceId).ToHashSet();
            var servicesDTO = await _appDbContext.Services.Where(s => s.IsActive && applicationsServicesIds.Contains(s.Id))
                                                          .Select(s => Service.ToDTO(s))
                                                          .ToListAsync();

            var initiatorDTO = User.ToDTO(application.Initiator);
            if (initiatorDTO is null) return null;

            return new CleaningApplicationDTO
            {
                Id = application.Id,
                Initiator = initiatorDTO,
                Executor = User.ToDTO(application.Executor),
                Office = Office.ToDTO(application.Office),
                Status = application.Status,
                ClientFirstName = application.ClientFirstName,
                ClientLastName = application.ClientLastName,
                ClientPatronymic = application.ClientPatronymic,
                ClientTelephoneNumber = application.ClientTelephoneNumber,
                CleaningDate = application.CleaningDate,
                CleaningTime = application.CleaningTime,
                Comment = application.Comment ?? string.Empty,
                Rating = application.Rating,
                Feedback = application.Feedback ?? string.Empty,
                Rooms = roomsDTO,
                Services = servicesDTO
            };
        }

        public async Task<bool> CreateNewAppliacationAsync(CreateApplicationRequest request)
        {
            //если указанный офис не существует
            if (!await _appDbContext.Offices.AnyAsync(o => o.Id == request.OfficeId))
                return false;

            //если количество комнат, совпадающих, не соотвествуют количеству выбранных
            if (await _appDbContext.Rooms.CountAsync(r => r.IsActive && request.Rooms.Contains(r.Id)) != request.Rooms.Count)
                return false;

            //если указанные комнаты не соотвествуют с офисом
            if (!await _appDbContext.Rooms.AllAsync(r => r.IsActive && request.Rooms.Contains(r.Id) && r.OfficeId == request.OfficeId))
                return false;

            //если количество услуг, совпадающих, не соотвествуют количеству выбранных
            if (await _appDbContext.Services.CountAsync(s => s.IsActive && request.Services.Contains(s.Id)) != request.Services.Count)
                return false;

            var currentDateTime = DateTime.Now;
            var currentDate = DateOnly.FromDateTime(currentDateTime);
            if (currentDate < request.CleaningDate) return false;
            else if (currentDate == request.CleaningDate && TimeOnly.FromDateTime(currentDateTime) > request.CleaningTime)
                return false;

            var currentUser = (await _provider.GetAuthenticationStateAsync()).User;
            if (!Guid.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return false;

            var newApplication = new CleaningApplication
            {
                InitiatorId = userId,
                OfficeId = request.OfficeId,
                Status = CleaningApplicationStatus.Waiting,
                ClientFirstName = request.ClientFirstName,
                ClientLastName = request.ClientLastName,
                ClientPatronymic = request.ClientPatronymic,
                ClientTelephoneNumber = request.ClientTelephoneNumber,
                CleaningDate = request.CleaningDate,
                CleaningTime = request.CleaningTime
            };

            var applicationRooms = new List<ApplicationRoom>();
            foreach (var roomId in request.Rooms)
            {
                applicationRooms.Add(new ApplicationRoom 
                { 
                    ApplicationId = newApplication.Id, 
                    RoomId = roomId 
                });
            }

            var applicationServices = new List<ApplicationService>();
            foreach (var serviceId in request.Services)
            {
                applicationServices.Add(new ApplicationService
                {
                    ApplicationId = newApplication.Id,
                    ServiceId = serviceId
                });
            }

            await _appDbContext.Applications.AddAsync(newApplication);

            await _appDbContext.SaveChangesAsync();

            await _appDbContext.ApplicationRooms.AddRangeAsync(applicationRooms);
            await _appDbContext.ApplicationServices.AddRangeAsync(applicationServices);

            await _appDbContext.SaveChangesAsync();

            return true;
        }
    }
}
