# Guía de Handlers Personalizados para Microsoft.Extensions.Http.Resilience

## 📌 ¿Qué hemos configurado?

Se han creado **extensiones personalizadas** (`AddCustomResilienceHandler`) en los 3 proyectos que permiten aplicar las políticas de resiliencia de Microsoft de forma reutilizable.

---

## 🎯 FORMAS DE USAR LOS HANDLERS

### **OPCIÓN 1: Mediante extensión personalizada (ACTUAL - Recomendada)**

```csharp
// En Program.cs
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();  // ✅ Limpio y reutilizable
```

**Archivo de extensión:**
```csharp
// Extensions/HttpClientExtensions.cs
public static class HttpClientExtensions
{
    public static IHttpStandardResiliencePipelineBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder)
    {
        return builder.AddStandardResilienceHandler();
    }
}
```

**Ventajas:**
- ✅ Código limpio
- ✅ Centralizado en una extensión
- ✅ Fácil de mantener
- ✅ Reusable

---

### **OPCIÓN 2: Sin extensión (más directo)**

```csharp
// En Program.cs
builder.Services.AddHttpClient("BackendApi")
    .AddStandardResilienceHandler();  // Directo del paquete
```

**Ventajas:**
- ✅ Menos código
- ✅ Más control explícito

**Desventajas:**
- ❌ No reutilizable
- ❌ Repetitivo si tienes múltiples clientes

---

### **OPCIÓN 3: Aplicar a múltiples clientes**

```csharp
// En Program.cs
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();

builder.Services.AddHttpClient("ExternalApi")
    .AddCustomResilienceHandler();

builder.Services.AddHttpClient("ThirdPartyApi")
    .AddCustomResilienceHandler();
```

**O más compacto:**

```csharp
// Crear una extensión que registra varios a la vez
public static IServiceCollection AddHttpClientsWithResilience(
    this IServiceCollection services,
    params string[] clientNames)
{
    foreach (var clientName in clientNames)
    {
        services
            .AddHttpClient(clientName)
            .AddCustomResilienceHandler();
    }
    return services;
}

// Usar:
builder.Services.AddHttpClientsWithResilience("BackendApi", "ExternalApi", "ThirdPartyApi");
```

---

### **OPCIÓN 4: Configuración global por perfil**

```csharp
// Extensions/HttpClientExtensions.cs
public static class HttpClientExtensions
{
    public static IHttpStandardResiliencePipelineBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder,
        ResilienceProfile profile = ResilienceProfile.Standard)
    {
        return builder.AddStandardResilienceHandler();
    }
}

public enum ResilienceProfile
{
    Standard,      // 3 reintentos
    Aggressive,    // 5 reintentos
    Conservative   // 2 reintentos
}

// Usar:
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler(ResilienceProfile.Aggressive);

builder.Services.AddHttpClient("CriticalApi")
    .AddCustomResilienceHandler(ResilienceProfile.Conservative);
```

---

## 📊 POLÍTICAS APLICADAS AUTOMÁTICAMENTE

Cuando usas `AddStandardResilienceHandler()`, Microsoft aplica automáticamente:

| Política | Configuración por defecto |
|----------|---------------------------|
| **Retry** | 3 intentos con backoff exponencial |
| **Circuit Breaker** | Umbral de 50%, duración 30 segundos |
| **Timeout** | 10 segundos |

---

## 🔧 COMPARATIVA ANTES vs DESPUÉS

### ❌ ANTES (Polly.Extensions.Http)
```csharp
using Polly;
using Polly.Extensions.Http;

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(retryCount: 3, ...);

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(...);

builder.Services.AddHttpClient("BackendApi")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);
```

### ✅ DESPUÉS (Microsoft.Extensions.Http.Resilience)
```csharp
using Microsoft.Extensions.Http.Resilience;

builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();  // ¡Todo en una línea!
```

---

## 📝 CONFIGURACIÓN AVANZADA

Si necesitas customizar las políticas (aunque `AddStandardResilienceHandler()` no expone parámetros):

```csharp
// Una alternativa es crear tu propio DelegatingHandler
public class CustomResilienceDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Aquí implementas tu lógica personalizada
        return await base.SendAsync(request, cancellationToken);
    }
}

// Registrar:
builder.Services.AddHttpClient("BackendApi")
    .AddHttpMessageHandler<CustomResilienceDelegatingHandler>()
    .AddStandardResilienceHandler();  // También agregar la resiliencia estándar
```

---

## 🎓 RESUMEN

| Escenario | Recomendación |
|-----------|---------------|
| Cliente único | Opción 1 o 2 |
| Múltiples clientes | Opción 3 |
| Diferentes perfiles | Opción 4 |
| Lógica custom compleja | DelegatingHandler personalizado |

**ELEGIDA PARA ESTE PROYECTO:** Opción 1 (Extensión personalizada)

---

## 📌 ARCHIVOS CREADOS

- ✅ `Polly.Retry.Example/Extensions/HttpClientExtensions.cs`
- ✅ `Polly.Retry.Example.TypedClient/Extensions/HttpClientExtensions.cs`
- ✅ `Polly.Retry.Example.DelegatingHandler/Extensions/HttpClientExtensions.cs`

Todos usan la misma pauta: encapsular `AddStandardResilienceHandler()` en una extensión reutilizable.

