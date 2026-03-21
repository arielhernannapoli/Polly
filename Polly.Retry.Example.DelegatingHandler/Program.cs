using Polly;
using Polly.Retry;
using Polly.Retry.Example.DelegatingHandler.Handlers;
using Polly.Retry.Example.DelegatingHandler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Crear política de reintento con backoff exponencial para HttpResponseMessage
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
    .WaitAndRetryAsync<HttpResponseMessage>(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Reintentando... Intento {retryCount} después de {timespan.TotalSeconds}s");
            if (outcome.Exception != null)
            {
                Console.WriteLine($"  Excepción: {outcome.Exception.Message}");
            }
            else if (outcome.Result != null)
            {
                Console.WriteLine($"  StatusCode: {outcome.Result.StatusCode}");
            }
        }
    );

// Crear política de circuit breaker para HttpResponseMessage
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500)
    .CircuitBreakerAsync<HttpResponseMessage>(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(10),
        onBreak: (outcome, timespan) =>
        {
            Console.WriteLine($"Circuit breaker abierto por {timespan.TotalSeconds}s");
        },
        onReset: () =>
        {
            Console.WriteLine("Circuit breaker reseteado");
        }
    );

// Combinar políticas
var combinedPolicy = Policy.WrapAsync<HttpResponseMessage>(retryPolicy, circuitBreakerPolicy);

// Registrar el DelegatingHandler con la política combinada
builder.Services.AddScoped<PollyDelegatingHandler>(sp =>
    new PollyDelegatingHandler(combinedPolicy)
);

// Configurar HttpClient con el DelegatingHandler
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddHttpMessageHandler<PollyDelegatingHandler>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7056");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
