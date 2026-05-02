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

        // Sprint 3
        public DbSet<Reservation> Reservations { get; set; }

        // Sprint 4
        public DbSet<PaymentSession> PaymentSessions { get; set; }
        public DbSet<QrTicket> QrTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<PaymentSession>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

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

            modelBuilder.Entity<PaymentSession>()
                .HasOne(p => p.Spot)
                .WithMany()
                .HasForeignKey(p => p.SpotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QrTicket>()
                .HasOne(q => q.Reservation)
                .WithOne()
                .HasForeignKey<QrTicket>(q => q.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}