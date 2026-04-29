using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;
using SmartParking.Models.DTOs;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpotsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetSpots()
        {
            var spots = await _context.Spots
                .Include(s => s.Zone!)
                    .ThenInclude(z => z.Parking!)
                .ToListAsync();

            var result = spots.Select(s => new
            {
                s.Id,
                s.Code,
                s.ZoneId,
                ZoneName = s.Zone?.Name ?? "",
                ParkingId = s.Zone?.ParkingId,
                ParkingName = s.Zone?.Parking?.Name ?? "",
                s.Status
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetSpotById(int id)
        {
            var s = await _context.Spots
                .Include(x => x.Zone!)
                    .ThenInclude(z => z.Parking!)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null)
                return NotFound(new { message = "Spot not found" });

            return Ok(new
            {
                s.Id,
                s.Code,
                s.ZoneId,
                ZoneName = s.Zone?.Name ?? "",
                ParkingId = s.Zone?.ParkingId,
                ParkingName = s.Zone?.Parking?.Name ?? "",
                s.Status
            });
        }

        [HttpGet("by-zone/{zoneId}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetSpotsByZone(int zoneId)
        {
            var zoneExists = await _context.Zones.AnyAsync(z => z.Id == zoneId);

            if (!zoneExists)
                return NotFound(new { message = "Zone not found" });

            var spots = await _context.Spots
                .Where(s => s.ZoneId == zoneId)
                .Include(s => s.Zone!)
                    .ThenInclude(z => z.Parking!)
                .ToListAsync();

            var result = spots.Select(s => new
            {
                s.Id,
                s.Code,
                s.ZoneId,
                ZoneName = s.Zone?.Name ?? "",
                ParkingId = s.Zone?.ParkingId,
                ParkingName = s.Zone?.Parking?.Name ?? "",
                s.Status
            });

            return Ok(result);
        }

        [HttpGet("available/by-zone/{zoneId}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetAvailableSpotsByZone(int zoneId)
        {
            var zoneExists = await _context.Zones.AnyAsync(z => z.Id == zoneId);

            if (!zoneExists)
                return NotFound(new { message = "Zone not found" });

            var spots = await _context.Spots
                .Where(s => s.ZoneId == zoneId && s.Status == SpotStatus.Free)
                .Include(s => s.Zone!)
                    .ThenInclude(z => z.Parking!)
                .ToListAsync();

            var result = spots.Select(s => new
            {
                s.Id,
                s.Code,
                s.ZoneId,
                ZoneName = s.Zone?.Name ?? "",
                ParkingId = s.Zone?.ParkingId,
                ParkingName = s.Zone?.Parking?.Name ?? "",
                s.Status
            });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateSpot([FromBody] SpotDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var zone = await _context.Zones
                .Include(z => z.Parking!)
                .FirstOrDefaultAsync(z => z.Id == dto.ZoneId);

            if (zone == null)
                return BadRequest(new { message = "Zone does not exist" });

            var codeExistsInZone = await _context.Spots
                .AnyAsync(s => s.ZoneId == dto.ZoneId && s.Code == dto.Code);

            if (codeExistsInZone)
                return BadRequest(new { message = "Duplicate code in zone" });

            var spot = new Spot
            {
                Code = dto.Code,
                ZoneId = dto.ZoneId,
                Status = dto.Status
            };

            _context.Spots.Add(spot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotById), new { id = spot.Id }, new
            {
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = zone.Name,
                ParkingId = zone.ParkingId,
                ParkingName = zone.Parking?.Name ?? "",
                spot.Status
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpot(int id, [FromBody] SpotDto dto)
        {
            var spot = await _context.Spots.FindAsync(id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            var zone = await _context.Zones
                .Include(z => z.Parking!)
                .FirstOrDefaultAsync(z => z.Id == dto.ZoneId);

            if (zone == null)
                return BadRequest(new { message = "Zone does not exist" });

            var duplicateCode = await _context.Spots.AnyAsync(s =>
                s.Id != id &&
                s.ZoneId == dto.ZoneId &&
                s.Code == dto.Code);

            if (duplicateCode)
                return BadRequest(new { message = "Duplicate code in zone" });

            spot.Code = dto.Code;
            spot.ZoneId = dto.ZoneId;
            spot.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Spot updated",
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = zone.Name,
                ParkingId = zone.ParkingId,
                ParkingName = zone.Parking?.Name ?? "",
                spot.Status
            });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateSpotStatus(int id, [FromBody] SpotStatus status)
        {
            var spot = await _context.Spots
                .Include(s => s.Zone!)
                    .ThenInclude(z => z.Parking!)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            spot.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Status updated",
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = spot.Zone?.Name ?? "",
                ParkingId = spot.Zone?.ParkingId,
                ParkingName = spot.Zone?.Parking?.Name ?? "",
                spot.Status
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpot(int id)
        {
            var spot = await _context.Spots.FindAsync(id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            _context.Spots.Remove(spot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Spot deleted" });
        }
    }
}