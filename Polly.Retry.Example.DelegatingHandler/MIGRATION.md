# Guía de Migración - Polly.Retry.Example.DelegatingHandler

## 📋 Resumen

Este proyecto ha sido migrado a **Microsoft.Extensions.Http.Resilience** manteniendo la capacidad de usar **DelegatingHandlers personalizados**.

## 🎯 Patrón: DelegatingHandler + Resiliencia

Combina:
- ✅ Interceptación de requests/responses
- ✅ Lógica personalizada (headers, logging, auth)
- ✅ Resiliencia automática de Microsoft

## 💻 Estructura Básica (Sin DelegatingHandler Personalizado)

```csharp
// Program.cs
builder.Services.AddHttpClient<IBackendService, BackendService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7056");
})
.AddCustomResilienceHandler();

// Services/BackendService.cs
public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;

    public BackendService(HttpClient httpClient)
    {
        _httpClient = httpClient; // Ya tiene resiliencia aplicada
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("api/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

## 🔧 Estructura Avanzada (Con DelegatingHandler Personalizado)

Si necesitas lógica personalizada:

```csharp
// CustomDelegatingHandler.cs
public class CustomDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<CustomDelegatingHandler> _logger;

    public CustomDelegatingHandler(ILogger<CustomDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Pre-procesamiento
        _logger.LogInformation("📤 {Method} {Uri}", request.Method, request.RequestUri);

        // Ejecutar request (con resiliencia aplicada abajo en la cadena)
        var response = await base.SendAsync(request, cancellationToken);

        // Post-procesamiento
        _logger.LogInformation("📥 {StatusCode}", response.StatusCode);

        return response;
    }
}

// Program.cs
builder.Services.AddScoped<CustomDelegatingHandler>();

builder.Services.AddHttpClient<IBackendService, BackendService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7056");
})
.AddHttpMessageHandler<CustomDelegatingHandler>()  // ← Handler personalizado
.AddCustomResilienceHandler();                      // ← Resiliencia automática
```

## 🔄 Flujo de Ejecución

```
Request
   ↓
[CustomDelegatingHandler]  (si existe)
   ↓
[ResilienceHandler]        (Retry, Circuit Breaker, Timeout)
   ↓
Backend API
   ↓
[ResilienceHandler]        (Manejo de errores)
   ↓
[CustomDelegatingHandler]  (Post-procesamiento)
   ↓
Response
```

## 📋 Casos de Uso para DelegatingHandler

| Caso | Solución |
|------|----------|
| Solo resiliencia | `AddCustomResilienceHandler()` |
| + Logging | DelegatingHandler para logging |
| + Headers personalizados | DelegatingHandler para headers |
| + Autenticación | DelegatingHandler para auth |
| + Métricas | DelegatingHandler para métricas |

Ver `EJEMPLOS_AVANZADOS.cs` para ejemplos.

## ✅ Checklist de Migración

- [x] Actualizar .csproj
- [x] Crear HttpClientExtensions.cs
- [x] Actualizar Program.cs
- [x] Eliminar PollyDelegatingHandler.cs (ya no necesario)
- [x] Compilación exitosa ✅

## 🔗 Referencia

- [DelegatingHandler Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler)
- [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)

## 📚 Documentación

- 📖 README.md
- 📖 GUIA_HANDLERS_GLOBALES.md
- 📖 EJEMPLOS_AVANZADOS.cs

---

**Estado:** ✅ COMPLETADO  
**Versión .NET:** 8.0
