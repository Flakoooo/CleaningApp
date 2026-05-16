using CleaningAppWeb.Domain.Enums;

namespace CleaningAppWeb.Domain.DTOs
{
    public class UserDTO
    {
        public required Guid Id { get; set; }
        public required string Login { get; set; }
        public RoleType Role { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;
    }
}
