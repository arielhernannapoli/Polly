# Polly Retry Example

Este proyecto demuestra cómo usar **Polly .NET** para implementar reintentos automáticos (retry) y circuit breaker en llamadas HTTP entre dos APIs en el mismo proyecto.

## Arquitectura

El proyecto contiene dos APIs:

### 1. **Backend API** (`/api/backend/data`)
- Endpoint que simula un servicio backend que falla aleatoriamente
- Falla 2 de cada 3 intentos para demostrar los reintentos de Polly

### 2. **Consumer API** (`/api/consumer/data`)
- Endpoint que llama a la Backend API
- Implementa reintentos automáticos con Polly
- Aplica políticas de retry y circuit breaker

## Características de Polly

### Política de Reintento (Retry)
```csharp
- Máximo 3 reintentos
- Backoff exponencial: 2^n segundos
  - 1er reintento: 2 segundos
  - 2do reintento: 4 segundos
  - 3er reintento: 8 segundos
```

### Política de Circuit Breaker
```csharp
- Se abre después de 3 fallos consecutivos
- Se mantiene abierto durante 10 segundos
- Evita sobrecargar el servicio backend
```

## Cómo usar

### 1. Ejecutar la aplicación
```powershell
dotnet run
```

### 2. Llamar a la Consumer API (que usa reintentos)
```bash
curl -X GET "https://localhost:7147/api/consumer/data" -k
```

Verás en la consola los intentos de reintento:
```
Reintentando... Intento 1 después de 2s
Reintentando... Intento 2 después de 4s
Respuesta exitosa
```

### 3. Llamar a la Backend API directamente
```bash
curl -X GET "https://localhost:7147/api/backend/data" -k
```

### 4. Resetear el contador de fallos
```bash
curl -X POST "https://localhost:7147/api/backend/reset" -k
```

## Estructura de carpetas

```
Polly.Retry.Example/
├── Controllers/
│   ├── BackendController.cs      # API que simula fallos
│   ├── ConsumerController.cs     # API que llama con reintentos
│   └── WeatherForecastController.cs
├── Services/
│   ├── IBackendService.cs        # Interfaz del servicio
│   └── BackendService.cs         # Implementación con HttpClient
├── Program.cs                    # Configuración de Polly
└── Polly.Retry.Example.csproj   # Dependencias
```

## Paquetes NuGet utilizados

- **Polly** (8.4.1) - Framework de resiliencia
- **Polly.Extensions.Http** (3.0.0) - Extensiones para HTTP
- **Microsoft.Extensions.Http.Polly** (8.0.0) - Integración con ASP.NET Core

## Flujo de ejecución

```
Cliente → Consumer API (GET /api/consumer/data)
         ↓
         BackendService (con Polly)
         ↓ (Reintenta con backoff exponencial)
         Backend API (GET /api/backend/data)
         ↓
         Devuelve datos o error después de 3 reintentos
```

## Próximos pasos

Puedes extender este ejemplo con:
- Políticas de timeout
- Bulkhead isolation
- Fallback policies
- Métricas y telemetría
