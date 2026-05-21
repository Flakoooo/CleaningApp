using CleaningAppWeb.Domain.Entities;
using CleaningAppWeb.Domain.Relations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleaningAppWeb.Data
{
    public class AppDbContext(
        DbContextOptions<AppDbContext> options
    ) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
    {
        public DbSet<Office> Offices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<CleaningApplication> Applications { get; set; }
        public DbSet<ApplicationRoom> ApplicationRooms { get; set; }
        public DbSet<ApplicationService> ApplicationServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Ignore(u => u.Email);
                entity.Ignore(u => u.NormalizedEmail);
                entity.Ignore(u => u.EmailConfirmed);
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.AccessFailedCount);
            });

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
                entity.Property(u => u.UserName).HasColumnName("login");
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
                entity.Property(u => u.NormalizedUserName).HasColumnName("normalized_login").IsRequired();
                entity.Property(u => u.SecurityStamp).HasColumnName("security_stamp").IsRequired();

                entity.Property(e => e.Role).HasConversion<string>();

                entity.HasIndex(u => u.UserName)
                      .HasDatabaseName("ix_users_login")
                      .IsUnique();
                entity.HasIndex(u => u.Role)
                      .HasDatabaseName("ix_users_role");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasOne(r => r.Office)
                      .WithMany(o => o.Rooms)
                      .HasForeignKey(r => r.OfficeId)
                      .HasConstraintName("fk_rooms_office")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => r.OfficeId)
                      .HasDatabaseName("ix_rooms_office_id");
                entity.HasIndex(r => r.IsActive)
                      .HasDatabaseName("ix_rooms_is_active");
            });

            modelBuilder.Entity<Service>()
                        .HasIndex(s => s.IsActive)
                        .HasDatabaseName("ix_services_is_active");

            modelBuilder.Entity<CleaningApplication>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<string>();

                entity.HasOne(ca => ca.Initiator)
                      .WithMany(u => u.InitiatedApplications)
                      .HasForeignKey(ca => ca.InitiatorId)
                      .HasConstraintName("fk_applications_initiator")
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(ca => ca.Executor)
                      .WithMany(u => u.ExecutedApplications)
                      .HasForeignKey(ca => ca.ExecutorId)
                      .HasConstraintName("fk_applications_executor")
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ca => ca.InitiatorId)
                      .HasDatabaseName("ix_applications_initiator_id");
                entity.HasIndex(ca => ca.ExecutorId)
                      .HasDatabaseName("ix_applications_executor_id");
                entity.HasIndex(ca => ca.OfficeId)
                      .HasDatabaseName("ix_applications_office_id");
                entity.HasIndex(ca => ca.Status)
                      .HasDatabaseName("ix_applications_status");
                entity.HasIndex(ca => ca.CleaningDate)
                      .HasDatabaseName("ix_applications_cleaning_date");

                entity.ToTable(tb => tb.HasCheckConstraint("ck_applications_rating_range", "rating >= 0 AND rating <= 5"));
            });

            modelBuilder.Entity<ApplicationRoom>()
                        .HasKey(ar => new { ar.ApplicationId, ar.RoomId });
            modelBuilder.Entity<ApplicationService>()
                        .HasKey(aps => new { aps.ApplicationId, aps.ServiceId });


            modelBuilder.Entity<Service>().HasData(
                new Service { CodeName = "Window", Name = "Мойка окон" },
                new Service { CodeName = "Wet", Name = "Влажная уборка" },
                new Service { CodeName = "Dry", Name = "Сухая уборка" },
                new Service { CodeName = "Carpet", Name = "Чистка ковров" },
                new Service { CodeName = "CarpetDry", Name = "Химчистка ковров" },
                new Service { CodeName = "Furniture", Name = "Чистка мебели" },
                new Service { CodeName = "Restroom", Name = "Уборка санузлов" },
                new Service { CodeName = "Kitchen", Name = "Уборка кухни/столовой" },
                new Service { CodeName = "TrashRemoval", Name = "Вынос мусора" },
                new Service { CodeName = "Disinfection", Name = "Дезинфекция поверхностей" },
                new Service { CodeName = "AfterEvent", Name = "Уборка после мероприятий" }
            );
        }
    }
}
