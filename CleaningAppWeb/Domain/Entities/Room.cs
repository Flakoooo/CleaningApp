using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("rooms")]
    public class Room
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("office_id")]
        public required Guid OfficeId { get; set; }

        [Column("room_number")]
        public required short RoomNumber { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public virtual Office Office { get; set; } = null!;
    }
}
