using Microsoft.Extensions.Http.Resilience;
using Polly.Retry.Example.DelegatingHandler.Services;
using Polly.Retry.Example.DelegatingHandler.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Usar método de extensión personalizado
// Configurar HttpClient con políticas de resiliencia usando Microsoft.Extensions.Http.Resilience
// Esto reemplaza la necesidad de un DelegatingHandler personalizado
builder.Services.AddHttpClient<IBackendService, BackendService>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7056");
        client.Timeout = TimeSpan.FromSeconds(10);
    })
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

app.Run();
