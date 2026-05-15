using CleaningAppWeb.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Relations
{
    [Table("application_rooms")]
    public class ApplicationRoom
    {
        [Column("application_id")]
        public required Guid ApplicationId { get; set; }

        [Column("room_id")]
        public required Guid RoomId { get; set; }

        public virtual CleaningApplication Application { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
    }
}
