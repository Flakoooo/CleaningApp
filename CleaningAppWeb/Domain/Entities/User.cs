using CleaningAppWeb.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("login")]
        public required string Login { get; set; }

        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Column("role")]
        public required RoleType Role { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("patronymic")]
        public string Patronymic { get; set; } = string.Empty;

        [Column("telephone_number")]
        public string TelephoneNumber { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CleaningApplication> InitiatedApplications { get; set; } = [];
        public virtual ICollection<CleaningApplication> ExecutedApplications { get; set; } = [];


        public static User Create(
            string login, string password, RoleType role,
            string firstName, string lastName, string patronymic,
            string telephoneNumber,
            Guid? id = null
        ) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Login = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            Patronymic = patronymic,
            TelephoneNumber = telephoneNumber
        };
    }
}
