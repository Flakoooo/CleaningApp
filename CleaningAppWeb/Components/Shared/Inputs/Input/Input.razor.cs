using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.Inputs.Input
{
    public partial class Input
    {
        [Parameter]
        public bool IsLoading { get; set; } = false;

        [Parameter]
        public int Width { get; set; } = 100;

        [Parameter]
        public string? Label { get; set; }

        [Parameter]
        public string IconStyle { get; set; } = string.Empty;

        [Parameter]
        public InputType InputType { get; set; } = InputType.Text;

        [Parameter]
        public string Placeholder { get; set; } = string.Empty;

        [Parameter]
        public bool? IsDisabled { get; set; }

        [Parameter]
        public string? CustomClass { get; set; }

        [Parameter]
        public string Value { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public bool NeedValidation { get; set; } = false;

        [Parameter]
        public Func<string, bool>? CustomValidation { get; set; }

        [Parameter]
        public string? ErrorMessage { get; set; } = "Поле не заполнено";

        private bool _showError = false;
        private string _pendingValue = string.Empty;

        protected override void OnParametersSet()
        {
            if (NeedValidation)
            {
                if (CustomValidation is not null)
                {
                    // var result = CustomValidation(Value);
                    // ErrorMessage = result.Message;
                    // _showError = !result.IsValid;
                }
                else
                {
                    _showError = string.IsNullOrWhiteSpace(Value);
                }
            }
            else _showError = false;

            StateHasChanged();
        }

        private Dictionary<string, object> GetInputAttributes()
        {
            var attributes = new Dictionary<string, object>();

            if (IsDisabled.HasValue && IsDisabled.Value)
            {
                attributes["disabled"] = "disabled";
            }

            return attributes;
        }

        private async Task OnInputChanged(ChangeEventArgs e)
        {
            var newValue = e.Value?.ToString() ?? string.Empty;

            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(newValue);
            }
        }
    }
}
