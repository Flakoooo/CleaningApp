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

        protected override async Task OnInitializedAsync()
        {
            await LoadApplicationsAsync();
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
    }
}
