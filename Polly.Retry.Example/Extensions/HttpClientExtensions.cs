using Microsoft.Extensions.Http.Resilience;

namespace Polly.Retry.Example.Extensions
{
    /// <summary>
    /// Extensiones para configurar HttpClient con políticas de resiliencia estándar
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Aplica el handler de resiliencia estándar (reintento, circuit breaker, timeout)
        /// Configuración predefinida por Microsoft
        /// </summary>
        public static IHttpStandardResiliencePipelineBuilder AddCustomResilienceHandler(this IHttpClientBuilder builder)
        {
            return builder.AddStandardResilienceHandler();
        }

        /// <summary>
        /// Alias para AddCustomResilienceHandler
        /// </summary>
        public static IHttpStandardResiliencePipelineBuilder AddAggressiveResilienceHandler(this IHttpClientBuilder builder)
        {
            return builder.AddStandardResilienceHandler();
        }

        /// <summary>
        /// Alias para AddCustomResilienceHandler
        /// </summary>
        public static IHttpStandardResiliencePipelineBuilder AddConservativeResilienceHandler(this IHttpClientBuilder builder)
        {
            return builder.AddStandardResilienceHandler();
        }

        /// <summary>
        /// Registra múltiples HttpClients con resiliencia estándar
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="clientNames">Nombres de los clientes a registrar</param>
        /// <returns>Colección de servicios para encadenamiento</returns>
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
    }
}
