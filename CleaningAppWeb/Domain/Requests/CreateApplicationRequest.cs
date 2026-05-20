namespace CleaningAppWeb.Domain.Requests
{
    public class CreateApplicationRequest
    {
        public string ClientFirstName { get; set; } = string.Empty;
        public string ClientLastName { get; set; } = string.Empty;
        public string ClientPatronymic { get; set; } = string.Empty;
        public string ClientTelephoneNumber { get; set; } = string.Empty;
        public Guid OfficeId { get; set; }
        public HashSet<Guid> Rooms { get; set; } = [];
        public DateOnly CleaningDate { get; set; }
        public TimeOnly CleaningTime { get; set; }
        public string Comment { get; set; } = string.Empty;
        public HashSet<Guid> Services { get; set; } = [];
    }
}
