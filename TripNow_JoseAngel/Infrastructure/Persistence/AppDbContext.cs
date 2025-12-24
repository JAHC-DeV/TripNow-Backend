using Microsoft.EntityFrameworkCore;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;

namespace TripNow_JoseAngel.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura el enum Status para almacenarse como string en la BD
            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();
        }
    }
}
