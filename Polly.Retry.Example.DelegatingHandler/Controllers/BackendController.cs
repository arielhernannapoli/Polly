using Microsoft.AspNetCore.Mvc;

namespace Polly.Retry.Example.DelegatingHandler.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackendController : ControllerBase
    {
        private readonly ILogger<BackendController> _logger;
        private static int _callCount = 0;

        public BackendController(ILogger<BackendController> logger)
        {
            _logger = logger;
        }

        [HttpGet("data")]
        public IActionResult GetData()
        {
            _callCount++;
            
            // Simular fallos en los primeros intentos
            if (_callCount % 3 != 0)
            {
                _logger.LogWarning($"Backend: Simulando fallo en llamada {_callCount}");
                return StatusCode(500, "Error temporal en el servidor");
            }

            _logger.LogInformation($"Backend: Respondiendo correctamente en llamada {_callCount}");
            return Ok(new 
            { 
                message = "Datos del backend",
                timestamp = DateTime.UtcNow,
                callCount = _callCount
            });
        }

        [HttpPost("reset")]
        public IActionResult ResetCallCount()
        {
            _callCount = 0;
            return Ok("Call count resetado");
        }
    }
}
