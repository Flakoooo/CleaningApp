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


        private readonly List<CleaningApplicationListElement> _applications = [];

        private List<ValueSelectableItem<TimeOnly>> _timeFilter = [];

        protected override async Task OnInitializedAsync()
        {
            for (int i = 9; i < 17; ++i)
            {
                _timeFilter.Add(new ValueSelectableItem<TimeOnly>(new TimeOnly(i, 0, 0), $"{i}:00"));
            }
        }

        protected override int GetCurrentItemsCount() => _applications.Count;

        protected override async Task OnLoadMoreItemsAsync()
            => await LoadApplicationsAsync(true);

        private async Task LoadApplicationsAsync(bool append = false) => await LoadDataAsync(
            _applications,
            () => ApplicationsService.GetApplicationsAsync(
                _currentPage,
                selectedStatuses: [CleaningApplicationStatus.Waiting, CleaningApplicationStatus.InWork]
            ),
            append
        );
    }
}
