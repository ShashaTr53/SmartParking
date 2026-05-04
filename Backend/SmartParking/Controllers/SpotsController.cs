using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;

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

        // GET: api/spots
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetSpots()
        {
            var spots = await _context.Spots
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.ZoneId,
                    ZoneName = s.Zone != null ? s.Zone.Name : null,
                    ParkingId = s.Zone != null ? s.Zone.ParkingId : (int?)null,
                    ParkingName = s.Zone != null && s.Zone.Parking != null ? s.Zone.Parking.Name : null,
                    s.Status
                })
                .ToListAsync();

            return Ok(spots);
        }

        // GET: api/spots/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetSpotById(int id)
        {
            var spot = await _context.Spots
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.ZoneId,
                    ZoneName = s.Zone != null ? s.Zone.Name : null,
                    ParkingId = s.Zone != null ? s.Zone.ParkingId : (int?)null,
                    ParkingName = s.Zone != null && s.Zone.Parking != null ? s.Zone.Parking.Name : null,
                    s.Status
                })
                .FirstOrDefaultAsync();

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            return Ok(spot);
        }

        // GET: api/spots/by-zone/{zoneId}
        [HttpGet("by-zone/{zoneId}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetSpotsByZone(int zoneId)
        {
            var zoneExists = await _context.Zones.AnyAsync(z => z.Id == zoneId);

            if (!zoneExists)
                return NotFound(new { message = "Zone not found" });

            var spots = await _context.Spots
                .Where(s => s.ZoneId == zoneId)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.ZoneId,
                    ZoneName = s.Zone != null ? s.Zone.Name : null,
                    ParkingId = s.Zone != null ? s.Zone.ParkingId : (int?)null,
                    ParkingName = s.Zone != null && s.Zone.Parking != null ? s.Zone.Parking.Name : null,
                    s.Status
                })
                .ToListAsync();

            return Ok(spots);
        }

        // GET: api/spots/available/by-zone/{zoneId}
        [HttpGet("available/by-zone/{zoneId}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetAvailableSpotsByZone(int zoneId)
        {
            var zoneExists = await _context.Zones.AnyAsync(z => z.Id == zoneId);

            if (!zoneExists)
                return NotFound(new { message = "Zone not found" });

            var spots = await _context.Spots
                .Where(s => s.ZoneId == zoneId && s.Status == SpotStatus.Free)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.ZoneId,
                    ZoneName = s.Zone != null ? s.Zone.Name : null,
                    ParkingId = s.Zone != null ? s.Zone.ParkingId : (int?)null,
                    ParkingName = s.Zone != null && s.Zone.Parking != null ? s.Zone.Parking.Name : null,
                    s.Status
                })
                .ToListAsync();

            return Ok(spots);
        }

        // POST: api/spots
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateSpot(Spot spot)
        {
            var zone = await _context.Zones
                .Include(z => z.Parking)
                .FirstOrDefaultAsync(z => z.Id == spot.ZoneId);

            if (zone == null)
                return BadRequest(new { message = "Zone does not exist" });

            var codeExistsInZone = await _context.Spots
                .AnyAsync(s => s.ZoneId == spot.ZoneId && s.Code == spot.Code);

            if (codeExistsInZone)
                return BadRequest(new { message = "A spot with the same code already exists in this zone" });

            _context.Spots.Add(spot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSpotById), new { id = spot.Id }, new
            {
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = zone.Name,
                ParkingId = zone.ParkingId,
                ParkingName = zone.Parking != null ? zone.Parking.Name : null,
                spot.Status
            });
        }

        // PUT: api/spots/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpot(int id, Spot updatedSpot)
        {
            if (id != updatedSpot.Id)
                return BadRequest(new { message = "Spot ID mismatch" });

            var spot = await _context.Spots.FindAsync(id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            var zone = await _context.Zones
                .Include(z => z.Parking)
                .FirstOrDefaultAsync(z => z.Id == updatedSpot.ZoneId);

            if (zone == null)
                return BadRequest(new { message = "Zone does not exist" });

            var duplicateCode = await _context.Spots.AnyAsync(s =>
                s.Id != id &&
                s.ZoneId == updatedSpot.ZoneId &&
                s.Code == updatedSpot.Code);

            if (duplicateCode)
                return BadRequest(new { message = "A spot with the same code already exists in this zone" });

            spot.Code = updatedSpot.Code;
            spot.ZoneId = updatedSpot.ZoneId;
            spot.Status = updatedSpot.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Spot updated successfully",
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = zone.Name,
                ParkingId = zone.ParkingId,
                ParkingName = zone.Parking != null ? zone.Parking.Name : null,
                spot.Status
            });
        }

        // PATCH: api/spots/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateSpotStatus(int id, [FromBody] SpotStatus status)
        {
            var spot = await _context.Spots
                .Include(s => s.Zone)
                .ThenInclude(z => z.Parking)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            spot.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Spot status updated successfully",
                spot.Id,
                spot.Code,
                spot.ZoneId,
                ZoneName = spot.Zone != null ? spot.Zone.Name : null,
                ParkingId = spot.Zone != null ? spot.Zone.ParkingId : (int?)null,
                ParkingName = spot.Zone != null && spot.Zone.Parking != null ? spot.Zone.Parking.Name : null,
                spot.Status
            });
        }

        // DELETE: api/spots/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSpot(int id)
        {
            var spot = await _context.Spots.FindAsync(id);

            if (spot == null)
                return NotFound(new { message = "Spot not found" });

            _context.Spots.Remove(spot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Spot deleted successfully" });
        }
    }
}