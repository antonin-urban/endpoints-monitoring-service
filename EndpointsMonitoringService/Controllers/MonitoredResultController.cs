using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndpointsMonitoringService.Model;

namespace EndpointsMonitoringService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonitoredResultController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public MonitoredResultController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/MonitoredResult
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoringResult>>> GetMonitoringResult()
        {
            return await _context.MonitoringResult.ToListAsync();
        }

        // GET: api/MonitoredResult/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoringResult>> GetMonitoringResult(long id)
        {
            var monitoringResult = await _context.MonitoringResult.FindAsync(id);

            if (monitoringResult == null)
            {
                return NotFound();
            }

            return monitoringResult;
        }

        // PUT: api/MonitoredResult/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonitoringResult(long id, MonitoringResult monitoringResult)
        {
            if (id != monitoringResult.Id)
            {
                return BadRequest();
            }

            _context.Entry(monitoringResult).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MonitoringResultExists(id))
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

        // POST: api/MonitoredResult
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<MonitoringResult>> PostMonitoringResult(MonitoringResult monitoringResult)
        {
            _context.MonitoringResult.Add(monitoringResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonitoringResult", new { id = monitoringResult.Id }, monitoringResult);
        }

        // DELETE: api/MonitoredResult/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<MonitoringResult>> DeleteMonitoringResult(long id)
        {
            var monitoringResult = await _context.MonitoringResult.FindAsync(id);
            if (monitoringResult == null)
            {
                return NotFound();
            }

            _context.MonitoringResult.Remove(monitoringResult);
            await _context.SaveChangesAsync();

            return monitoringResult;
        }

        private bool MonitoringResultExists(long id)
        {
            return _context.MonitoringResult.Any(e => e.Id == id);
        }
    }
}
