using Microsoft.EntityFrameworkCore;

namespace SmartParking.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Sprint 1
        public DbSet<User> Users { get; set; }

        // Sprint 2
        public DbSet<Parking> Parkings { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Spot> Spots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Conversion enum Role -> string
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Zone>()
                .HasOne(z => z.Parking)
                .WithMany(p => p.Zones)
                .HasForeignKey(z => z.ParkingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Spot>()
                .HasOne(s => s.Zone)
                .WithMany(z => z.Spots)
                .HasForeignKey(s => s.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}