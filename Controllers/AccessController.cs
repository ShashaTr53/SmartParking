using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.DTOs;
using SmartParking.Models;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccessController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccessController(AppDbContext context)
        {
            _context = context;
        }

        // Scan du QR code à l'entrée par Manager/Admin
        [HttpPost("entry")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Entry([FromBody] ScanQrDto dto)
        {
            var qr = await _context.QrTickets
                .Include(q => q.Reservation!)
                    .ThenInclude(r => r.Spot)
                .FirstOrDefaultAsync(q => q.Token == dto.QrToken);

            if (qr == null)
                return BadRequest("QR invalide");

            var reservation = qr.Reservation;

            if (reservation == null)
                return NotFound("Reservation not found");

            // Vérifie que la réservation est prête pour l'entrée
            if (reservation.Status != ReservationStatus.Created)
                return BadRequest("Reservation is not ready for entry");

            if (reservation.Spot == null)
                return NotFound("Spot not found");

            // Vérifie que le spot est réservé
            if (reservation.Spot.Status != SpotStatus.Reserved)
                return BadRequest("Spot is not reserved");

            // Passage à l'état actif
            reservation.Status = ReservationStatus.Active;
            reservation.Spot.Status = SpotStatus.Occupied;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Entry accepted",
                reservationId = reservation.Id,
                status = reservation.Status.ToString(),
                spotStatus = reservation.Spot.Status.ToString()
            });
        }

        // Scan du QR code à la sortie par Manager/Admin
        [HttpPost("exit")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Exit([FromBody] ScanQrDto dto)
        {
            var qr = await _context.QrTickets
                .Include(q => q.Reservation!)
                    .ThenInclude(r => r.Spot)
                .FirstOrDefaultAsync(q => q.Token == dto.QrToken);

            if (qr == null)
                return BadRequest("QR invalide");

            var reservation = qr.Reservation;

            if (reservation == null)
                return NotFound("Reservation not found");

            // Vérifie que la réservation est en cours
            if (reservation.Status != ReservationStatus.Active)
                return BadRequest("Reservation is not active");

            if (reservation.Spot == null)
                return NotFound("Spot not found");

            // Heure de sortie (réelle ou simulée)
            var exitTime = dto.ScanTime ?? DateTime.Now;

            double extraAmount = 0;

            // Calcul du dépassement
            if (exitTime > reservation.PlannedEndTime)
            {
                var extraMinutes = (exitTime - reservation.PlannedEndTime).TotalMinutes;
                var extraHours = Math.Ceiling(extraMinutes / 60);

                // 1 DT par heure supplémentaire
                extraAmount = extraHours * 1;
            }

            // Mise à jour des montants
            reservation.ExtraAmount = extraAmount;
            reservation.TotalAmount = reservation.InitialAmount + extraAmount;

            // Si dépassement → paiement supplémentaire requis
            if (extraAmount > 0)
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Extra payment required",
                    reservationId = reservation.Id,
                    plannedEndTime = reservation.PlannedEndTime,
                    exitTime,
                    extraAmount,
                    totalAmount = reservation.TotalAmount
                });
            }

            // Sinon sortie normale
            reservation.Status = ReservationStatus.Completed;
            reservation.Spot.Status = SpotStatus.Free;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Exit accepted",
                reservationId = reservation.Id,
                extraAmount = reservation.ExtraAmount,
                totalAmount = reservation.TotalAmount,
                status = reservation.Status.ToString(),
                spotStatus = reservation.Spot.Status.ToString()
            });
        }

        // Confirmation du paiement supplémentaire
        [HttpPost("confirm-extra-payment/{reservationId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ConfirmExtraPayment(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Spot)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound("Reservation not found");

            // Vérifie que la réservation est toujours active
            if (reservation.Status != ReservationStatus.Active)
                return BadRequest("Reservation is not active");

            // Vérifie qu'il y a un paiement supplémentaire
            if (reservation.ExtraAmount <= 0)
                return BadRequest("No extra payment required");

            // Finalisation de la réservation
            reservation.Status = ReservationStatus.Completed;

            if (reservation.Spot != null)
                reservation.Spot.Status = SpotStatus.Free;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Extra payment confirmed, exit accepted",
                reservationId = reservation.Id,
                extraAmount = reservation.ExtraAmount,
                totalAmount = reservation.TotalAmount,
                status = reservation.Status.ToString(),
                spotStatus = reservation.Spot?.Status.ToString()
            });
        }
    }
}
