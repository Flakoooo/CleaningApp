using CleaningAppWeb.Components.Models;
using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.CleanerApplicationsCard
{
    public partial class CleanerApplicationsCard
    {
        [Inject]
        private CleaningApplicationsService ApplicationsService { get; set; } = null!;

        [Inject]
        private CleaningServicesService CleaningServicesService { get; set; } = null!;

        [Inject]
        private OfficesService OfficesService { get; set; } = null!;

        [Parameter]
        public HashSet<CleaningApplicationStatus> Statuses { get; set; } = [];

        private bool _applicationModalActive = false;
        private Guid? _selectedApplicationId = null;

        private readonly List<CleaningApplicationListElement> _applications = [];

        private readonly List<ValueSelectableItem<CleaningApplicationStatus>> _filterStatuses = [];
        private HashSet<ValueSelectableItem<CleaningApplicationStatus>> _selectedStatuses = [];

        private List<ServiceDTO> _filterServices = [];
        private HashSet<ServiceDTO> _selectedServices = [];

        private int? _roomsCount;

        private HashSet<OfficeDTO> _selectedOffices = [];

        private readonly List<ValueSelectableItem<TimeOnly>> _filterTime = [];
        private HashSet<ValueSelectableItem<TimeOnly>> _selectedTimes = [];

        protected override async Task OnInitializedAsync()
        {
            ApplicationsService.OnApplicationStatusHasChanged += ApplicationStatusHasCnhaged;

            foreach (var status in Statuses)
                _filterStatuses.Add(new ValueSelectableItem<CleaningApplicationStatus>(status, GetStatusTranslation(status)));

            _filterServices = await CleaningServicesService.GetAvailableServices();

            for (int i = 9; i < 17; ++i)
                _filterTime.Add(new ValueSelectableItem<TimeOnly>(new TimeOnly(i, 0, 0), $"{i}:00"));

            await LoadApplicationsAsync();
            MarkAsInitialized();
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
                    selectedStatuses: statuses.Count > 0 ? statuses : Statuses,
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

        private void SelectApplication(Guid id)
        {
            _selectedApplicationId = id;
            _applicationModalActive = true;
        }

        private void ModalClose()
        {
            _applicationModalActive = false;
            _selectedApplicationId = null;
        }

        private void ApplicationStatusHasCnhaged(Guid applicationId, CleaningApplicationStatus newStatus)
        {
            var applicationForUpdate = _applications.FirstOrDefault(a => a.Id == applicationId);
            if (applicationForUpdate is null) return;

            if (newStatus is CleaningApplicationStatus.Waiting or CleaningApplicationStatus.InWork)
                applicationForUpdate.Status = newStatus;
            else
                _applications.Remove(applicationForUpdate);

            StateHasChanged();
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            ApplicationsService.OnApplicationStatusHasChanged -= ApplicationStatusHasCnhaged;

            await ValueTask.CompletedTask;
        }
    }
}
