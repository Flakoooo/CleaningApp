using CleaningAppWeb.Data.Services;
using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.OfficerApplicationsCard
{
    public partial class OfficerApplicationsCard
    {
        [Inject]
        private CleaningApplicationsService ApplicationsService { get; set; } = null!;

        [Parameter]
        public string Label { get; set; } = "Заявки";

        [Parameter]
        public HashSet<CleaningApplicationStatus> Statuses { get; set; } = [];

        private readonly List<CleaningApplicationListElement> _applications = [];

        private bool _applicationModalActive = false;
        private Guid? _selectedApplicationId = null;

        protected override async Task OnInitializedAsync()
        {
            ApplicationsService.OnApplicationStatusHasChanged += ApplicationStatusHasCnhaged;

            await LoadApplicationsAsync();
            MarkAsInitialized();
        }

        protected override int GetCurrentItemsCount() => _applications.Count;

        protected override async Task OnLoadMoreItemsAsync() 
            => await LoadApplicationsAsync(true);

        private async Task LoadApplicationsAsync(bool append = false) => await LoadDataAsync(
            _applications,
            () => ApplicationsService.GetApplicationsAsync(
                _currentPage,
                selectedStatuses: Statuses
            ),
            append
        );

        private void SelectApplication(Guid id)
        {
            _selectedApplicationId = id;
            _applicationModalActive = true;
            StateHasChanged();
        }

        private void ModalClose()
        {
            _applicationModalActive = false;
            _selectedApplicationId = null;
            StateHasChanged();
        }

        private void ApplicationStatusHasCnhaged(Guid applicationId, CleaningApplicationStatus newStatus)
        {
            var applicationForUpdate = _applications.FirstOrDefault(a => a.Id == applicationId);
            if (applicationForUpdate is null) return;

            applicationForUpdate.Status = newStatus;
            StateHasChanged();
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            ApplicationsService.OnApplicationStatusHasChanged -= ApplicationStatusHasCnhaged;

            await ValueTask.CompletedTask;
        }
    }
}
