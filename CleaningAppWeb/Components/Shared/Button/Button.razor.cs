using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.Button
{
    public partial class Button
    {
        [Parameter]
        public bool InProgress { get; set; } = false;

        [Parameter]
        public bool IsMainButton { get; set; } = false;

        [Parameter]
        public EventCallback OnButtonClick { get; set; }

        [Parameter]
        public byte FontSize { get; set; } = 24;

        [Parameter]
        public string Text { get; set; } = string.Empty;

        [Parameter]
        public string LoadingText { get; set; } = string.Empty;

        [Parameter]
        public string? ButtonClass { get; set; }

        [Parameter]
        public string Style { get; set; } = string.Empty;

        [Parameter]
        public string Width { get; set; } = "width: 14vw;";

        private async Task ButtonClick() => await OnButtonClick.InvokeAsync();

        private string GetClass()
        {
            var classes = new List<string>();
            string baseClass = $"btn btn-{(IsMainButton ? "main" : "second")} d-flex";

            if (!string.IsNullOrWhiteSpace(ButtonClass))
                classes.Add(ButtonClass);

            if (classes.Count == 0)
                return baseClass;

            return $"{baseClass} {string.Join(" ", classes)}";
        }
    }
}
