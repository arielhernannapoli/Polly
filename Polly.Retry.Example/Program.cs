using Microsoft.Extensions.Http.Resilience;
using Polly.Retry.Example.Services;
using Polly.Retry.Example.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ OPCIÓN 1: Usar método de extensión personalizado (recomendado)
builder.Services.AddHttpClient("BackendApi")
    .AddCustomResilienceHandler();

// ✅ O usar configuración agresiva
// builder.Services.AddHttpClient("BackendApi")
//     .AddAggressiveResilienceHandler();

// O de forma individual si prefieres:
// builder.Services.AddHttpClient("BackendApi")
//     .AddCustomResilienceHandler();

// Registrar servicios
builder.Services.AddScoped<IBackendService, BackendService>();

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

await app.RunAsync();
