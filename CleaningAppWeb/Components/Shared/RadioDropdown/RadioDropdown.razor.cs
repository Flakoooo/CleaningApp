using CleaningAppWeb.Components.Services;
using CleaningAppWeb.Domain.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CleaningAppWeb.Components.Shared.RadioDropdown
{
    public partial class RadioDropdown<T>
    {
        [Parameter]
        public bool IsLoading { get; set; } = false;

        [Parameter]
        public string? Label { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = string.Empty;

        [Parameter]
        public T? Value { get; set; }

        [Parameter]
        public EventCallback<T?> ValueChanged { get; set; }

        [Parameter]
        public List<T> AllValues { get; set; } = [];

        [Parameter]
        public bool IsDisabled { get; set; } = false;

        [Parameter]
        public Func<int, string?, Task<ListDataResponse<T>>>? DataLoaderMethod { get; set; }

        [Parameter]
        public int DebounceDelay { get; set; } = 0;

        [Parameter]
        public string? ErrorMessage { get; set; }

        private bool _showError = false;

        private ElementReference _inputRef;
        private DebounceHelper? _searchDebounce;

        private bool _isOpen = false;

        private List<T> _values = [];
        private readonly string _radioGroupName = typeof(T).Name;
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
                        await LoadRadioDataAsync();
                        StateHasChanged();
                    });
                });
            }
            await LoadRadioDataAsync();
            MarkAsInitialized();
        }

        protected override void OnParametersSet()
        {
            _showError = !string.IsNullOrWhiteSpace(ErrorMessage);

            StateHasChanged();
        }

        protected override async Task OnLoadMoreItemsAsync()
            => await LoadRadioDataAsync(append: true);

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

        private async Task LoadRadioDataAsync(bool append = false)
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

        private async Task OpenDropdown()
        {
            if (!_isOpen)
            {
                _isOpen = true;
                await InvokeAsync(StateHasChanged);
            }
        }

        private void ToggleDropdown() => _isOpen = !_isOpen;

        [JSInvokable]
        public void CloseDropdown()
        {
            _isOpen = false;
            InvokeAsync(StateHasChanged);
        }

        private async Task OnSearch(string? value = null)
        {
            _searchText = (value ?? string.Empty).Trim();

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

        private async Task OnElementChange(T value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);
        }

        private bool IsItemSelected(T value)
        {
            if (Value is null) return false;

            return Value.Equals(value);
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
