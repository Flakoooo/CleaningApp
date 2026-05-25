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
        IDbContextFactory<AppDbContext> factory
    )
    {
        private readonly IDbContextFactory<AppDbContext> _factory = factory;
        private readonly AuthenticationStateProvider _provider = provider;

        public event Action<Guid, CleaningApplicationStatus>? OnApplicationStatusHasChanged;

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
            await using var dbContext = await _factory.CreateDbContextAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var currentUser = (await _provider.GetAuthenticationStateAsync()).User;
            if (!Guid.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return new ListDataResponse<CleaningApplicationListElement>(0, []);

            if (!Enum.TryParse(currentUser.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty, true, out RoleType userRole))
                return new ListDataResponse<CleaningApplicationListElement>(0, []);

            var query = dbContext.Applications
                .AsNoTracking()
                .Where(ca => userRole == RoleType.Officer
                    ? ca.InitiatorId == userId
                    : (ca.ExecutorId == userId || ca.ExecutorId == null));

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

            int count = await query.CountAsync();

            var items = await query
                .OrderByDescending(ca => ca.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ca => new CleaningApplicationListElement
                {
                    Id = ca.Id,
                    InitiatorId = ca.InitiatorId,
                    Executor = ca.Executor != null ? new UserDTO
                    {
                        Id = ca.Executor.Id,
                        FirstName = ca.Executor.FirstName,
                        LastName = ca.Executor.LastName,
                        Patronymic = ca.Executor.Patronymic
                    } : null,
                    Office = new OfficeDTO
                    {
                        Id = ca.Office.Id,
                        Name = ca.Office.Name,
                        Address = ca.Office.Address,
                    },
                    Status = ca.Status,
                    ClientFirstName = ca.ClientFirstName,
                    ClientLastName = ca.ClientLastName,
                    ClientPatronymic = ca.ClientPatronymic,
                    ClientTelephoneNumber = ca.ClientTelephoneNumber,
                    CleaningDate = ca.CleaningDate,
                    CleaningTime = ca.CleaningTime,
                    Comment = ca.Comment ?? string.Empty,
                    Rating = ca.Rating,
                    Feedback = ca.Feedback ?? string.Empty,
                    RoomsCount = ca.ApplicationRooms.Count,
                    ServicesCount = ca.ApplicationServices.Count
                })
                .ToListAsync();

            return new ListDataResponse<CleaningApplicationListElement>(count, items);
        }

        public async Task<CleaningApplicationDTO?> GetApplicationAsync(
            Guid applicationId
        )
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            var application = await dbContext.Applications
                .AsNoTracking()
                .Include(ca => ca.ApplicationRooms.Where(ar => ar.Room.IsActive))
                    .ThenInclude(ar => ar.Room)
                .Include(ca => ca.ApplicationServices.Where(aser => aser.Service.IsActive))
                    .ThenInclude(aser => aser.Service)
                .Include(ca => ca.Initiator)
                .Include(ca => ca.Executor)
                .Include(ca => ca.Office)
                .FirstOrDefaultAsync(ca => ca.Id == applicationId);
            if (application is null) return null;

            var roomsDTO = application.ApplicationRooms.Select(r => Room.ToDTO(r.Room)).ToList();

            var servicesDTO = application.ApplicationServices.Select(s => Service.ToDTO(s.Service)).ToList();

            return new CleaningApplicationDTO
            {
                Id = application.Id,
                InitiatorId = application.InitiatorId,
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

        public async Task<string> CreateNewApplicationAsync(CreateApplicationRequest request)
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            if (!await dbContext.Offices.AnyAsync(o => o.Id == request.OfficeId))
                return "Указанный офис в данный момент недоступен";

            if (await dbContext.Rooms.CountAsync(r => r.IsActive && request.Rooms.Contains(r.Id) && r.OfficeId == request.OfficeId) != request.Rooms.Count)
                return "Некоторые команты в данный момент недоступны";

            if (await dbContext.Services.CountAsync(s => s.IsActive && request.Services.Contains(s.Id)) != request.Services.Count)
                return "Некоторые услуги в данный момент недоступны";

            var currentDateTime = DateTime.Now;
            var currentDate = DateOnly.FromDateTime(currentDateTime);
            if (currentDate > request.CleaningDate) return "Нельзя выбрать прошедшую дату";
            else if (currentDate == request.CleaningDate && TimeOnly.FromDateTime(currentDateTime) > request.CleaningTime)
                return "Нельзя выбрать прошедшее время";

            var currentUser = (await _provider.GetAuthenticationStateAsync()).User;
            if (!Guid.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return "Ошибка авторизации";

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
                CleaningTime = request.CleaningTime,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment
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

            await dbContext.Applications.AddAsync(newApplication);
            await dbContext.ApplicationRooms.AddRangeAsync(applicationRooms);
            await dbContext.ApplicationServices.AddRangeAsync(applicationServices);

            await dbContext.SaveChangesAsync();

            return string.Empty;
        }

        public async Task<bool> UpdateApplicationStatus(Guid applicationId, CleaningApplicationStatus newStatus)
        {
            await using var dbContext = await _factory.CreateDbContextAsync();

            var currentUser = (await _provider.GetAuthenticationStateAsync()).User;
            if (!Guid.TryParse(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return false;

            if (!Enum.TryParse(currentUser.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty, true, out RoleType userRole))
                return false;

            var applicationForUpdate =  await dbContext.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
            if (applicationForUpdate is null) return false;

            if (newStatus is CleaningApplicationStatus.InWork && userRole is RoleType.Cleaner)
                applicationForUpdate.ExecutorId = userId;

            applicationForUpdate.Status = newStatus;

            await dbContext.SaveChangesAsync();

            OnApplicationStatusHasChanged?.Invoke(applicationId, newStatus);
            return true;
        }
    }
}
