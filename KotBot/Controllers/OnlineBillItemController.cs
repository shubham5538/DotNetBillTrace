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
    public class OnlineBillItemsController : ControllerBase
    {
        private readonly MongoService _mongoService;

        public OnlineBillItemsController(MongoService mongoService)
        {
            _mongoService = mongoService;
        }

        // Get all bill items
        [HttpGet]
        public async Task<ActionResult<List<OnlineBillItem>>> GetAllItems() =>
            Ok(await _mongoService.GetBillItemsAsync());

        // Get items by serial number
        [HttpGet("{srNumber}")]
        public async Task<ActionResult<List<OnlineBillItem>>> GetItemsBySerialNumber(int srNumber)
        {
            var items = await _mongoService.GetBillItemsBySrNumberAsync(srNumber);
            if (items == null || items.Count == 0) return NotFound();
            return Ok(items);
        }

        // Alternative method to get items by serial number
        [HttpGet("items/by-sr-number/{srNumber}")]
        public async Task<ActionResult<List<OnlineBillItem>>> GetItemsBySrNumberAlternate(int srNumber)
        {
            var items = await _mongoService.GetBillItemsBySrNumberAsync(srNumber);
            if (items == null || items.Count == 0) return NotFound();
            return Ok(items);
        }

        // Create a new bill item
        [HttpPost]
        public async Task<ActionResult> CreateBillItem([FromBody] OnlineBillItem item)
        {
            await _mongoService.CreateBillItemAsync(item);
            return CreatedAtAction(nameof(GetItemsBySerialNumber), new { srNumber = item.srNumber }, item);
        }

        // Update an existing bill item
        [HttpPut("{srNumber}")]
        public async Task<ActionResult> UpdateBillItem(int srNumber, [FromBody] OnlineBillItem item)
        {
            var existingItems = await _mongoService.GetBillItemsBySrNumberAsync(srNumber);
            if (existingItems == null || existingItems.Count == 0) return NotFound();
            await _mongoService.UpdateBillItemAsync(srNumber, item);
            return NoContent();
        }

        // Delete a bill item by serial number
        [HttpDelete("{srNumber}")]
        public async Task<ActionResult> DeleteBillItem(int srNumber)
        {
            var existingItems = await _mongoService.GetBillItemsBySrNumberAsync(srNumber);
            if (existingItems == null || existingItems.Count == 0) return NotFound();

            foreach (var existingItem in existingItems)
            {
                await _mongoService.DeleteBillItemAsync(existingItem.srNumber);
            }
            return NoContent();
        }

        // Get a bill item by its ID
        [HttpGet("item/{id}")]
        public async Task<ActionResult<OnlineBillItem>> GetBillItemById(string id)
        {
            var item = await _mongoService.GetBillItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }
}
