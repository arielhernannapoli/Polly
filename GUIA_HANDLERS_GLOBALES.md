# 🌐 Guía Completa: Handlers Personalizados Globales

## ÍNDICE
1. [Resumen Rápido](#resumen-rápido)
2. [Opciones de Implementación](#opciones-de-implementación)
3. [Ejemplos Prácticos](#ejemplos-prácticos)
4. [Troubleshooting](#troubleshooting)

---

## Resumen Rápido

**¿Se puede aplicar `AddStandardResilienceHandler` globalmente?**

**SÍ**, hay varias formas:

| Opción | Código | Complejidad | Recomendación |
|--------|--------|------------|---------------|
| **1. Extensión simple** | `AddCustomResilienceHandler()` | ⭐ Baja | ✅ ACTUAL |
| **2. Directo** | `AddStandardResilienceHandler()` | ⭐ Muy baja | Para casos simples |
| **3. Múltiples clientes** | Loop + extensión | ⭐⭐ Media | Varios clientes |
| **4. Con DelegatingHandlers** | Stack de handlers | ⭐⭐⭐ Alta | Lógica compleja |
| **5. Configuración desde JSON** | IConfiguration | ⭐⭐⭐⭐ Muy alta | Producción |

---

## Opciones de Implementación

### ✅ OPCIÓN 1: EXTENSIÓN SIMPLE (ACTUAL)

**Archivo:** `Extensions/HttpClientExtensions.cs`

```csharp
public static class HttpClientExtensions
{
    /// Aplicar a un cliente
    public static IHttpStandardResiliencePipelineBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder)
    {
        return builder.AddStandardResilienceHandler();
    }
}
```

**Uso en Program.cs:**
```csharp
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();
```

**✅ Ventajas:**
- Código limpio
- Fácil de mantener
- Centralizado

**❌ Desventajas:**
- Debes llamarlo para cada cliente
- No aplica globalmente por defecto

---

### ✅ OPCIÓN 2: APLICAR A VARIOS CLIENTES (LOOP)

```csharp
var clientNames = new[] { "BackendApi", "ExternalApi", "ThirdPartyApi" };

foreach (var clientName in clientNames)
{
    builder.Services.AddHttpClient(clientName)
        .AddCustomResilienceHandler();
}

// O más elegante:
builder.Services.AddHttpClientsWithResilience("BackendApi", "ExternalApi", "ThirdPartyApi");
```

**Extensión:**
```csharp
public static class MultiHttpClientExtensions
{
    public static IServiceCollection AddHttpClientsWithResilience(
        this IServiceCollection services,
        params string[] clientNames)
    {
        foreach (var clientName in clientNames)
        {
            services.AddHttpClient(clientName)
                .AddCustomResilienceHandler();
        }
        return services;
    }
}
```

---

### ✅ OPCIÓN 3: STACK DE HANDLERS (CON LOGGING)

```csharp
builder.Services
    .AddScoped<LoggingDelegatingHandler>()
    .AddHttpClient("BackendApi")
    .AddHttpMessageHandler<LoggingDelegatingHandler>()  // Ejecuta primero
    .AddCustomResilienceHandler();                       // Después resiliencia
```

**Handler de Logging:**
```csharp
public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📤 {Method} {Uri}", request.Method, request.RequestUri);
        
        var response = await base.SendAsync(request, cancellationToken);
        
        _logger.LogInformation("📥 {StatusCode}", response.StatusCode);
        
        return response;
    }
}
```

---

### ✅ OPCIÓN 4: CONFIGURACIÓN DESDE appsettings.json

**appsettings.json:**
```json
{
  "HttpClients": {
    "BackendApi": {
      "BaseUrl": "https://localhost:7056",
      "TimeoutSeconds": 10,
      "AddLogging": true
    },
    "ExternalApi": {
      "BaseUrl": "https://api.external.com",
      "TimeoutSeconds": 30,
      "AddLogging": false
    }
  }
}
```

**Extensión:**
```csharp
public class HttpClientConfig
{
    public string BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public bool AddLogging { get; set; }
}

public static class ConfigurableHttpClientExtensions
{
    public static IServiceCollection AddHttpClientsFromConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var httpClientsSection = configuration.GetSection("HttpClients");
        
        foreach (var clientConfig in httpClientsSection.GetChildren())
        {
            var clientName = clientConfig.Key;
            var config = clientConfig.Get<HttpClientConfig>();
            
            var httpClientBuilder = services
                .AddHttpClient(clientName)
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(config.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
                });
            
            if (config.AddLogging)
            {
                httpClientBuilder.AddHttpMessageHandler<LoggingDelegatingHandler>();
            }
            
            httpClientBuilder.AddCustomResilienceHandler();
        }
        
        return services;
    }
}
```

**Uso en Program.cs:**
```csharp
// Registrar handlers
builder.Services.AddScoped<LoggingDelegatingHandler>();

// Registrar todos los clientes desde config
builder.Services.AddHttpClientsFromConfig(builder.Configuration);
```

---

### ✅ OPCIÓN 5: MIDDLEWARE GLOBAL (Si usas HttpClientFactory)

```csharp
// En Program.cs
builder.Services.AddHttpClient("", httpClient =>
{
    // Configuración por defecto para TODOS los clientes sin nombre
    httpClient.Timeout = TimeSpan.FromSeconds(10);
})
.AddCustomResilienceHandler();
```

⚠️ **Nota:** Esto no aplica a clientes nombrados.

---

## Ejemplos Prácticos

### 📌 Ejemplo 1: Proyecto Simple (Lo que tienes ahora)

```csharp
// Program.cs
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();

// Usar en servicio
var httpClient = httpClientFactory.CreateClient("BackendApi");
var response = await httpClient.GetAsync("https://localhost:7056/data");
```

---

### 📌 Ejemplo 2: Múltiples Clientes con Diferentes Configuraciones

```csharp
// Extensión para perfiles
public static class ProfiledHttpClientExtensions
{
    public static IHttpStandardResiliencePipelineBuilder AddResilienceByProfile(
        this IHttpClientBuilder builder,
        string profile = "Standard")
    {
        // Aquí podrías aplicar diferentes configuraciones según el perfil
        // Por ahora, AddStandardResilienceHandler es igual para todos
        return builder.AddStandardResilienceHandler();
    }
}

// Uso
builder.Services.AddHttpClient("CriticalApi")
    .AddResilienceByProfile("Aggressive");  // Más reintentos

builder.Services.AddHttpClient("NonCriticalApi")
    .AddResilienceByProfile("Conservative");  // Menos reintentos
```

---

### 📌 Ejemplo 3: Combo - Logging + Resiliencia

```csharp
// Program.cs
builder.Services.AddScoped<LoggingDelegatingHandler>();

builder.Services.AddHttpClient("BackendApi")
    .AddHttpMessageHandler<LoggingDelegatingHandler>()
    .AddCustomResilienceHandler();

// OUTPUT:
// 📤 GET https://localhost:7056/data
// 📥 200 OK
```

---

### 📌 Ejemplo 4: Con Headers Personalizados

```csharp
public class AuthDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", "Bearer token123");
        return await base.SendAsync(request, cancellationToken);
    }
}

// Uso
builder.Services.AddScoped<AuthDelegatingHandler>();

builder.Services.AddHttpClient("SecureApi")
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .AddCustomResilienceHandler();
```

---

## Troubleshooting

### ❓ P: ¿Cómo sé si se aplica realmente?

R: Agrega logging en un DelegatingHandler:

```csharp
public class DebugDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(...)
    {
        Console.WriteLine("✅ Handler ejecutándose");
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

### ❓ P: ¿Cuál es el orden de ejecución de los handlers?

R: **De abajo hacia arriba**:

```csharp
builder.Services.AddHttpClient("Api")
    .AddHttpMessageHandler<AuthHandler>()      // 3. Ejecuta último
    .AddHttpMessageHandler<LoggingHandler>()   // 2. Ejecuta segundo
    .AddCustomResilienceHandler();             // 1. Ejecuta primero
```

Flujo: ResilienceHandler → LoggingHandler → AuthHandler → Servidor HTTP

---

### ❓ P: ¿Puedo aplicar globalmente SIN nombre?

R: Técnicamente no. Debes iterar sobre los nombres. Pero puedes crear una extensión:

```csharp
public static IServiceCollection AddGlobalHttpClientResilience(
    this IServiceCollection services)
{
    // Registra un cliente "default"
    services.AddHttpClient()
        .AddCustomResilienceHandler();
    
    // Luego úsalo:
    // var client = httpClientFactory.CreateClient();
    return services;
}
```

---

## 📊 Matriz de Decisión

**¿Cuál opción elegir?**

| Situación | Opción | Razón |
|-----------|--------|-------|
| 1 cliente | 1 (Extensión simple) | Simple y directo |
| 2-5 clientes | 2 (Loop) | Reutilizable |
| Múltiples con diferencias | 3 (Stack) | Flexible |
| Producción con config | 4 (JSON config) | Mantenible |
| Lógica muy compleja | 5 + custom handlers | Control total |

---

## ✅ RECOMENDACIÓN FINAL

**Para tu caso (3 proyectos):**

✅ **OPCIÓN 1** - Extensión simple (ACTUAL)
- Limpio
- Mantenible
- Suficiente para tus necesidades

Si crece a +10 clientes: **OPCIÓN 4** (JSON config)

---

## 📁 Archivos de Referencia

- ✅ `Extensions/HttpClientExtensions.cs` - Extensión actual
- 📄 `EJEMPLOS_AVANZADOS.cs` - Ejemplos comentados
- 📄 `HANDLERS_PERSONALIZADOS.md` - Documentación detallada

