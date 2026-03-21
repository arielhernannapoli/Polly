# Polly.Retry.Example.TypedClient

## Descripción

Este proyecto es un ejemplo de implementación de un cliente HTTP tipado (Typed HttpClient) en ASP.NET Core 8.0 utilizando la librería **Polly** para gestionar resiliencia mediante políticas de reintentos y circuit breaker.

## Características

- **Typed HttpClient**: Inyección de dependencias de un cliente HTTP tipado
- **Política de Reintentos**: Implementa reintentos exponenciales con backoff (hasta 3 intentos)
- **Circuit Breaker**: Detecta fallos transitorios y corta el circuito después de 3 errores
- **Swagger/OpenAPI**: Documentación interactiva de la API
- **Logging**: Información detallada de reintentos y errores

## Estructura del Proyecto

```
Polly.Retry.Example.TypedClient/
├── Controllers/
│   ├── ConsumerController.cs       # Punto de entrada de la API
│   └── BackendController.cs        # Simulación del servicio backend
├── Services/
│   ├── IBackendService.cs          # Interfaz del cliente tipado
│   └── BackendService.cs           # Implementación del cliente HTTP tipado
├── Program.cs                      # Configuración de servicios y políticas
└── README.md
```

## Componentes Principales

### 1. **Program.cs**
Configura:
- Política de reintentos con backoff exponencial (2^n segundos)
- Política de circuit breaker (3 errores consecutivos = 10 segundos de corte)
- Inyección del Typed HttpClient

### 2. **IBackendService / BackendService**
- `IBackendService`: Contrato para llamadas al backend
- `BackendService`: Implementación del cliente HTTP tipado con HttpClient inyectado

### 3. **ConsumerController**
Endpoint que utiliza el cliente tipado para llamar al backend:
- `GET /api/consumer/data` - Obtiene datos del backend con reintentos automáticos

### 4. **BackendController**
Simula un servicio backend con posibles errores transitorios para demostrar el funcionamiento de Polly.

## Políticas de Polly

### Retry Policy (Política de Reintentos)
```csharp
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"[TypedClient] Reintentando... Intento {retryCount} después de {timespan.TotalSeconds}s");
        }
    );
```
- **Reintentos**: 3 intentos máximo
- **Backoff**: Exponencial (1s, 2s, 4s)
- **Manejo de errores**: Códigos HTTP 5xx y timeouts

### Circuit Breaker Policy (Política de Circuit Breaker)
```csharp
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(10)
    );
```
- **Umbral de apertura**: 3 errores consecutivos
- **Duración de corte**: 10 segundos
- **Previene**: Sobrecargar un servicio degradado

## Cómo Usar

### Ejecutar el proyecto
```bash
dotnet run
```

### Probar el endpoint
1. Abre Swagger en: `https://localhost:5001/swagger`
2. Llama a `GET /api/consumer/data`
3. Observa los reintentos en la consola

### Monitorear reintentos
La consola mostrará mensajes como:
```
[TypedClient] Consumer: Iniciando llamada a Backend con Typed HttpClient y Polly retry
[TypedClient] Reintentando... Intento 1 después de 1s
[TypedClient] Reintentando... Intento 2 después de 2s
[TypedClient] Reintentando... Intento 3 después de 4s
```

## Ventajas del Typed HttpClient

✅ **Type-safe**: Reutilización de interfaces fuertemente tipadas  
✅ **Inyección de dependencias**: Integración con el contenedor de IoC  
✅ **Configuración centralizada**: Políticas y headers en un único lugar  
✅ **Fácil testing**: Permite mock de la interfaz para pruebas  
✅ **Ciclo de vida gestionado**: El contenedor controla la creación y liberación de recursos  

## Comparación con otros patrones

Este proyecto es parte de una serie de ejemplos:
- **DelegatingHandler**: Implementación con delegating handlers personalizados
- **TypedClient**: Implementación con Typed HttpClient (este proyecto)
- **Base**: Implementación simple de reintentos

## Dependencias

- .NET 8.0
- Polly 8.x
- ASP.NET Core 8.0
- Microsoft.AspNetCore.Mvc

## Licencia

Este proyecto es un ejemplo educativo basado en la librería Polly.

## Referencias

- [Documentación de Polly](https://github.com/App-vNext/Polly)
- [Microsoft: Implementar reintentos con backoff exponencial](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff)
- [Typed HttpClient en ASP.NET Core](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
