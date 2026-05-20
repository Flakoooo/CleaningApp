using CleaningAppWeb.Components.Models;
using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;

namespace CleaningAppWeb.Components.Pages.NewApplication
{
    public partial class NewApplication
    {
        [Inject]
        private AuthenticationStateProvider AuthProvider { get; set; } = null!;

        [Inject]
        private OfficesService OfficesService { get; set; } = null!;

        [Inject]
        private RoomsService RoomsService { get; set; } = null!;

        [Inject]
        private CleaningServicesService CleaningServicesService { get; set; } = null!;

        [Inject]
        private CleaningApplicationsService CleaningApplicationsService { get; set; } = null!;

        private CreateApplicationRequest CreateRequest { get; set; } = new();

        private readonly Dictionary<string, string> _errors = [];

        private bool _setUserData = false;
        private string _userFirstName = string.Empty;
        private string _userLastName = string.Empty;
        private string _userPatronymicName = string.Empty;
        private string _userTelephoneNumber = string.Empty;

        private OfficeDTO? _selectedOffice;

        private HashSet<RoomDTO> _selectedRooms = [];

        private string _selectedDateString = string.Empty;

        private List<ValueSelectableItem<TimeOnly>> _availableTime = [];
        private ValueSelectableItem<TimeOnly>? _selectedTime;

        private List<ServiceDTO> _availableServices = [];
        private readonly HashSet<Guid> _selectedServices = [];

        private string? _createTextResult;

        protected override async Task OnInitializedAsync()
        {
            _availableServices = await CleaningServicesService.GetAvailableServices();
            _availableTime = [new ValueSelectableItem<TimeOnly>(new TimeOnly(), "Сперва выберите дату")];

            var user = (await AuthProvider.GetAuthenticationStateAsync()).User;

            _userFirstName = user.FindFirst("FirstName")?.Value ?? string.Empty;
            _userLastName = user.FindFirst("LastName")?.Value ?? string.Empty;
            _userPatronymicName = user.FindFirst("Patronymic")?.Value ?? string.Empty;
            _userTelephoneNumber = user.FindFirst("TelephoneNumber")?.Value ?? string.Empty;
        }

        private void SetError(string key, string value)
        {
            if (_errors.TryGetValue(key, out var _)) 
                _errors[key] = value;
            else 
                _errors.Add(key, value);
        }

        private void SetUserData()
        {
            if (string.IsNullOrWhiteSpace(_userFirstName)
                || string.IsNullOrWhiteSpace(_userLastName)
                || string.IsNullOrWhiteSpace(_userPatronymicName)
                || string.IsNullOrWhiteSpace(_userTelephoneNumber)
            ) return;

            _setUserData = !_setUserData;
            if (_setUserData && _errors.Count > 0)
            {
                _errors.Remove("firstName");
                _errors.Remove("lastName");
                _errors.Remove("patronymic");
                _errors.Remove("telephoneNumber");
            }
        }

        private void SelectOffice(OfficeDTO officeDTO)
        {
            _selectedOffice = officeDTO;
            string errorOffice = "office";
            if (_selectedOffice is not null && _errors.TryGetValue(errorOffice, out var _))
                _errors.Remove(errorOffice);
        }

        private void SelectService(Guid serviceId)
        {
            if (!_selectedServices.Add(serviceId))
                _selectedServices.Remove(serviceId);

            string errorServicesKey = "services";
            if (_selectedServices.Count > 0 && _errors.TryGetValue(errorServicesKey, out var _))
                _errors.Remove(errorServicesKey);
        }

        private void SelectDate(string date)
        {
            string errorOfficeKey = "date";
            string errorText = string.Empty;
            _selectedDateString = date;
            if (DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly newDate))
            {
                CreateRequest.CleaningDate = newDate;
                if (CreateRequest.CleaningDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    errorText = "Выбранная дата не может быть раньше текущей";
                }
            }
            else
            {
                errorText = "Выбранная дата имеет неверный формат";
            }

            if (!string.IsNullOrWhiteSpace(errorText))
            {
                SetError(errorOfficeKey, errorText);

                _availableTime = [new ValueSelectableItem<TimeOnly>(new TimeOnly(), errorText)];
                return;
            }
            else
            {
                if (_errors.TryGetValue(errorOfficeKey, out var _))
                    _errors.Remove(errorOfficeKey);
            }

            GetAvailableTime();
        }

        private void SelectRooms(HashSet<RoomDTO> rooms)
        {
            _selectedRooms = rooms;
            string errorRooms = "rooms";
            if (_selectedRooms.Count > 0 && _errors.TryGetValue(errorRooms, out var _))
                _errors.Remove(errorRooms);
        }

