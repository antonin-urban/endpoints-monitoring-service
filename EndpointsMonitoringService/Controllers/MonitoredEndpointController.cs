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
            _logger.LogInformation("ROUTE: GET api/MonitoredEndpoint");
            return await _context.MonitoredEndpoint.Where(x=>x.Owner == _owner.Data).ToListAsync();
        }

        // GET: api/MonitoredEndpoint/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoredEndpoint>> GetMonitoredEndpoint(int id)
        {
            _logger.LogInformation("ROUTE: GET api/MonitoredEndpoint/id");
            var monitoredEndpoint = await _context.MonitoredEndpoint.FindAsync(id);

            if (monitoredEndpoint == null)
            {
                return NotFound();
            }

            return monitoredEndpoint;
        }

        // PUT: api/MonitoredEndpoint/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonitoredEndpoint(int id, [FromBody] MonitoredEndpoint monitoredEndpoint)
        {
            _logger.LogInformation("ROUTE: PUT api/MonitoredEndpoint/id");
            monitoredEndpoint = CleanDeserializedMonitoredEndpoint(monitoredEndpoint);

            var endpointToUpdate = await _context.MonitoredEndpoint.FindAsync(id);

            if(endpointToUpdate == null)
            {
                return NotFound("ENTITY NOT FOUND IN DB, USE PUT ONLY FOR UPDATES WITH EXISTING ID");
            }

            endpointToUpdate.Name = monitoredEndpoint.Name;
            endpointToUpdate.Url = monitoredEndpoint.Url;

            _context.Entry(endpointToUpdate).State = EntityState.Modified;

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
        [HttpPost]
        public async Task<ActionResult<MonitoredEndpoint>> PostMonitoredEndpoint([FromBody]MonitoredEndpoint monitoredEndpoint)
        {
            _logger.LogInformation("ROUTE: POST api/MonitoredEndpoint");
            monitoredEndpoint = CleanDeserializedMonitoredEndpoint(monitoredEndpoint);

            monitoredEndpoint.Owner = _owner.Data;

            _context.MonitoredEndpoint.Add(monitoredEndpoint);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonitoredEndpoint", new { id = monitoredEndpoint.Id }, monitoredEndpoint);
        }

        // DELETE: api/MonitoredEndpoint/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MonitoredEndpoint>> DeleteMonitoredEndpoint(int id)
        {
            _logger.LogInformation("ROUTE: DELETE api/MonitoredEndpoint/id");
            var monitoredEndpoint = await _context.MonitoredEndpoint.FindAsync(id);
            if (monitoredEndpoint == null)
            {
                return NotFound();
            }

            if(monitoredEndpoint.Owner != _owner.Data)
            {
                return Unauthorized("RECORD WITH THIS ID HAS DIFFERENT OWNER");
            }

            _context.MonitoredEndpoint.Remove(monitoredEndpoint);
            await _context.SaveChangesAsync();

            return monitoredEndpoint;
        }

        private bool MonitoredEndpointExists(int id)
        {
            return _context.MonitoredEndpoint.Any(e => e.Id == id);
        }



        private MonitoredEndpoint CleanDeserializedMonitoredEndpoint(MonitoredEndpoint deserializedMonitoredEndpoint)
        {
            var cleanMonitoredEndpoint = new MonitoredEndpoint();

            cleanMonitoredEndpoint.Name = deserializedMonitoredEndpoint.Name;
            cleanMonitoredEndpoint.Url = deserializedMonitoredEndpoint.Url;
            cleanMonitoredEndpoint.MonitoredInterval = deserializedMonitoredEndpoint.MonitoredInterval;
            return cleanMonitoredEndpoint;
        }


    }
}
