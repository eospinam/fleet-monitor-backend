using FleetMonitor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DeviceController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var isAdmin = User.IsInRole("admin");
            var devices = await _db.Devices.ToListAsync();

            if (!isAdmin)
            {
                return Ok(devices.Select(d => new {
                    id = d.Id,
                    deviceIdentifier = d.MaskedIdentifier,
                    capacity = d.TankCapacityLiters
                }));
            }

            return Ok(devices);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
        {
            var device = new Device
            {
                Id = Guid.NewGuid(),
                DeviceIdentifier = dto.DeviceIdentifier,
                TankCapacityLiters = dto.TankCapacityLiters
            };
            _db.Devices.Add(device);
            await _db.SaveChangesAsync();
            return Ok(device);
        }
    }

    public record CreateDeviceDto(string DeviceIdentifier, double TankCapacityLiters);
}
