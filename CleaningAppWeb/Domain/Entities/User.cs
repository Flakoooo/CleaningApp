using CleaningAppWeb.Domain.DTOs;
using CleaningAppWeb.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("users")]
    public class User : IdentityUser<Guid>
    {
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
            string userName, RoleType role,
            string firstName, string lastName, string patronymic,
            string telephoneNumber,
            Guid? id = null
        ) => new User
        {
            Id = id ?? Guid.NewGuid(),
            UserName = userName,
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            Patronymic = patronymic,
            TelephoneNumber = telephoneNumber
        };

        public static UserDTO? ToDTO(User? user)
        {
            if (user is null || user.UserName is null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Login = user.UserName,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Patronymic = user.Patronymic,
                TelephoneNumber = user.TelephoneNumber
            };
        }

    }
}
