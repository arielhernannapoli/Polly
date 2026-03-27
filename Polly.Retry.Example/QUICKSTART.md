# ⚡ Quick Start - Polly.Retry.Example

## 🚀 Empieza en 30 segundos

### 1. **Program.cs** - Configuración
```csharp
// Registra TODOS los clientes HTTP con resiliencia automática
builder.Services.AddHttpClientsWithResilience(
    "BackendApi",
    "ExternalApi",
    "ThirdPartyApi"
);
```

### 2. **BackendService.cs** - Usa HttpClientFactory
```csharp
public class BackendService : IBackendService
{
    private readonly IHttpClientFactory _factory;

    public BackendService(IHttpClientFactory factory) => _factory = factory;

    public async Task<string> GetDataAsync()
    {
        var client = _factory.CreateClient("BackendApi");
        var response = await client.GetAsync("https://api.example.com/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 3. **¡Listo!** ✅

La resiliencia se aplica automáticamente:
- ✅ **Retry** - 3 intentos con backoff exponencial
- ✅ **Circuit Breaker** - Abierto si 50% de fallos
- ✅ **Timeout** - 10 segundos máximo

## 📖 Docs

- `README.md` - Instrucciones detalladas
- `MIGRATION.md` - Cómo se hizo la migración
- `GUIA_HANDLERS_GLOBALES.md` - 5 opciones diferentes

## 🔧 Extensiones Disponibles

```csharp
// Un cliente
builder.Services.AddHttpClient("Api")
    .AddCustomResilienceHandler();

// Múltiples clientes (RECOMENDADO)
builder.Services.AddHttpClientsWithResilience("Api1", "Api2", "Api3");
```

## ⚙️ Ver también

- `Extensions/HttpClientExtensions.cs` - Handlers personalizados
- `EJEMPLOS_AVANZADOS.cs` - Código avanzado comentado

---

**¿Preguntas?** Ver `README.md`
