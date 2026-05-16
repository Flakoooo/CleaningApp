using CleaningAppWeb.Domain.DTOs;
using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.CheckboxFilter
{
    public partial class CheckboxFilter<T>
    {
        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public List<T> AllValues { get; set; } = [];

        [Parameter]
        public HashSet<T> SelectedValues { get; set; } = [];

        [Parameter]
        public EventCallback<HashSet<T>> SelectedValuesChanged { get; set; }

        [Parameter]
        public EventCallback FilterChanged { get; set; }

        [Parameter]
        public Func<int, Task<ListDataResponse<T>>>? DataLoaderMethod { get; set; }

        private List<T> _values = [];

        protected override async Task OnInitializedAsync()
        {
            if (DataLoaderMethod is not null)
            {
                await LoadCheckboxDataAsync();
                MarkAsInitialized();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (DataLoaderMethod is not null)
            {
                ResetPagination();
                await LoadCheckboxDataAsync();
            }
            else
            {
                _values = AllValues.ToList();
            }
        }

        protected override async Task OnLoadMoreItemsAsync()
            => await LoadCheckboxDataAsync(append: true);

        protected override int GetCurrentItemsCount() => _values.Count;

        private async Task LoadCheckboxDataAsync(bool append = false)
        {
            if (DataLoaderMethod is not null)
            {
                await LoadDataAsync(
                    AllValues,
                    () => DataLoaderMethod.Invoke(_currentPage),
                    append: append
                );

                _values = AllValues.ToList();
            }
        }

        private async Task SelectValue(T value)
        {
            if (!SelectedValues.Add(value))
                SelectedValues.Remove(value);

            await SelectedValuesChanged.InvokeAsync(SelectedValues);
            await FilterChanged.InvokeAsync();
        }
    }
}
