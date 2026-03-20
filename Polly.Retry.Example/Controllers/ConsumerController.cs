using Microsoft.AspNetCore.Mvc;
using Polly.Retry.Example.Services;

namespace Polly.Retry.Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly IBackendService _backendService;
        private readonly ILogger<ConsumerController> _logger;

        public ConsumerController(IBackendService backendService, ILogger<ConsumerController> logger)
        {
            _backendService = backendService;
            _logger = logger;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetDataFromBackend()
        {
            try
            {
                _logger.LogInformation("Consumer: Iniciando llamada a Backend con Polly retry");
                var data = await _backendService.GetDataAsync();
                return Ok(new 
                { 
                    success = true,
                    data = data,
                    message = "Datos obtenidos del backend con reintentos de Polly"
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Consumer: Error después de reintentos");
                return StatusCode(503, new 
                { 
                    success = false,
                    error = "No se pudo conectar al backend después de reintentos",
                    details = ex.Message
                });
            }
        }
    }
}