        private void GetAvailableTime()
        {
            var currentDateTime = DateTime.Now;

            bool selectCurrentDay = DateOnly.FromDateTime(currentDateTime) == CreateRequest.CleaningDate;

            var newAvailableTimes = new List<ValueSelectableItem<TimeOnly>>();
            for (int i = 9; i < 17; ++i)
            {
                if (selectCurrentDay && currentDateTime.Hour >= i) continue;

                newAvailableTimes.Add(new ValueSelectableItem<TimeOnly>(new TimeOnly(i, 0, 0), $"{i}:00"));
            }

            if (newAvailableTimes.Count == 0)
            {
                string errorText = "На данную дату нет доступного времени";
                SetError("time", errorText);
                _selectedTime = new ValueSelectableItem<TimeOnly>(new TimeOnly(), errorText);
                newAvailableTimes.Add(_selectedTime);
            }

            _availableTime = newAvailableTimes;
        }

        private void SelectTime(ValueSelectableItem<TimeOnly> value)
        {
            string errorTimeKey = "time";
            if (value.Value.Hour is < 9 or > 16)
            {
                SetError(errorTimeKey, "Выбранное время выходит за рамки доступного");
                return;
            }

            _selectedTime = value;
            if (_errors.TryGetValue(errorTimeKey, out var _))
                _errors.Remove(errorTimeKey);
        }

        private async Task CreateApplication()
        {
            _errors.Clear();

            string errorUserData;
            if (_setUserData)
            {
                errorUserData = "Поле не может быть пустым, заполните поля самостоятельно";
                if (string.IsNullOrWhiteSpace(_userFirstName))
                    SetError("firstName", errorUserData);

                if (string.IsNullOrWhiteSpace(_userLastName))
                    SetError("lastName", errorUserData);

                if (string.IsNullOrWhiteSpace(_userPatronymicName))
                    SetError("patronymic", errorUserData);

                if (string.IsNullOrWhiteSpace(_userTelephoneNumber))
                    SetError("telephoneNumber", errorUserData);
            }
            else
            {
                errorUserData = "Поле не может быть пустым";
                if (string.IsNullOrWhiteSpace(CreateRequest.ClientFirstName))
                    SetError("firstName", errorUserData);

                if (string.IsNullOrWhiteSpace(CreateRequest.ClientLastName))
                    SetError("lastName", errorUserData);

                if (string.IsNullOrWhiteSpace(CreateRequest.ClientPatronymic))
                    SetError("patronymic", errorUserData);

                if (string.IsNullOrWhiteSpace(CreateRequest.ClientTelephoneNumber))
                    SetError("telephoneNumber", errorUserData);
            }

            if (_selectedOffice is null)
                SetError("office", "Необходимо выбрать офис");

            if (_selectedRooms.Count == 0)
                SetError("rooms", "Необходимо выбрать хотя бы одну комнату");

            string dateError = "date";
            if (CreateRequest.CleaningDate == DateOnly.MinValue)
                SetError(dateError, "Необходимо выбрать дату уборки");
            else if (CreateRequest.CleaningDate < DateOnly.FromDateTime(DateTime.Now))
                SetError(dateError, "Выбранная дата не может быть раньше текущей");

            string timeError = "time";
            if (_selectedTime is null)
                SetError(timeError, "Необходимо выбрать время уборки");
            else if (CreateRequest.CleaningDate == DateOnly.MinValue && DateTime.Now.Hour > _selectedTime.Value.Hour)
                SetError(timeError, "Выбранное время не может быть прошедшим");
            else if (_selectedTime.Value.Hour is < 9 or > 16)
                SetError(timeError, "Выбранное время выходит за рамки доступного");

            if (_selectedServices.Count == 0)
                SetError("services", "Необходимо выбрать хотя бы одну услугу");


            if (_errors.Count > 0)
                return;

            if (_setUserData)
            {
                CreateRequest.ClientFirstName = _userFirstName;
                CreateRequest.ClientLastName = _userLastName;
                CreateRequest.ClientPatronymic = _userPatronymicName;
                CreateRequest.ClientTelephoneNumber = _userTelephoneNumber;
            }
            CreateRequest.OfficeId = _selectedOffice!.Id;
            CreateRequest.Rooms = _selectedRooms.Select(r => r.Id).ToHashSet();
            CreateRequest.CleaningTime = _selectedTime!.Value;
            CreateRequest.Services = _selectedServices;

            var result = await CleaningApplicationsService.CreateNewApplicationAsync(CreateRequest);

            _createTextResult = !string.IsNullOrWhiteSpace(result) ? result : "Заявка успешно создана!";
        }
    }
}
