using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;
using SmartParking.Models.DTOs;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ZonesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/zones
        [HttpGet]
        public async Task<ActionResult> GetZones()
        {
            var zones = await _context.Zones
                .Include(z => z.Parking)
                .Select(z => new
                {
                    z.Id,
                    z.Name,
                    z.ParkingId,
                    ParkingName = z.Parking != null ? z.Parking.Name : ""
                })
                .ToListAsync();

            return Ok(zones);
        }

        // GET: api/Zones/parking/{parkingId}
        [HttpGet("parking/{parkingId}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetZonesByParkingId(int parkingId)
        {
            var parkingExists = await _context.Parkings
                .AnyAsync(p => p.Id == parkingId);

            if (!parkingExists)
                return NotFound(new { message = "Parking not found" });

            var zones = await _context.Zones
                .Where(z => z.ParkingId == parkingId)
                .Select(z => new
                {
                    z.Id,
                    z.Name,
                    z.ParkingId
                })
                .ToListAsync();

            return Ok(zones);
        }

        // GET: api/zones/by-parking-name/{parkingName}
        [HttpGet("by-parking-name/{parkingName}")]
        public async Task<ActionResult> GetZonesByParkingName(string parkingName)
        {
            var parking = await _context.Parkings
                .FirstOrDefaultAsync(p => p.Name.ToLower() == parkingName.ToLower());

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            var zones = await _context.Zones
                .Where(z => z.ParkingId == parking.Id)
                .Select(z => new
                {
                    z.Id,
                    z.Name,
                    z.ParkingId
                })
                .ToListAsync();

            return Ok(zones);
        }

        // GET: api/zones/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> GetZoneById(int id)
        {
            var zone = await _context.Zones
                .Include(z => z.Parking)
                .Where(z => z.Id == id)
                .Select(z => new
                {
                    z.Id,
                    z.Name,
                    z.ParkingId,
                    ParkingName = z.Parking != null ? z.Parking.Name : ""
                })
                .FirstOrDefaultAsync();

            if (zone == null)
                return NotFound(new { message = "Zone not found" });

            return Ok(zone);
        }

        // POST: api/zones
        [HttpPost]
        public async Task<ActionResult> CreateZone([FromBody] ZoneDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parkingExists = await _context.Parkings
                .AnyAsync(p => p.Id == dto.ParkingId);

            if (!parkingExists)
                return BadRequest(new { message = "Parking does not exist" });

            var zone = new Zone
            {
                Name = dto.Name,
                ParkingId = dto.ParkingId
            };

            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                zone.Id,
                zone.Name,
                zone.ParkingId
            });
        }

        // PUT: api/zones/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, [FromBody] ZoneDto dto)
        {
            var zone = await _context.Zones.FindAsync(id);

            if (zone == null)
                return NotFound(new { message = "Zone not found" });

            var parkingExists = await _context.Parkings
                .AnyAsync(p => p.Id == dto.ParkingId);

            if (!parkingExists)
                return BadRequest(new { message = "Parking does not exist" });

            zone.Name = dto.Name;
            zone.ParkingId = dto.ParkingId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Zone updated successfully",
                zone.Id,
                zone.Name,
                zone.ParkingId
            });
        }

        // DELETE: api/zones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);

            if (zone == null)
                return NotFound(new { message = "Zone not found" });

            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Zone deleted successfully" });
        }
    }
}