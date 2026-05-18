using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.Inputs.Input
{
    public partial class Input
    {
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
        public string Value { get; set; } = string.Empty;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter]
        public string? ErrorMessage { get; set; }

        private bool _showError = false;

        protected override void OnParametersSet()
        {
            _showError = !string.IsNullOrWhiteSpace(ErrorMessage);

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
