using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using KotBot.Services;

namespace KotBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThermalPrinterController : ControllerBase
    {
        private readonly string _printerIp;
        private readonly ThermalPrinter _thermalPrinter;

        public ThermalPrinterController(IConfiguration configuration)
        {
            _printerIp = configuration["PrinterIp"] ?? throw new ArgumentNullException("Printer IP cannot be null.");
            _thermalPrinter = new ThermalPrinter(_printerIp);
        }

        [HttpPost("print-receipt")]
        public IActionResult PrintReceipt([FromBody] Receipt receipt)
        {
            try
            {
                // Call method to print
                _thermalPrinter.PrintReceipt(receipt);
                return Ok("Receipt printed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error printing receipt: {ex.Message}");
            }
        }
    }
}
