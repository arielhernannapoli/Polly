# Polly.Retry.Example.DelegatingHandler

## 📚 Descripción General

Este proyecto es una implementación de **resilencia en comunicaciones HTTP** usando la librería **Polly**, pero con un enfoque diferente al proyecto original: en lugar de configurar las políticas de Polly directamente en el **Inyector de Dependencias (DI)**, utilizamos un **`DelegatingHandler`** personalizado que intercept​a y aplica las políticas de manera explícita.

### ¿Por qué dos enfoques diferentes?

| Aspecto | DI (HttpClientFactory) | DelegatingHandler |
|--------|------------------------|-------------------|
| **Configuración** | `AddPolicyHandler()` en `services` | `AddHttpMessageHandler<T>()` con handler personalizado |
| **Aplicación** | Políticas configuradas de forma implícita | Handler personalizado intercepta solicitudes |
| **Control** | Menor - políticas predefinidas | Mayor - lógica personalizada en el handler |
| **Flexibilidad** | Política por cliente HTTP nombrado | Handler reutilizable y más flexible |
| **Testing** | Más simple con IHttpClientFactory | Más fácil de mockear el handler |
| **Mantenibilidad** | Configuración centralizada | Código explícito y autodocumentado |

## 🏗️ Estructura del Proyecto

```
Polly.Retry.Example.DelegatingHandler/
│
├── 📁 Handlers/
│   └── PollyDelegatingHandler.cs       # Handler personalizado que aplica políticas
│
├── 📁 Services/
│   ├── IBackendService.cs              # Interfaz del servicio de backend
│   └── BackendService.cs               # Implementación que usa HttpClient inyectado
│
├── 📁 Controllers/
│   ├── ConsumerController.cs           # Endpoint consumidor del backend
│   └── BackendController.cs            # Endpoint simulado del backend
│
├── Program.cs                          # Configuración de servicios y políticas
├── appsettings.json                    # Configuración de producción
├── appsettings.Development.json        # Configuración de desarrollo
├── Polly.Retry.Example.DelegatingHandler.csproj
└── README.md                           # Este archivo
```

## 🔧 Componentes Principales

### 1. **PollyDelegatingHandler.cs** - El Corazón del Proyecto

```csharp
public class PollyDelegatingHandler : System.Net.Http.DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await _policy.ExecuteAsync(
            async () => await base.SendAsync(request, cancellationToken),
            cancellationToken);

        return response;
    }
}
```

**¿Cómo funciona?**
- Hereda de `DelegatingHandler`, lo que permite interceptar solicitudes HTTP
- Recibe una política de Polly inyectada
- En `SendAsync()`, ejecuta la solicitud HTTP dentro del contexto de la política
- La política maneja reintentos, circuit breaker, etc. automáticamente

### 2. **BackendService.cs** - Servicio Simplificado

```csharp
public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BackendService> _logger;

    public BackendService(HttpClient httpClient, ILogger<BackendService> logger)
    {
        _httpClient = httpClient;  // HttpClient inyectado ya tiene el handler
        _logger = logger;
    }

    public async Task<string> GetDataAsync()
    {
        // El DelegatingHandler intercepta automáticamente esta llamada
        var response = await _httpClient.GetAsync("https://localhost:7056/api/backend/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Ventaja clave:** El servicio no necesita conocer sobre Polly. El handler se encarga transparentemente.

### 3. **Program.cs** - Configuración con DelegatingHandler

```csharp
// 1. Crear política de reintento
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
    .WaitAndRetryAsync<HttpResponseMessage>(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Reintentando... Intento {retryCount} después de {timespan.TotalSeconds}s");
        }
    );

// 2. Crear política de circuit breaker
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
    .CircuitBreakerAsync<HttpResponseMessage>(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(10)
    );

// 3. Combinar políticas
var combinedPolicy = Policy.WrapAsync<HttpResponseMessage>(retryPolicy, circuitBreakerPolicy);

// 4. Registrar handler con política
builder.Services.AddScoped<PollyDelegatingHandler>(sp =>
    new PollyDelegatingHandler(combinedPolicy)
);

// 5. Registrar HttpClient con el handler
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddHttpMessageHandler<PollyDelegatingHandler>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7056");
        client.Timeout = TimeSpan.FromSeconds(10);
    });
```

## 🎯 Políticas Implementadas

### Política de Reintento (Retry Policy)

- **Reintentos:** 3 intentos
- **Backoff:** Exponencial (2^n segundos)
  - 1er reintento: 2 segundos
  - 2do reintento: 4 segundos
  - 3er reintento: 8 segundos
- **Condiciones de activación:**
  - `HttpRequestException`
  - `OperationCanceledException`
  - Respuestas HTTP 5xx (errores del servidor)

### Política de Circuit Breaker

- **Umbral:** 3 fallos consecutivos
- **Duración de apertura:** 10 segundos
- **Estados:**
  - 🟢 **Closed**: Solicitudes normales
  - 🔴 **Open**: Se rechazan solicitudes después de 3 fallos
  - 🟡 **Half-Open**: Se permiten solicitudes de prueba después del timeout

### Combinación de Políticas

Ambas políticas se combinan usando `Policy.WrapAsync()`, permitiendo que funcionen conjuntamente:

```csharp
var combinedPolicy = Policy.WrapAsync<HttpResponseMessage>(retryPolicy, circuitBreakerPolicy);
```

## 🧪 Cómo Probar el Proyecto

### 1. **Iniciar la aplicación**

```pwsh
cd "F:\Data\Code\Polly.Retry.Example\Polly.Retry.Example.DelegatingHandler"
dotnet run
```

La aplicación se ejecutará en: `https://localhost:7056`

