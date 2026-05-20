using Microsoft.AspNetCore.Components;

namespace CleaningAppWeb.Components.Shared.Inputs.Number
{
    public partial class Number<T> where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
    {
        [Parameter]
        public string? Label { get; set; }

        [Parameter]
        public T? MinValue { get; set; }

        [Parameter]
        public T? MaxValue { get; set; }

        [Parameter]
        public string Placeholder { get; set; } = string.Empty;

        [Parameter]
        public T? Value { get; set; }

        [Parameter]
        public EventCallback<T?> ValueChanged { get; set; }

        [Parameter]
        public bool? IsDisabled { get; set; }

        protected override void OnInitialized()
        {
            if (typeof(T).IsValueType && Value.HasValue)
            {
                if (MinValue.HasValue && Comparer<T>.Default.Compare(Value.Value, MinValue.Value) < 0)
                    Value = null;
                else if (MaxValue.HasValue && Comparer<T>.Default.Compare(Value.Value, MaxValue.Value) > 0)
                    Value = null;
            }
        }

        private async Task OnInputChanged(T? value)
        {
            if (value.HasValue)
            {
                if (MinValue.HasValue && Comparer<T>.Default.Compare(value.Value, MinValue.Value) < 0)
                    value = MinValue.Value;
                else if (MaxValue.HasValue && Comparer<T>.Default.Compare(value.Value, MaxValue.Value) > 0)
                    value = MaxValue.Value;
            }

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(value);
        }

        private Dictionary<string, object> GetInputAttributes()
        {
            var attributes = new Dictionary<string, object>();

            if (IsDisabled.HasValue && IsDisabled.Value)
                attributes["disabled"] = "disabled";

            return attributes;
        }
    }
}
