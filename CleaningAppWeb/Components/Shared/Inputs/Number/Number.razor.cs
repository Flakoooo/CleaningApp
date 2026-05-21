using CleaningAppWeb.Components.Services;
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

        [Parameter]
        public int DebounceDelay { get; set; } = 0;

        private T? _value;
        private DebounceHelper? _searchDebounce;

        protected override void OnInitialized()
        {
            if (DebounceDelay > 0)
            {
                _searchDebounce = new DebounceHelper(DebounceDelay, async () =>
                {
                    await InvokeAsync(async () =>
                    {
                        await ValueHasChanged();
                        StateHasChanged();
                    });
                });
            }

            _value = Value;
            if (typeof(T).IsValueType && Value.HasValue)
            {
                if (MinValue.HasValue && Comparer<T>.Default.Compare(Value.Value, MinValue.Value) < 0)
                    Value = null;
                else if (MaxValue.HasValue && Comparer<T>.Default.Compare(Value.Value, MaxValue.Value) > 0)
                    Value = null;
            }
        }

        private async Task ValueHasChanged()
        {
            if (_value.HasValue)
            {
                if (MinValue.HasValue && Comparer<T>.Default.Compare(_value.Value, MinValue.Value) < 0)
                    _value = MinValue.Value;
                else if (MaxValue.HasValue && Comparer<T>.Default.Compare(_value.Value, MaxValue.Value) > 0)
                    _value = MaxValue.Value;
            }

            if (ValueChanged.HasDelegate)
                await ValueChanged.InvokeAsync(_value);
        }

        private async Task OnInputChanged(T? value)
        {
            _value = value;
            if (DebounceDelay > 0 && _searchDebounce is not null)
            {
                _searchDebounce.Trigger();
                return;
            }

            await ValueHasChanged();
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
