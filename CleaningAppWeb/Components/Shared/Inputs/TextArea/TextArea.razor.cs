using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CleaningAppWeb.Components.Shared.Inputs.TextArea
{
    public partial class TextArea
    {
        [Parameter]
        public bool IsLoading { get; set; } = false;

        [Parameter]
        public string? Label { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = string.Empty;

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

        [Parameter]
        public string Style { get; set; } = string.Empty;

        [Parameter]
        public bool? IsDisabled { get; set; }

        private bool _showError = false;

        protected override void OnParametersSet()
        {
            if (NeedValidation)
            {
                if (CustomValidation is not null)
                {
                    //var result = CustomValidation(Value);
                    //ErrorMessage = result.Message;
                    //_showError = !result.IsValid;
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
                attributes["disabled"] = "disabled";

            return attributes;
        }

        private async Task OnInputChanged(ChangeEventArgs e)
        {
            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(e.Value?.ToString());
        }
    }
}
