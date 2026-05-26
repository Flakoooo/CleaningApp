namespace CleaningAppWeb.Domain.Requests
{
    public class RatingApplicationRequest
    {
        public Guid ApplicationId { get; set; }
        public byte Rating { get; set; } = 0;
        public string? Feedback { get; set; }
    }
}
