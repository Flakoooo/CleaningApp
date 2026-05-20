using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using CleaningAppWeb.Domain.Relations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("applications")]
    public class CleaningApplication
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("initiator_id")]
        public required Guid InitiatorId { get; set; }

        [Column("executor_id")]
        public Guid? ExecutorId { get; set; }

        [Column("office_id")]
        public required Guid OfficeId { get; set; }

        [Column("status")]
        public required CleaningApplicationStatus Status { get; set; }

        [Column("client_first_name")]
        public string ClientFirstName { get; set; } = string.Empty;

        [Column("client_last_name")]
        public string ClientLastName { get; set; } = string.Empty;

        [Column("client_patronymic")]
        public string ClientPatronymic { get; set; } = string.Empty;

        [Column("client_telephone_number")]
        public string ClientTelephoneNumber { get; set; } = string.Empty;

        [Column("cleaning_date")]
        public required  DateOnly CleaningDate { get; set; }

        [Column("cleaning_time")]
        public required TimeOnly CleaningTime { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("rating")]
        public byte Rating { get; set; } = 0;

        [Column("feedback")]
        public string? Feedback { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        public virtual User Initiator { get; set; } = null!;
        public virtual User? Executor { get; set; }
        public virtual Office Office { get; set; } = null!;
        public virtual ICollection<ApplicationRoom> ApplicationRooms { get; set; } = [];
        public virtual ICollection<ApplicationService> ApplicationServices { get; set; } = [];


        public static CleaningApplicationListElement? ToListElement(CleaningApplication ca)
        {
            var initiatorDTO = User.ToDTO(ca.Initiator);
            if (initiatorDTO is null) return null;

            return new CleaningApplicationListElement
            {
                Id = ca.Id,
                Initiator = initiatorDTO,
                Executor = User.ToDTO(ca.Executor),
                Office = Office.ToDTO(ca.Office),
                Status = ca.Status,
                ClientFirstName = ca.ClientFirstName,
                ClientLastName = ca.ClientLastName,
                ClientPatronymic = ca.ClientPatronymic,
                ClientTelephoneNumber = ca.ClientTelephoneNumber,
                CleaningDate = ca.CleaningDate,
                CleaningTime = ca.CleaningTime,
                Comment = ca.Comment ?? string.Empty,
                Rating = ca.Rating,
                Feedback = ca.Feedback ?? string.Empty,
                RoomsCount = ca.ApplicationRooms.Count,
                ServicesCount = ca.ApplicationServices.Count
            };
        }
    }
}
