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
        private string _UNAUTHORIZED_MSG = "RECORD WITH THIS ID HAS DIFFERENT OWNER";

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
            var ednpointsIds = await _context.MonitoredEndpoint.Where(x => x.Owner == _owner.Data).Select(x => x.Id).ToListAsync();

            var result = new List<MonitoringResult>();

            foreach (var endpointId in ednpointsIds)
            {
                var last10 = await _context.MonitoringResult.Where(x => x.MonitoredEndpoint.Id == endpointId).OrderByDescending(x => x.DateOfCheck).Take(10).ToListAsync();
                result.AddRange(last10);
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

            var ownerTestResult = await OwnerTestAsync(monitoringResult);

            if (!ownerTestResult)
            {
                return Unauthorized(_UNAUTHORIZED_MSG);
            }

            return monitoringResult;
        }

        // GET: api/MonitoredResult/ForEndpoind/5
        [HttpGet("ForEndpoint/{id}")]
        public async Task<ActionResult<IEnumerable<MonitoringResult>>> GetMonitoringResultForEndopoint(int id)
        {
            var monitoredEndpoint = await _context.MonitoredEndpoint.FindAsync(id);

            if (monitoredEndpoint == null)
            {
                return NotFound();
            }

            if (monitoredEndpoint.Owner.Id != _owner.Data.Id)
            {
                return Unauthorized(_UNAUTHORIZED_MSG);
            }

            var results = await GetTenLastResultsForEndpointAsync(monitoredEndpoint);
            return results;

        }

        private async Task<bool> OwnerTestAsync(MonitoringResult monitoringResult)
        {
            try
            {
                var resultOwner = await _context.MonitoredEndpoint.FirstOrDefaultAsync(x => x.Id == monitoringResult.MonitoredEndpointForeignKey);
                if (resultOwner.Id != _owner.Data.Id)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR RESOLVING OWNER", ex);
                throw ex;
            }
        }

        private async Task<List<MonitoringResult>> GetTenLastResultsForEndpointAsync(MonitoredEndpoint endpoint)
        {
            var result = await _context.MonitoringResult.Where(x => x.MonitoredEndpointForeignKey == endpoint.Id).OrderByDescending(x => x.DateOfCheck).Take(10).ToListAsync();
            return result;
        }

    }
}
