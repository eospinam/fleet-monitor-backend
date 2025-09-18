using FleetMonitor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AlertsController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var isAdmin = User.IsInRole("admin");
            if (isAdmin)
            {
                return Ok(await _db.Alerts.Include(a => a.Device).ToListAsync());
            }
            // si no es admin -> no mostramos deviceIdentifier completo
            var alerts = await _db.Alerts.Include(a => a.Device).ToListAsync();
            return Ok(alerts.Select(a => new {
                id = a.Id,
                device = a.Device.MaskedIdentifier,
                a.Message,
                a.Timestamp,
                a.AutonomyHours
            }));
        }
    }
}
