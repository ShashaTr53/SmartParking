using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("verify/{sessionId}")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> VerifyPayment(int sessionId)
        {
            var session = await _context.PaymentSessions
                .Include(s => s.Spot)
                    .ThenInclude(sp => sp!.Zone!)
                        .ThenInclude(z => z.Parking!)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound("Payment session not found");

            if (session.Status == PaymentStatus.Paid)
                return BadRequest("Payment already verified");

            if (session.Status == PaymentStatus.Failed)
                return BadRequest("Payment failed");

            if (session.ExpiresAt < DateTime.Now)
            {
                session.Status = PaymentStatus.Failed;
                await _context.SaveChangesAsync();
                return BadRequest("Payment session expired");
            }

            if (session.Spot == null)
                return NotFound("Spot not found");

            if (session.Spot.Status != SpotStatus.Free)
                return BadRequest("Spot is no longer available");

            session.Status = PaymentStatus.Paid;

            var reservation = new Reservation
            {
                UserId = session.UserId,
                SpotId = session.SpotId,
                StartTime = session.StartTime,
                PlannedEndTime = session.StartTime.AddHours(2),
                InitialAmount = 3,
                ExtraAmount = 0,
                TotalAmount = 3,
                Status = ReservationStatus.Created
            };

            _context.Reservations.Add(reservation);

            session.Spot.Status = SpotStatus.Reserved;

            var qrToken = Guid.NewGuid().ToString();

            var qr = new QrTicket
            {
                Reservation = reservation,
                Token = qrToken
            };

            _context.QrTickets.Add(qr);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment successful and reservation confirmed",
                reservationId = reservation.Id,
                startTime = reservation.StartTime,
                plannedEndTime = reservation.PlannedEndTime,
                initialAmount = reservation.InitialAmount,
                extraAmount = reservation.ExtraAmount,
                totalAmount = reservation.TotalAmount,
                status = reservation.Status.ToString(),
                parkingName = session.Spot.Zone?.Parking?.Name ?? "",
                zoneName = session.Spot.Zone?.Name ?? "",
                spotCode = session.Spot.Code,
                qrToken = qrToken
            });
        }
    }
}
