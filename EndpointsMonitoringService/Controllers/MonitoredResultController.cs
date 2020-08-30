using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices.ComTypes;
using System.IO;

namespace EndpointsMonitoringService.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MonitoredResultController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<MonitoredResultController> _logger;
        private string _failureMessage;
        private readonly IOwner _owner; //user identity from request

        public MonitoredResultController(IOwner owner, DatabaseContext context, ILoggerFactory logger)
        { 
            _context = context;
            _logger = logger.CreateLogger<MonitoredResultController>();
            _owner = owner;
        }

        // GET: api/MonitoredResult
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoringResult>>> GetMonitoringResult()
        {
            var ednpointsIds = await _context.MonitoredEndpoint.Where(x => x.Owner == _owner.Data).Select(x=>x.Id).ToListAsync();
  
            dynamic result = new List<MonitoringResult>();


            foreach (var endpointId in ednpointsIds)
            {
                result.AddRange(_context.MonitoringResult.Where(x => x.MonitoredEndpoint.Id == endpointId).OrderBy(x=>x.Id).ToList().TakeLast(10));
            }
            return result;
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

        // GET: api/MonitoredResult/5
        [HttpGet("ForEndpoint/{id}")]
        public async Task<ActionResult<IEnumerable<MonitoringResult>>> GetMonitoringResultForEndopoint(int id)
        {
            var monitoringResult = await _context.MonitoredEndpoint.FindAsync(id);

            if (monitoringResult == null)
            {
                return NotFound();
            }

            return _context.MonitoringResult.Where(x => x.MonitoredEndpointForeignKey == id).OrderBy(x=>x.Id).ToList().TakeLast(10).ToList();
            //when the first .ToList() is missing before .TakeLast(10), EF is throwing exception (TakeLast() probably unsuported in EF):
            //        System.InvalidOperationException: Processing of the LINQ expression 'DbSet<MonitoringResult>
            //.Where(x => x.MonitoredEndpointForeignKey == __id_0)
            //.OrderByDescending(x => x.DateOfCheck)
            //.TakeLast(__p_1)' by 'NavigationExpandingExpressionVisitor' failed. This may indicate either a bug or a limitation in EF Core. See https://go.microsoft.com/fwlink/?linkid=2101433 for more detailed information.







        }


        private bool MonitoringResultExists(long id)
        {
            return _context.MonitoringResult.Any(e => e.Id == id);
        }
    }
}
