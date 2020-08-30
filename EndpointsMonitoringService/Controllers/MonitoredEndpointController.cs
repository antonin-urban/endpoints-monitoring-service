using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EndpointsMonitoringService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MonitoredEndpointController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<MonitoredEndpointController> _logger;
        private string _failureMessage;
        private readonly IOwner _owner; //user identity from request

        public MonitoredEndpointController(IOwner owner, DatabaseContext context, ILoggerFactory logger)
        {
            _context = context;
            _logger = logger.CreateLogger<MonitoredEndpointController>();
            _owner = owner;
        }

        // GET: api/MonitoredEndpoint
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoredEndpoint>>> GetMonitoredEndpoint()
        {
            return await _context.MonitoredEndpoint.ToListAsync();
        }

        // GET: api/MonitoredEndpoint/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoredEndpoint>> GetMonitoredEndpoint(int id)
        {
            var monitoredEndpoint = await _context.MonitoredEndpoint.FindAsync(id);

            if (monitoredEndpoint == null)
            {
                return NotFound();
            }

            return monitoredEndpoint;
        }

        // PUT: api/MonitoredEndpoint/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonitoredEndpoint(int id, MonitoredEndpoint monitoredEndpoint)
        {
            if (id != monitoredEndpoint.Id)
            {
                return BadRequest();
            }

            _context.Entry(monitoredEndpoint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MonitoredEndpointExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MonitoredEndpoint
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MonitoredEndpoint>> PostMonitoredEndpoint(MonitoredEndpoint monitoredEndpoint)
        {
            monitoredEndpoint.Owner = _owner.Data;

            _context.MonitoredEndpoint.Add(monitoredEndpoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonitoredEndpoint", new { id = monitoredEndpoint.Id }, monitoredEndpoint);
        }

        // DELETE: api/MonitoredEndpoint/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MonitoredEndpoint>> DeleteMonitoredEndpoint(int id)
        {
            var monitoredEndpoint = await _context.MonitoredEndpoint.FindAsync(id);
            if (monitoredEndpoint == null)
            {
                return NotFound();
            }

            _context.MonitoredEndpoint.Remove(monitoredEndpoint);
            await _context.SaveChangesAsync();

            return monitoredEndpoint;
        }

        private bool MonitoredEndpointExists(int id)
        {
            return _context.MonitoredEndpoint.Any(e => e.Id == id);
        }
    }
}
