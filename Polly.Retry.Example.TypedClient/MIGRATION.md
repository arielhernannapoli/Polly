# Guía de Migración - Polly.Retry.Example.TypedClient

## 📋 Resumen

Este proyecto ha sido migrado a **Microsoft.Extensions.Http.Resilience** usando el patrón **Typed HttpClient**.

## 🚀 Patrón: Typed HttpClient

El **Typed HttpClient** es el patrón recomendado por Microsoft para:
- ✅ Type-safety
- ✅ Dependency Injection
- ✅ Testing
- ✅ Escalabilidad

## 💻 Estructura

```csharp
// 1. Definir contrato
public interface IBackendService
{
    Task<string> GetDataAsync();
}

// 2. Implementar con HttpClient inyectado
public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;

    public BackendService(HttpClient httpClient) // ← Inyectado automáticamente
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("...");
        return await response.Content.ReadAsStringAsync();
    }
}

// 3. Registrar en Program.cs
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddCustomResilienceHandler();

// 4. Usar en controlador
public class ConsumerController : ControllerBase
{
    public ConsumerController(IBackendService service) // ← Inyectado automáticamente
    {
        _service = service;
    }
}
```

## ✅ Checklist de Migración

- [x] Actualizar .csproj
- [x] Crear HttpClientExtensions.cs con AddCustomResilienceHandler()
- [x] Actualizar Program.cs
- [x] Verificar servicios usan HttpClient
- [x] Compilación exitosa ✅

## 🔧 Extensiones Disponibles

### `AddCustomResilienceHandler()`
Para un cliente Typed:

```csharp
builder.Services.AddHttpClient<IService, Service>()
    .AddCustomResilienceHandler();
```

### `AddTypedHttpClientsWithResilience<T1, T2>()`
Para múltiples Typed Clients:

```csharp
builder.Services.AddTypedHttpClientsWithResilience<IService1, Service1>();
builder.Services.AddTypedHttpClientsWithResilience<IService2, Service2>();
```

## 📊 Ventajas vs HttpClientFactory

| Feature | HttpClientFactory | Typed HttpClient |
|---------|------------------|-----------------|
| Type-safe | ❌ | ✅ |
| IntelliSense | ❌ | ✅ |
| DI automático | Manual | ✅ Automático |
| Testing | Difícil | Fácil (mock) |
| Refactoring | Peligroso | Seguro |

## 🎯 Cuándo usar Typed HttpClient

**USA TYPED HTTPCLIENT CUANDO:**
- ✅ Necesitas type-safety
- ✅ Múltiples métodos HTTP
- ✅ Aplicación enterprise
- ✅ Código legacy que migras

**USA HTTPCLIENTFACTORY CUANDO:**
- ✅ Necesitas clientes dinámicos
- ✅ Muchos clientes (20+)
- ✅ Configuración en runtime

## 📚 Documentación

- 📖 README.md
- 📖 GUIA_HANDLERS_GLOBALES.md
- 📖 EJEMPLOS_AVANZADOS.cs

---

**Estado:** ✅ COMPLETADO  
**Versión .NET:** 8.0
