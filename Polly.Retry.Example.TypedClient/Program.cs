using Microsoft.Extensions.Http.Resilience;
using Polly.Retry.Example.TypedClient.Services;
using Polly.Retry.Example.TypedClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Usar método de extensión personalizado
// Configurar HttpClient con inyección de tipo (Typed HttpClient) usando Microsoft.Extensions.Http.Resilience
// El HttpClient se inyecta directamente en el constructor del servicio
builder.Services.AddHttpClient<IBackendService, BackendService>()
    .AddCustomResilienceHandler();

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
