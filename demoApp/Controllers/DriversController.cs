using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using demoApp.Models;
using demoApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace demoApp.Controllers
{
    [ApiController]
    [Route("")]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService cacheService;

        protected ApiDbContext _dbContext;

        public DriversController(ILogger<DriversController> logger, ApiDbContext dbContext, ICacheService cacheService)
        {
            this._logger = logger;
            this._dbContext = dbContext;
            this.cacheService = cacheService;
        }

        [HttpGet("GetAllDrivers")]
        [Benchmark]
        public async Task<IActionResult> Get()
        {
            var cacheData = cacheService.GetData<IEnumerable<Driver>>("drivers");

            if (cacheData != null && cacheData.Any())
            {
                return Ok(cacheData.Take(10));
            }
            else
            {
                var AllDrivers = await _dbContext.Drivers.ToListAsync();

                if (AllDrivers.Any())
                {
                    // Set expiry time
                    var expiryTime = DateTimeOffset.Now.AddSeconds(360);

                    // Set data in cache
                    cacheService.SetData<IEnumerable<Driver>>("drivers", AllDrivers, expiryTime);

                    AllDrivers = AllDrivers.Take(10).ToList();
                    return Ok(AllDrivers);
                }
                else
                {
                    return NotFound("No drivers found in the database.");
                }
            }
        }
    }
}