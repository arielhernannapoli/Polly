using Microsoft.Extensions.Http.Resilience;

namespace Polly.Retry.Example.TypedClient.Extensions
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
        /// Registra múltiples Typed HttpClients con resiliencia estándar
        /// </summary>
        public static IServiceCollection AddTypedHttpClientsWithResilience<T1, T2>(
            this IServiceCollection services)
            where T1 : class
            where T2 : class
        {
            services.AddHttpClient<T1>().AddCustomResilienceHandler();
            services.AddHttpClient<T2>().AddCustomResilienceHandler();
            return services;
        }
    }
}
