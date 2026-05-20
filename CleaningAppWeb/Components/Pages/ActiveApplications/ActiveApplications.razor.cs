using CleaningAppWeb.Components.Models;
using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Pages.ActiveApplications
{
    public partial class ActiveApplications
    {
        [Inject]
        private CleaningApplicationsService ApplicationsService { get; set; } = null!;

        [Inject]
        private CleaningServicesService CleaningServicesService { get; set; } = null!;

        [Inject]
        private OfficesService OfficesService { get; set; } = null!;

        [Parameter]
        public HashSet<CleaningApplicationStatus> Statuses { get; set; } = [];


        private readonly List<CleaningApplicationListElement> _applications = [];

        private List<ValueSelectableItem<CleaningApplicationStatus>> _filterStatuses = [];
        private HashSet<ValueSelectableItem<CleaningApplicationStatus>> _selectedStatuses = [];

        private List<ServiceDTO> _filterServices = [];
        private HashSet<ServiceDTO> _selectedServices = [];

        private int? _roomsCount;

        private HashSet<OfficeDTO> _selectedOffices = [];

        private List<ValueSelectableItem<TimeOnly>> _filterTime = [];
        private HashSet<ValueSelectableItem<TimeOnly>> _selectedTimes = [];

        protected override async Task OnInitializedAsync()
        {
            _filterStatuses = 
            [
                new ValueSelectableItem<CleaningApplicationStatus>(CleaningApplicationStatus.Waiting, GetStatusTranslation(CleaningApplicationStatus.Waiting)),
                new ValueSelectableItem<CleaningApplicationStatus>(CleaningApplicationStatus.InWork, GetStatusTranslation(CleaningApplicationStatus.InWork))
            ];

            _filterServices = await CleaningServicesService.GetAvailableServices();

            for (int i = 9; i < 17; ++i)
                _filterTime.Add(new ValueSelectableItem<TimeOnly>(new TimeOnly(i, 0, 0), $"{i}:00"));

            await LoadApplicationsAsync();
        }

        private static string GetStatusTranslation(CleaningApplicationStatus status) => status switch
        {
            CleaningApplicationStatus.Waiting => "Ожидание",
            CleaningApplicationStatus.InWork => "В работе",
            CleaningApplicationStatus.Done => "Выполнена",
            CleaningApplicationStatus.Rated => "Оценена",
            _ => status.ToString()
        };

        protected override int GetCurrentItemsCount() => _applications.Count;

        protected override async Task OnLoadMoreItemsAsync()
            => await LoadApplicationsAsync(true);

        private async Task LoadApplicationsAsync(bool append = false)
        {
            var statuses = _selectedStatuses.Select(s => s.Value).ToHashSet();
            await LoadDataAsync(
                _applications,
                () => ApplicationsService.GetApplicationsAsync(
                    _currentPage,
                    selectedStatuses: statuses.Count > 0 ? statuses : [CleaningApplicationStatus.Waiting, CleaningApplicationStatus.InWork],
                    selectedOffices: _selectedOffices.Select(o => o.Id).ToHashSet(),
                    selectedServices: _selectedServices.Select(s => s.Id).ToHashSet(),
                    roomsCount: _roomsCount,
                    selectedTime: _selectedTimes.Select(t => t.Value).ToHashSet()
                ),
                append
            );
        }

        private async Task SetRoomsCount(int? value)
        {
            _roomsCount = value;
            await LoadApplicationsAsync();
        }

        private void ResetFilters()
        {
            _selectedStatuses.Clear();
            _selectedServices.Clear();
            _roomsCount = null;
            _selectedOffices.Clear();
            _selectedTimes.Clear();

            StateHasChanged();
        }
    }
}
