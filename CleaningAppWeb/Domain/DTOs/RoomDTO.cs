using CleaningAppWeb.Components.Models;

namespace CleaningAppWeb.Domain.DTOs
{
    public class RoomDTO : SelectableItem
    {
        public Guid Id { get; set; }
        public short RoomNumber { get; set; }

        public override string GetDisplayInfo() => RoomNumber.ToString();
        public override object GetId() => Id;
    }
}
