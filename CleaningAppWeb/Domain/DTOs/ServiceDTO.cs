namespace CleaningAppWeb.Domain.DTOs
{
    public class ServiceDTO
    {
        public Guid Id { get; set; }
        public string CodeName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
