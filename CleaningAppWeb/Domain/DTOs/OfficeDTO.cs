using CleaningAppWeb.Components.Models;

namespace CleaningAppWeb.Domain.DTOs
{
    public class OfficeDTO : SelectableItem
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public override string GetDisplayInfo() => Address;
        public override object GetId() => Id;
    }
}
