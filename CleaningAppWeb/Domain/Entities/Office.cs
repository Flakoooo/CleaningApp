using CleaningAppWeb.Domain.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningAppWeb.Domain.Entities
{
    [Table("offices")]
    public class Office
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("address")]
        public required string Address { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Room> Rooms { get; set; } = [];


        public static OfficeDTO ToDTO(Office office) => new OfficeDTO
        {
            Id = office.Id,
            Address = office.Address,
            Name = office.Name
        };
    }
}
