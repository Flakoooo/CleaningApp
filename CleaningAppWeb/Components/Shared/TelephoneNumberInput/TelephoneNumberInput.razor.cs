using CleaningAppWeb.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CleaningAppWeb.Components.Shared.TelephoneNumberInput
{
    public partial class TelephoneNumberInput : IDisposable
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Parameter]
        public string? Label { get; set; } = "Контактный номер";

        [Parameter]
        public string IconStyle { get; set; } = string.Empty;

        [Parameter]
        public string Placeholder { get; set; } = "Введите номер телефона";

        [Parameter]
        public bool IsDisabled { get; set; } = false;

        [Parameter]
        public string Value { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public string? ErrorMessage { get; set; }

        private bool _showError = false;
        private ElementReference _phoneInputElement;
        private DotNetObjectReference<TelephoneNumberInput>? _dotNetRef;

        private string _rawDigits = string.Empty;
        private string _phoneInput = string.Empty;
        private bool _jsInitialized = false;
        private string _previousValue = string.Empty;

        protected override void OnParametersSet()
        {
            _showError = string.IsNullOrWhiteSpace(_phoneInput) &&!string.IsNullOrWhiteSpace(ErrorMessage);

            var normalized = PhoneHelper.NormalizeRawDigits(Value);
            if (normalized != _previousValue)
            {
                _previousValue = normalized;
                _rawDigits = normalized;
                _phoneInput = PhoneHelper.FormatPhoneFromRaw(_rawDigits);
                if (_jsInitialized)
                {
                    _ = UpdatePhoneFieldInJs();
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("initPhoneMask", _dotNetRef, _phoneInputElement);
                _jsInitialized = true;

                if (!string.IsNullOrWhiteSpace(_rawDigits))
                {
                    await JSRuntime.InvokeVoidAsync("setPhoneValue", _phoneInputElement, _rawDigits);
                }
            }
        }

        private async Task UpdatePhoneFieldInJs()
        {
            if (_jsInitialized)
                await JSRuntime.InvokeVoidAsync("setPhoneValue", _phoneInputElement, _rawDigits);
        }

        [JSInvokable]
        public async Task UpdateRawDigits(string digits)
        {
            if (digits == _rawDigits) return;

            _rawDigits = digits;
            _phoneInput = PhoneHelper.FormatPhoneFromRaw(digits);

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(digits);
        }

        private void HandleInput(ChangeEventArgs e) { }

        public void Dispose()
        {
            _dotNetRef?.Dispose();
        }
    }
}