### 2. **Llamar al endpoint del consumidor**

```bash
curl https://localhost:7056/api/consumer/data
```

O abrir en navegador: `https://localhost:7056/swagger`

### 3. **Observar los reintentos**

En la consola verás algo como:

```
info: Polly.Retry.Example.DelegatingHandler.Controllers.ConsumerController[0]
      Consumer: Iniciando llamada a Backend con Polly retry (DelegatingHandler)
Reintentando... Intento 1 después de 2s
Reintentando... Intento 2 después de 4s
info: Polly.Retry.Example.DelegatingHandler.Controllers.BackendController[0]
      Backend: Respondiendo correctamente en llamada 3
info: Polly.Retry.Example.DelegatingHandler.Services.BackendService[0]
      Backend API respondió correctamente
```

### 4. **Resetear contador de fallos**

```bash
curl -X POST https://localhost:7056/api/backend/reset
```

## 📊 Flujo de Ejecución

```
Cliente HTTP
    ↓
ConsumerController.GetDataFromBackend()
    ↓
BackendService.GetDataAsync()
    ↓
HttpClient.GetAsync() ← Interceptado por
    ↓
PollyDelegatingHandler.SendAsync()
    ↓
Aplicar Políticas Polly
├─ Retry Policy (3 reintentos con backoff)
└─ Circuit Breaker Policy (3 fallos → circuito abierto)
    ↓
HttpClientHandler.SendAsync() (request real)
    ↓
Backend API
```

## 🚀 Ventajas del Enfoque DelegatingHandler

### 1. **Separación de Responsabilidades**
- La lógica de reintento está centralizada en el handler
- El servicio no necesita conocer sobre Polly

### 2. **Reutilización**
- El mismo handler puede utilizarse en múltiples clientes HTTP
- Las políticas se comparten fácilmente

### 3. **Testing**
- Fácil de mockear el `DelegatingHandler` para pruebas unitarias
- Permite aislar la lógica de reintento

### 4. **Flexibilidad**
- Posibilidad de tener diferentes handlers para diferentes escenarios
- Control explícito sobre las políticas

### 5. **Debuggeo**
- Código más legible y autodocumentado
- Fácil de seguir en el debugger

## 📦 Dependencias

- **.NET:** 8.0
- **Polly:** 8.4.1
- **Polly.Extensions.Http:** 3.0.0
- **Swashbuckle.AspNetCore:** 6.6.2 (Swagger)

## 🔄 Comparación con el Proyecto Original

### Proyecto Original (DI Method)

```csharp
builder.Services.AddHttpClient("BackendApi")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

var httpClient = _httpClientFactory.CreateClient("BackendApi");
```

### Este Proyecto (DelegatingHandler Method)

```csharp
builder.Services.AddScoped<PollyDelegatingHandler>(sp =>
    new PollyDelegatingHandler(combinedPolicy)
);

builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddHttpMessageHandler<PollyDelegatingHandler>();

// En el servicio:
var response = await _httpClient.GetAsync(...);
```

## ✅ Casos de Uso Recomendados

| Caso | Enfoque | Razón |
|------|---------|-------|
| Simple, 1-2 clientes HTTP | DI (`AddPolicyHandler`) | Menos código, más simple |
| Múltiples clientes con mismas políticas | **DelegatingHandler** | Reutilización, claridad |
| Lógica compleja o personalizada | **DelegatingHandler** | Mayor control y flexibilidad |
| Aplicación con muchos handlers | **DelegatingHandler** | Patrón más escalable |
| Microservicios con patrón consistente | **DelegatingHandler** | Estandarización |

## 📝 Notas Importantes

1. **Timeout:** Se configura en el `HttpClient` (10 segundos)
2. **Política sobre respuestas:** El handler trabaja con `HttpResponseMessage`, no solo excepciones
3. **Composición de políticas:** `Policy.WrapAsync()` permite combinar múltiples políticas
4. **Logging:** Todos los reintentos se loguean a la consola
5. **SSL:** La aplicación usa certificados autofirmados en desarrollo

## 🐛 Troubleshooting

### Problema: "Certificate validation error"
**Solución:** En desarrollo, esto es normal. Puedes deshabilitar la validación para testing.

### Problema: "No se ejecutan los reintentos"
**Solución:** Verifica que el endpoint del backend esté retornando 5xx para activar la política.

### Problema: "Circuit breaker siempre abierto"
**Solución:** Resetea el contador llamando a `POST /api/backend/reset`

## 📚 Recursos Adicionales

- [Documentación oficial de Polly](https://github.com/App-vNext/Polly)
- [DelegatingHandler en Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler)
- [HttpClientFactory en ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)

## 🎓 Conclusión

Este proyecto demuestra cómo usar **`DelegatingHandler`** para implementar patrones de resiliencia con Polly de manera explícita y mantenible. Es un enfoque más avanzado que proporciona mayor control y flexibilidad para aplicaciones complejas que requieren comunicaciones HTTP robustas.

---

**Autor:** Ejemplo de Polly con DelegatingHandler  
**Versión:** 1.0  
**.NET Target:** 8.0  
**Última actualización:** 2024
