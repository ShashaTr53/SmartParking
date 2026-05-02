using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.DTOs;
using SmartParking.Models;
using System.Security.Claims;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ STEP 1: Start Reservation (PaymentSession فقط)
        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            var userId = GetCurrentUserId();

            var spot = await _context.Spots
                .FirstOrDefaultAsync(s => s.Id == dto.SpotId);

            if (spot == null)
                return NotFound("Spot not found");

            if (spot.Status != SpotStatus.Free)
                return BadRequest("Spot is not available");

            var session = new PaymentSession
            {
                UserId = userId,
                SpotId = spot.Id,
                StartTime = dto.StartTime,
                Amount = 3,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(15)
            };

            _context.PaymentSessions.Add(session);
            await _context.SaveChangesAsync();

            var paymentUrl = $"http://fake-flouci.com/pay/{session.Id}";

            return Ok(new
            {
                message = "Proceed to payment",
                paymentSessionId = session.Id,
                paymentUrl = paymentUrl,
                amount = session.Amount,
                startTime = session.StartTime,
                expiresAt = session.ExpiresAt
            });
        }

        // ✅ Driver Reservations
        [HttpGet("my")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<List<ReservationDto>>> GetMyReservations()
        {
            var userId = GetCurrentUserId();

            var data = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Spot!)
                    .ThenInclude(s => s.Zone!)
                        .ThenInclude(z => z.Parking!)
                .ToListAsync();

            var reservations = data.Select(r => new ReservationDto
            {
                Id = r.Id,
                StartTime = r.StartTime,
                PlannedEndTime = r.PlannedEndTime,
                InitialAmount = r.InitialAmount,
                ExtraAmount = r.ExtraAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString(),
                ParkingName = r.Spot?.Zone?.Parking?.Name ?? "",
                ZoneName = r.Spot?.Zone?.Name ?? "",
                SpotCode = r.Spot?.Code ?? ""
            }).ToList();

            return Ok(reservations);
        }

        // ✅ Admin / Manager
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<ReservationDto>>> GetAllReservations()
        {
            var data = await _context.Reservations
                .Include(r => r.Spot!)
                    .ThenInclude(s => s.Zone!)
                        .ThenInclude(z => z.Parking!)
                .ToListAsync();

            var reservations = data.Select(r => new ReservationDto
            {
                Id = r.Id,
                StartTime = r.StartTime,
                PlannedEndTime = r.PlannedEndTime,
                InitialAmount = r.InitialAmount,
                ExtraAmount = r.ExtraAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString(),
                ParkingName = r.Spot?.Zone?.Parking?.Name ?? "",
                ZoneName = r.Spot?.Zone?.Name ?? "",
                SpotCode = r.Spot?.Code ?? ""
            }).ToList();

            return Ok(reservations);
        }

        // ✅ Reservations by Parking
        [HttpGet("parking/{parkingId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<List<ReservationDto>>> GetReservationsByParking(int parkingId)
        {
            var data = await _context.Reservations
                .Include(r => r.Spot!)
                    .ThenInclude(s => s.Zone!)
                        .ThenInclude(z => z.Parking!)
                .Where(r => r.Spot != null &&
                            r.Spot.Zone != null &&
                            r.Spot.Zone.ParkingId == parkingId)
                .ToListAsync();

            var reservations = data.Select(r => new ReservationDto
            {
                Id = r.Id,
                StartTime = r.StartTime,
                PlannedEndTime = r.PlannedEndTime,
                InitialAmount = r.InitialAmount,
                ExtraAmount = r.ExtraAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString(),
                ParkingName = r.Spot?.Zone?.Parking?.Name ?? "",
                ZoneName = r.Spot?.Zone?.Name ?? "",  // ✅ FIX
                SpotCode = r.Spot?.Code ?? ""         // ✅ FIX
            }).ToList();

            return Ok(reservations);
        }

        // ✅ Cancel Reservation
        [HttpPut("cancel/{id}")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Spot!)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound("Reservation not found");

            var userId = GetCurrentUserId();

            if (User.IsInRole("Driver") && reservation.UserId != userId)
                return Forbid();

            if (reservation.Status != ReservationStatus.Created)
                return BadRequest("Only created reservations can be cancelled");

            reservation.Status = ReservationStatus.Cancelled;

            if (reservation.Spot != null)
                reservation.Spot.Status = SpotStatus.Free;

            await _context.SaveChangesAsync();

            return Ok("Reservation cancelled successfully");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst("id") ??
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new Exception("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }
    }
}