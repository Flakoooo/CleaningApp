using CleaningAppWeb.Domain.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("services")]
    public class Service
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("code_name")]
        public required string CodeName { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;


        public static ServiceDTO ToDTO(Service service) => new()
        {
            Id = service.Id,
            Name = service.Name
        };
    }
}
