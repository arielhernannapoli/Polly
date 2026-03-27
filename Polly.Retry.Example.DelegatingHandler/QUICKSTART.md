# ⚡ Quick Start - Polly.Retry.Example.DelegatingHandler

## 🚀 Empieza en 30 segundos

### 1. **Program.cs** - Sin lógica personalizada (RECOMENDADO)
```csharp
builder.Services.AddHttpClient<IBackendService, BackendService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7056");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddCustomResilienceHandler();
```

### 2. **Con lógica personalizada** (si la necesitas)
```csharp
// DelegatingHandler personalizado
public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"📤 {request.Method} {request.RequestUri}");
        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine($"📥 {response.StatusCode}");
        return response;
    }
}

// Registrar
builder.Services.AddScoped<LoggingHandler>();

builder.Services.AddHttpClient<IBackendService, BackendService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7056");
})
.AddHttpMessageHandler<LoggingHandler>()     // ← Lógica personalizada
.AddCustomResilienceHandler();               // ← Resiliencia automática
```

### 3. **Servicio**
```csharp
public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;

    public BackendService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("api/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 4. **¡Listo!** ✅

- ✅ Resiliencia automática
- ✅ Lógica personalizada (opcional)
- ✅ Flujo claro de ejecución

## 📖 Docs

- `README.md` - Instrucciones detalladas
- `MIGRATION.md` - Migración desde Polly
- `EJEMPLOS_AVANZADOS.cs` - DelegatingHandlers personalizados

## 🔄 Flujo de ejecución

```
Request
  ↓
[LoggingHandler]      (si existe)
  ↓
[ResilienceHandler]   (Retry, Circuit Breaker, Timeout)
  ↓
Backend API
  ↓
Response
```

---

**¿Preguntas?** Ver `README.md`
