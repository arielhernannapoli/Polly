# 🚀 Estructura del Workspace - Polly HttpClientFactory Examples

## 📂 Estructura de carpetas

```
C:\Users\ariel\Source\Repos\Polly.Retry.Example\
│
├── 📦 Polly.Retry.Example (Named HttpClient)
│   ├── Controllers/
│   │   ├── BackendController.cs      (API Backend que simula fallos)
│   │   ├── ConsumerController.cs     (API Consumer con IHttpClientFactory)
│   │   └── WeatherForecastController.cs
│   │
│   ├── Services/
│   │   ├── IBackendService.cs        (Interfaz del servicio)
│   │   └── BackendService.cs         (Usa IHttpClientFactory.CreateClient("BackendApi"))
│   │
│   ├── Program.cs                    (Configuración: AddHttpClient("BackendApi"))
│   ├── Polly.Retry.Example.csproj
│   ├── WeatherForecast.cs
│   └── README.md
│
├── 📦 Polly.Retry.Example.TypedClient (Typed HttpClient)
│   ├── Controllers/
│   │   ├── BackendController.cs      (API Backend que simula fallos)
│   │   ├── ConsumerController.cs     (API Consumer con Typed HttpClient)
│   │   └── WeatherForecastController.cs
│   │
│   ├── Services/
│   │   ├── IBackendService.cs        (Interfaz del servicio)
│   │   └── BackendService.cs         (Inyecta HttpClient directamente)
│   │
│   ├── Program.cs                    (Configuración: AddHttpClient<IBackendService, BackendService>())
│   ├── Polly.Retry.Example.TypedClient.csproj
│   ├── WeatherForecast.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Controllers/
│
└── 📄 README-COMPARACION.md          (Documentación comparativa)
```

---

## 🔌 Puertos

| Proyecto | Puerto | URL |
|----------|--------|-----|
| Named HttpClient | 7147 | https://localhost:7147 |
| Typed HttpClient | 7056 | https://localhost:7056 |

---

## 📊 Comparativa Rápida

### ✅ Proyecto 1: Named HttpClient
```csharp
// Program.cs
builder.Services.AddHttpClient("BackendApi")
    .AddPolicyHandler(retryPolicy);

// BackendService.cs
var httpClient = _httpClientFactory.CreateClient("BackendApi");
```
- ✅ Flexible y escalable
- ✅ Múltiples clientes
- ❌ Más código

### ✅ Proyecto 2: Typed HttpClient
```csharp
// Program.cs
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddPolicyHandler(retryPolicy);

// BackendService.cs
// Inyección directa: public BackendService(HttpClient httpClient)
```
- ✅ Simple y limpio
- ✅ Menos código
- ❌ Menos flexible

---

## 🧪 Endpoints para probar

### Proyecto 1 (Named HttpClient - Puerto 7147)
```bash
# Backend
curl -X GET "https://localhost:7147/api/backend/data" -k

# Consumer (con reintentos)
curl -X GET "https://localhost:7147/api/consumer/data" -k

# Reset
curl -X POST "https://localhost:7147/api/backend/reset" -k
```

### Proyecto 2 (Typed HttpClient - Puerto 7056)
```bash
# Backend
curl -X GET "https://localhost:7056/api/backend/data" -k

# Consumer (con reintentos)
curl -X GET "https://localhost:7056/api/consumer/data" -k

# Reset
curl -X POST "https://localhost:7056/api/backend/reset" -k
```

---

## 🚀 Cómo ejecutar ambos simultáneamente

### Terminal 1: Proyecto 1
```bash
cd Polly.Retry.Example
dotnet run
```

### Terminal 2: Proyecto 2
```bash
cd Polly.Retry.Example.TypedClient
dotnet run
```

---

## 📦 Dependencias comunes

Ambos proyectos tienen estas dependencias:
- `Polly` 8.4.1
- `Polly.Extensions.Http` 3.0.0
- `Microsoft.Extensions.Http.Polly` 8.0.0
- `Swashbuckle.AspNetCore` 6.6.2
- `.NET 8.0`

---

## 🎯 ¿Cuál usar?

**Usa Named HttpClient si:**
- Necesitas múltiples APIs
- Quieres máxima flexibilidad
- Diferentes servicios usan el mismo cliente

**Usa Typed HttpClient si:**
- Tienes una sola API a llamar
- Prefieres código limpio y simple
- Cada servicio es único

---

## 💡 Ambas implementan:

✅ **Polly Retry**: 3 reintentos con backoff exponencial
✅ **Circuit Breaker**: Protección ante fallos en cadena
✅ **HttpClientFactory**: Gestión óptima de conexiones
✅ **Logging**: Trazabilidad completa
✅ **ASP.NET Core 8**: Tecnología moderna

---

## 📚 Documentación adicional

- `README.md` en `Polly.Retry.Example/`
- `README-COMPARACION.md` en raíz
- Código comentado en ambos servicios

¡Ahora tienes ambas versiones funcionando! 🎉
