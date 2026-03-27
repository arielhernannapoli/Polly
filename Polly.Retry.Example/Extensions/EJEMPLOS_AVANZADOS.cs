using Microsoft.Extensions.Http.Resilience;

namespace Polly.Retry.Example.Extensions
{
    /// <summary>
    /// EJEMPLOS AVANZADOS DE EXTENSIONES PERSONALIZADAS
    /// Descomenta y adapta según tus necesidades
    /// </summary>
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 1: Extensión con Delegados para Callbacks
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public static class AdvancedHttpClientExtensions
    // {
    //     /// <summary>
    //     /// Extensión que permite pasar un callback cuando falla una petición
    //     /// </summary>
    //     public static IHttpStandardResiliencePipelineBuilder AddResilienceWithCallbacks(
    //         this IHttpClientBuilder builder,
    //         Action<Exception>? onFailure = null,
    //         Action? onSuccess = null)
    //     {
    //         // Agregar handler personalizado si es necesario
    //         builder.AddHttpMessageHandler(sp => new CallbackDelegatingHandler(onFailure, onSuccess));
    //         
    //         // Agregar resiliencia estándar
    //         return builder.AddStandardResilienceHandler();
    //     }
    // }
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 2: DelegatingHandler Personalizado para Logging
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public class LoggingDelegatingHandler : DelegatingHandler
    // {
    //     private readonly ILogger<LoggingDelegatingHandler> _logger;
    //
    //     public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    //     {
    //         _logger = logger;
    //     }
    //
    //     protected override async Task<HttpResponseMessage> SendAsync(
    //         HttpRequestMessage request,
    //         CancellationToken cancellationToken)
    //     {
    //         _logger.LogInformation("📤 Request: {Method} {Uri}", request.Method, request.RequestUri);
    //         
    //         var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    //         var response = await base.SendAsync(request, cancellationToken);
    //         stopwatch.Stop();
    //         
    //         _logger.LogInformation(
    //             "📥 Response: {StatusCode} - {Duration}ms",
    //             response.StatusCode,
    //             stopwatch.ElapsedMilliseconds);
    //         
    //         return response;
    //     }
    // }
    //
    // // Uso:
    // // builder.Services
    // //     .AddScoped<LoggingDelegatingHandler>()
    // //     .AddHttpClient("BackendApi")
    // //     .AddHttpMessageHandler<LoggingDelegatingHandler>()
    // //     .AddCustomResilienceHandler();
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 3: DelegatingHandler para Agregar Headers Personalizados
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public class HeadersDelegatingHandler : DelegatingHandler
    // {
    //     private readonly string _apiKey;
    //
    //     public HeadersDelegatingHandler(string apiKey)
    //     {
    //         _apiKey = apiKey;
    //     }
    //
    //     protected override async Task<HttpResponseMessage> SendAsync(
    //         HttpRequestMessage request,
    //         CancellationToken cancellationToken)
    //     {
    //         // Agregar headers personalizados
    //         request.Headers.Add("X-API-Key", _apiKey);
    //         request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());
    //         request.Headers.Add("User-Agent", "MyApp/1.0");
    //         
    //         return await base.SendAsync(request, cancellationToken);
    //     }
    // }
    //
    // // Uso:
    // // builder.Services.AddHttpClient("ExternalApi")
    // //     .AddHttpMessageHandler(sp => new HeadersDelegatingHandler("myapikey123"))
    // //     .AddCustomResilienceHandler();
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 4: Extensión con Múltiples DelegatingHandlers
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public static class CompositeHttpClientExtensions
    // {
    //     /// <summary>
    //     /// Aplica un stack completo de handlers: logging, headers, resiliencia
    //     /// </summary>
    //     public static IHttpStandardResiliencePipelineBuilder AddFullStackResilience(
    //         this IHttpClientBuilder builder,
    //         string apiKey)
    //     {
    //         builder
    //             .AddHttpMessageHandler(sp => 
    //                 new LoggingDelegatingHandler(sp.GetRequiredService<ILogger<LoggingDelegatingHandler>>()))
    //             .AddHttpMessageHandler(sp => new HeadersDelegatingHandler(apiKey));
    //         
    //         return builder.AddStandardResilienceHandler();
    //     }
    // }
    //
    // // Uso:
    // // builder.Services.AddHttpClient("FullStackClient")
    // //     .AddFullStackResilience("myapikey");
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 5: DelegatingHandler para Métricas/Telemetría
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public class MetricsDelegatingHandler : DelegatingHandler
    // {
    //     private readonly IMetricsService _metrics;
    //
    //     public MetricsDelegatingHandler(IMetricsService metrics)
    //     {
    //         _metrics = metrics;
    //     }
    //
    //     protected override async Task<HttpResponseMessage> SendAsync(
    //         HttpRequestMessage request,
    //         CancellationToken cancellationToken)
    //     {
    //         var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    //         
    //         try
    //         {
    //             var response = await base.SendAsync(request, cancellationToken);
    //             stopwatch.Stop();
    //             
    //             _metrics.RecordHttpRequest(
    //                 request.RequestUri?.Host ?? "unknown",
    //                 (int)response.StatusCode,
    //                 stopwatch.ElapsedMilliseconds);
    //             
    //             return response;
    //         }
    //         catch (Exception ex)
    //         {
    //             stopwatch.Stop();
    //             _metrics.RecordHttpRequestError(
    //                 request.RequestUri?.Host ?? "unknown",
    //                 ex.GetType().Name,
    //                 stopwatch.ElapsedMilliseconds);
    //             throw;
    //         }
    //     }
    // }
    
    // ═══════════════════════════════════════════════════════════════════════════════
    // EJEMPLO 6: Extensión con Configuración desde appsettings.json
    // ═══════════════════════════════════════════════════════════════════════════════
    
    // public class HttpClientConfig
    // {
    //     public string BaseUrl { get; set; }
    //     public int TimeoutSeconds { get; set; } = 30;
    //     public string ApiKey { get; set; }
    // }
    //
    // public static class ConfigurableHttpClientExtensions
    // {
    //     public static IHttpStandardResiliencePipelineBuilder AddConfiguredClient(
    //         this IHttpClientBuilder builder,
    //         IConfiguration configuration,
    //         string sectionName)
    //     {
    //         var config = configuration.GetSection(sectionName).Get<HttpClientConfig>();
    //         
    //         builder.ConfigureHttpClient(client =>
    //         {
    //             client.BaseAddress = new Uri(config.BaseUrl);
    //             client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
    //         });
    //         
    //         if (!string.IsNullOrEmpty(config.ApiKey))
    //         {
    //             builder.AddHttpMessageHandler(sp => new HeadersDelegatingHandler(config.ApiKey));
    //         }
    //         
    //         return builder.AddStandardResilienceHandler();
    //     }
    // }
    //
    // // appsettings.json:
    // // {
    // //   "HttpClients": {
    // //     "BackendApi": {
    // //       "BaseUrl": "https://api.example.com",
    // //       "TimeoutSeconds": 10,
    // //       "ApiKey": "secret-key"
    // //     }
    // //   }
    // // }
    //
    // // Uso en Program.cs:
    // // builder.Services.AddHttpClient("BackendApi")
    // //     .AddConfiguredClient(builder.Configuration, "HttpClients:BackendApi");
}
