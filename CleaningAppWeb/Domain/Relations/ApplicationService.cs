using CleaningAppWeb.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Relations
{
    [Table("application_services")]
    public class ApplicationService
    {
        [Column("application_id")]
        public required Guid ApplicationId { get; set; }

        [Column("service_id")]
        public required Guid ServiceId { get; set; }

        public virtual CleaningApplication Application { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
