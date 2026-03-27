# ⚡ Quick Start - Polly.Retry.Example.TypedClient

## 🚀 Empieza en 30 segundos

### 1. **Definir el servicio**
```csharp
public interface IBackendService
{
    Task<string> GetDataAsync();
}

public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;

    public BackendService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("https://api.example.com/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 2. **Program.cs** - Registrar
```csharp
// Typed HttpClient con resiliencia automática
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddCustomResilienceHandler();
```

### 3. **Controlador** - Usar
```csharp
public class ConsumerController : ControllerBase
{
    public ConsumerController(IBackendService service) => _service = service;

    [HttpGet("data")]
    public async Task<IActionResult> GetData()
    {
        return Ok(await _service.GetDataAsync());
    }
}
```

### 4. **¡Listo!** ✅

- ✅ HttpClient inyectado automáticamente
- ✅ Type-safe
- ✅ Resiliencia automática

## 📖 Docs

- `README.md` - Instrucciones detalladas
- `MIGRATION.md` - Migración desde Polly
- `GUIA_HANDLERS_GLOBALES.md` - Opciones avanzadas

## 🎯 Ventajas de Typed HttpClient

vs HttpClientFactory:
- ✅ Type-safe
- ✅ IntelliSense
- ✅ Refactoring seguro
- ✅ Testing fácil (mock)

---

**¿Preguntas?** Ver `README.md`
