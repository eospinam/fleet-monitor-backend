using FleetMonitor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FleetMonitor.Api.Hubs;

namespace FleetMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // requiere token
    public class TelemetryController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<TelemetryHub> _hub;

        public TelemetryController(AppDbContext db, IHubContext<TelemetryHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] TelemetryDto dto)
        {
            var device = await _db.Devices.FindAsync(dto.DeviceId);
            if (device == null) return NotFound("Device not found");

            var telemetry = new Telemetry
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                Timestamp = dto.Timestamp,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SpeedKph = dto.SpeedKph,
                FuelLiters = dto.FuelLiters,
                TemperatureC = dto.TemperatureC
            };

            _db.Telemetries.Add(telemetry);
            await _db.SaveChangesAsync();

            // calcular autonomía
            var autonomy = await EstimateAutonomyHoursAsync(device.Id, telemetry);

            if (autonomy.HasValue && autonomy.Value < 1)
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    DeviceId = device.Id,
                    Message = "Autonomía de combustible menor a 1 hora",
                    Timestamp = DateTime.UtcNow,
                    AutonomyHours = autonomy.Value
                };
                _db.Alerts.Add(alert);
                await _db.SaveChangesAsync();

                await _hub.Clients.Group("admins").SendAsync("alert", new
                {
                    device = device.DeviceIdentifier,
                    autonomy = autonomy.Value,
                    alert.Message,
                    alert.Timestamp
                });
            }

           
            var payload = new
            {
                device = User.IsInRole("admin") ? device.DeviceIdentifier : device.MaskedIdentifier,
                dto.Timestamp,
                dto.Latitude,
                dto.Longitude,
                dto.SpeedKph,
                dto.FuelLiters,
                dto.TemperatureC
            };

            await _hub.Clients.All.SendAsync("telemetry", payload);

            return Ok(new { telemetry.Id, autonomy });
        }

        [HttpGet]
        public async Task<IActionResult> GetByDevice(Guid deviceId)
        {
            var telemetries = await _db.Telemetries
                //.Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp)
                .Take(50) 
                .ToListAsync();

            return Ok(telemetries);
        }
        private async Task<double?> EstimateAutonomyHoursAsync(Guid deviceId, Telemetry current)
        {
            var last = await _db.Telemetries
                        .Where(t => t.DeviceId == deviceId && t.Timestamp < current.Timestamp)
                        .OrderByDescending(t => t.Timestamp)
                        .Take(5)
                        .ToListAsync();

            if (last.Count < 1) return null;

            double totalConsumption = 0;
            double totalHours = 0;

            foreach (var prev in last)
            {
                var deltaFuel = prev.FuelLiters - current.FuelLiters;
                var hours = (current.Timestamp - prev.Timestamp).TotalHours;
                if (hours <= 0) continue;
                if (deltaFuel <= 0) continue; 
                totalConsumption += deltaFuel;
                totalHours += hours;
            }

            if (totalHours <= 0) return null;
            var rate = totalConsumption / totalHours;
            if (rate <= 0) return null;

            return current.FuelLiters / rate;
        }
    }

    public record TelemetryDto(
        Guid DeviceId,
        DateTime Timestamp,
        double Latitude,
        double Longitude,
        double SpeedKph,
        double FuelLiters,
        double TemperatureC
    );
}
