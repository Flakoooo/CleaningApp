using CleaningAppWeb.Domain.Enums;

namespace CleaningAppWeb.Domain.DTOs
{
    public class CleaningApplicationListElement
    {
        public required Guid Id { get; set; }
        public required Guid InitiatorId { get; set; }
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
        public int RoomsCount { get; set; }
        public int ServicesCount { get; set; }

        public string ShortClientFullName => $"{ClientLastName} {ClientFirstName.FirstOrDefault()} {ClientPatronymic.FirstOrDefault()}";
    }
}
