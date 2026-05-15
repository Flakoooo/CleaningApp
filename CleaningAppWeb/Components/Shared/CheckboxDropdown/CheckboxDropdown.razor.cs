using CleaningAppWeb.Components.Services;
using CleaningAppWeb.Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CleaningAppWeb.Components.Shared.CheckboxDropdown
{
    public partial class CheckboxDropdown<T>
    {
        [Parameter]
        public bool IsLoading { get; set; } = false;

        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public string Placeholder { get; set; } = string.Empty;

        [Parameter]
        public bool CreateValueAllowed { get; set; } = false;

        [Parameter]
        public List<T> AllValues { get; set; } = [];

        [Parameter]
        public HashSet<T> SelectedValues { get; set; } = [];

        [Parameter]
        public EventCallback<HashSet<T>> SelectedValuesChanged { get; set; }

        [Parameter]
        public Func<int, string?, Task<ListDataResponse<T>>>? DataLoaderMethod { get; set; }

        [Parameter]
        public int DebounceDelay { get; set; } = 0;

        [Parameter]
        public bool? IsDisabled { get; set; }

        private ElementReference _inputRef;
        private DebounceHelper? _searchDebounce;

        private bool _isOpen = false;

        private List<T> _values = [];
        private string _searchText = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (DataLoaderMethod is not null && DebounceDelay > 0)
            {
                _searchDebounce = new DebounceHelper(DebounceDelay, async () =>
                {
                    await InvokeAsync(async () =>
                    {
                        ResetPagination();
                        await LoadCheckboxDataAsync();
                        StateHasChanged();
                    });
                });
            }
            await LoadCheckboxDataAsync();
            MarkAsInitialized();
        }

        protected override async Task OnLoadMoreItemsAsync()
            => await LoadCheckboxDataAsync(append: true);

        protected override int GetCurrentItemsCount() => _values.Count;

        protected override async Task AdditionalAfterRenderMethod()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("listDropdown.registerClickOutside", _inputRef, _dotNetHelper);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации clickOutside: {ex.Message}");
            }

            if (_values.Count == 0 && AllValues.Count > 0)
                _values = AllValues.ToList();
        }

        private async Task LoadCheckboxDataAsync(bool append = false)
        {
            if (DataLoaderMethod is not null)
            {
                await LoadDataAsync(
                    AllValues,
                    () => DataLoaderMethod.Invoke(_currentPage, _searchText),
                    append: append
                );

                _values = AllValues.ToList();
            }
        }

        private Dictionary<string, object> GetInputAttributes()
        {
            var attributes = new Dictionary<string, object>();

            if (IsDisabled.HasValue && IsDisabled.Value)
                attributes["disabled"] = "disabled";

            return attributes;
        }

        private async Task OpenDropdown()
        {
            if (!_isOpen) _isOpen = true;
        }

        private void ToggleDropdown() => _isOpen = !_isOpen;

        [JSInvokable]
        public void CloseDropdown()
        {
            _isOpen = false;
            InvokeAsync(StateHasChanged);
        }

        private async Task OnSearch(ChangeEventArgs e)
        {
            _searchText = (e.Value?.ToString() ?? string.Empty).Trim();

            if (DataLoaderMethod is not null && DebounceDelay > 0)
            {
                _searchDebounce?.Trigger();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_searchText))
                    _values = AllValues.ToList();
                else
                    _values = AllValues.Where(v => v.MatchesSearch(_searchText)).ToList();

                await InvokeAsync(StateHasChanged);
            }
        }

        private void OnInputKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Escape") _isOpen = false;
        }

        private async Task OnItemClick(T value)
        {
            if (!SelectedValues.Remove(value))
                SelectedValues.Add(value);

            await SelectedValuesChanged.InvokeAsync(SelectedValues);
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            _searchDebounce?.Dispose();
            try
            {
                if (_dotNetHelper != null)
                    await JSRuntime.InvokeVoidAsync("listDropdown.unregisterClickOutside", _inputRef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке RadioDropdown: {ex.Message}");
            }

            await ValueTask.CompletedTask;
        }
    }
}
