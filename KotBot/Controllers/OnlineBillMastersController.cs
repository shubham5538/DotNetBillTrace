using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KotBot.Models;
using KotBot.Services;
using Microsoft.AspNetCore.Cors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KotBot.Controllers
{
    [EnableCors("AllowAllOrigins")]
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineBillMastersController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public OnlineBillMastersController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Get all bill masters
        [HttpGet]
        public async Task<ActionResult<List<OnlineBillMaster>>> GetAllMasters() =>
            Ok(await _mongoService.GetBillMastersAsync());

        // Get a bill master by serial number
        [HttpGet("{srNumber}")]
        public async Task<ActionResult<OnlineBillMaster>> GetMasterBySerialNumber(int srNumber)
        {
            var master = await _mongoService.GetBillMasterAsync(srNumber);
            if (master == null) return NotFound();
            return Ok(master);
        }

        // Create a new bill master
        [HttpPost]
        public async Task<ActionResult> CreateBillMaster([FromBody] OnlineBillMaster master)
        {
            await _mongoService.CreateBillMasterAsync(master);
            return CreatedAtAction(nameof(GetMasterBySerialNumber), new { srNumber = master.srNumber }, master);
        }

        // Update an existing bill master
        [HttpPut("{srNumber}")]
        public async Task<ActionResult> UpdateBillMaster(int srNumber, [FromBody] OnlineBillMaster master)
        {
            var existingMaster = await _mongoService.GetBillMasterAsync(srNumber);
            if (existingMaster == null) return NotFound();
            await _mongoService.UpdateBillMasterAsync(srNumber, master);
            return NoContent();
        }

        // Delete a bill master by serial number
        [HttpDelete("{srNumber}")]
        public async Task<ActionResult> DeleteBillMaster(int srNumber)
        {
            var existingMaster = await _mongoService.GetBillMasterAsync(srNumber);
            if (existingMaster == null) return NotFound();

            await _mongoService.DeleteBillMasterAsync(srNumber);
            return NoContent();
        }

        // Get a bill master by ID
        [HttpGet("master/{id}")]
        public async Task<ActionResult<OnlineBillMaster>> GetBillMasterById(string id)
        {
            var master = await _mongoService.GetBillMasterByIdAsync(id);
            if (master == null) return NotFound();
            return Ok(master);
        }
    }
}
