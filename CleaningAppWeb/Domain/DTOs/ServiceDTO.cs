using CleaningAppWeb.Components.Models;
using System.Net;

namespace CleaningAppWeb.Domain.DTOs
{
    public class ServiceDTO : SelectableItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string GetDisplayInfo() => Name;
        public override object GetId() => Id;
    }
}
