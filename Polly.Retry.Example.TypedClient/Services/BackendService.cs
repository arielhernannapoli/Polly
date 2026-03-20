namespace Polly.Retry.Example.TypedClient.Services
{
    public class BackendService : IBackendService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BackendService> _logger;

        // El HttpClient se inyecta directamente (Typed HttpClient)
        public BackendService(HttpClient httpClient, ILogger<BackendService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetDataAsync()
        {
            try
            {
                _logger.LogInformation("[TypedClient] Llamando a Backend API...");
                
                // El cliente ya tiene aplicadas las políticas de Polly
                var response = await _httpClient.GetAsync("https://localhost:7056/api/backend/data");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[TypedClient] Backend API respondió correctamente");
                return content;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TypedClient] Error al llamar a Backend API");
                throw;
            }
        }
    }
}
