namespace CleaningAppWeb.Components.Models
{
    public class ValueSelectableItem<T>(T value, string displayText) : SelectableItem
    {
        public T Value { get; private set; } = value;
        public string DisplayText { get; set; } = displayText;

        public override string GetDisplayInfo() => DisplayText;

        public override object GetId() => Value ?? new object();
    }
}
