using CleaningAppWeb.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.DTOs
{
    public class CleaningApplicationDTO
    {
        public required Guid Id { get; set; }
        public required UserDTO Initiator { get; set; }
        public UserDTO? Executor { get; set; }
        public OfficeDTO? Office { get; set; }
        public CleaningApplicationStatus Status { get; set; }
        public string ClientFirstName { get; set; } = string.Empty;
        public string ClientLastName { get; set; } = string.Empty;
        public string ClientPatronymic { get; set; } = string.Empty;
        public string ClientTelephoneNumber { get; set; } = string.Empty;
        public required DateOnly CleaningDate { get; set; }
        public required TimeOnly CleaningTime { get; set; }
        public string Comment { get; set; } = string.Empty;
        public byte Rating { get; set; } = 0;
        public string Feedback { get; set; } = string.Empty;

    }
}
