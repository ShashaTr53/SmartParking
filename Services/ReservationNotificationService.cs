using Microsoft.EntityFrameworkCore;
using SmartParking.Models;

namespace SmartParking.Services
{
    public class ReservationNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReservationNotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.Now;

                var overdueReservations = await context.Reservations
                    .Where(r =>
                        r.PlannedEndTime < now &&
                        (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Active))
                    .ToListAsync(stoppingToken);

                foreach (var reservation in overdueReservations)
                {
                    bool alreadyNotified = await context.Notifications
                        .AnyAsync(n =>
                            n.ReservationId == reservation.Id &&
                            n.Type == "ReservationOverdue",
                            stoppingToken);

                    if (!alreadyNotified)
                    {
                        var notification = new Notification
                        {
                            UserId = reservation.UserId,
                            ReservationId = reservation.Id,
                            Title = "Temps de réservation dépassé",
                            Message = $"Votre réservation numéro {reservation.Id} a dépassé le temps prévu.",
                            Type = "ReservationOverdue",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        };

                        context.Notifications.Add(notification);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
