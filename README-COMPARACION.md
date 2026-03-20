# Polly Retry Examples - Ambas Implementaciones

Este repositorio contiene dos proyectos que demuestran diferentes formas de usar **HttpClientFactory** con **Polly .NET** para reintentos automáticos.

## 📁 Proyectos

### 1. `Polly.Retry.Example` (Cliente Nombrado)
- Usa `IHttpClientFactory` con cliente **nombrado** (`"BackendApi"`)
- Puerto: **7147**

### 2. `Polly.Retry.Example.TypedClient` (Typed HttpClient)
- Usa **Typed HttpClient** con inyección de tipo
- Puerto: **7056**

---

## 🔄 Comparativa de Implementaciones

### ✅ **Opción 1: Cliente Nombrado** (`Polly.Retry.Example`)

**Configuración en `Program.cs`:**
```csharp
builder.Services.AddHttpClient("BackendApi")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddScoped<IBackendService, BackendService>();
```

**Uso en el servicio:**
```csharp
public class BackendService : IBackendService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public BackendService(IHttpClientFactory httpClientFactory, ILogger<BackendService> logger)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetDataAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("BackendApi");
        var response = await httpClient.GetAsync("https://localhost:7056/api/backend/data");
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Ventajas:**
- ✅ Más flexible - múltiples clientes nombrados
- ✅ Reutilizable por diferentes servicios
- ✅ Fácil de escalar con nuevas APIs

**Desventajas:**
- ❌ Más inyecciones (IHttpClientFactory)
- ❌ Requiere recordar el nombre del cliente

---

### ✅ **Opción 2: Typed HttpClient** (`Polly.Retry.Example.TypedClient`)

**Configuración en `Program.cs`:**
```csharp
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);
```

**Uso en el servicio:**
```csharp
public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;
    
    public BackendService(HttpClient httpClient, ILogger<BackendService> logger)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("https://localhost:7056/api/backend/data");
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Ventajas:**
- ✅ Más limpio y simple
- ✅ Inyección directa (HttpClient)
- ✅ Menos código boilerplate
- ✅ Mejor para servicios específicos

**Desventajas:**
- ❌ Un tipo por servicio (menos flexible)
- ❌ Difícil compartir entre múltiples servicios

---

## 🚀 Cómo ejecutar

### Proyecto 1: Cliente Nombrado
```bash
cd Polly.Retry.Example
dotnet run
# Puerto: https://localhost:7147
```

### Proyecto 2: Typed HttpClient
```bash
cd Polly.Retry.Example.TypedClient
dotnet run
# Puerto: https://localhost:7056
```

---

## 🧪 Endpoints disponibles

En ambos proyectos:

```bash
# Llamar a Backend (simula fallos)
GET /api/backend/data

# Llamar a Consumer (con reintentos de Polly)
GET /api/consumer/data

# Resetear contador de fallos
POST /api/backend/reset
```

### Ejemplo con Cliente Nombrado:
```bash
curl -X GET "https://localhost:7147/api/consumer/data" -k
```

### Ejemplo con Typed HttpClient:
```bash
curl -X GET "https://localhost:7056/api/consumer/data" -k
```

---

## 📊 Flujo de ejecución

```
Cliente
   ↓
Consumer API (/api/consumer/data)
   ↓
[BackendService con Polly]
   ├─ Intento 1 (fallos) → Espera 2s
   ├─ Intento 2 (fallos) → Espera 4s
   ├─ Intento 3 (éxito) → Devuelve datos
   └─ Circuit Breaker (si 3 fallos en fila)
   ↓
Backend API (/api/backend/data)
```

---

## 🎯 Políticas de Polly en ambos proyectos

### Retry (Reintento)
- **Máximo 3 reintentos**
- **Backoff exponencial:**
  - 1er reintento: 2 segundos
  - 2do reintento: 4 segundos
  - 3er reintento: 8 segundos

### Circuit Breaker
- Se abre después de 3 fallos consecutivos
- Se mantiene abierto 10 segundos
- Evita sobrecargar el backend

---

## 💡 Cuándo usar cada una

| Escenario | Recomendación |
|-----------|---------------|
| **Una API única** | Typed HttpClient |
| **Múltiples APIs diferentes** | Cliente Nombrado |
| **Compartir entre servicios** | Cliente Nombrado |
| **Simplicidad máxima** | Typed HttpClient |
| **Escalabilidad** | Cliente Nombrado |

---

## 🔧 Paquetes NuGet utilizados

- `Polly` (8.4.1)
- `Polly.Extensions.Http` (3.0.0)
- `Microsoft.Extensions.Http.Polly` (8.0.0)
- `Swashbuckle.AspNetCore` (6.6.2)

---

## 📝 Logs esperados

Verás en consola:

```
[TypedClient] Reintentando... Intento 1 después de 2s
[TypedClient] Reintentando... Intento 2 después de 4s
[TypedClient] Backend: Respondiendo correctamente en llamada 3
[TypedClient] Consumer: Iniciando llamada a Backend con Typed HttpClient y Polly retry
```

---

## 🎓 Conclusión

Ambas implementaciones son excelentes. La elección depende de:
- **Complejidad**: ¿Cuántas APIs necesitas llamar?
- **Flexibilidad**: ¿Necesitas compartir clientes?
- **Mantenibilidad**: ¿Qué es más limpio para tu equipo?

¡Usa el proyecto que mejor se adapte a tus necesidades! 🚀
