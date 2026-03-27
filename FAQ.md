# ❓ PREGUNTAS FRECUENTES (FAQ)

## Categorías
- [Inicio Rápido](#inicio-rápido)
- [Migración](#migración)
- [Errores](#errores)
- [Opciones](#opciones)
- [Técnico](#técnico)
- [Documentación](#documentación)

---

## Inicio Rápido

### P: "¿Por dónde empiezo?"
**R:** Abre `[Proyecto]/QUICKSTART.md` (5 minutos)
```
1. Copia el código
2. Pégalo en Program.cs
3. ¡Funciona automáticamente!
```

### P: "¿Necesito cambiar mi código de servicio?"
**R:** **No**, la resiliencia es completamente transparente.

Tu código de servicio funciona igual:
```csharp
public async Task<string> GetDataAsync()
{
    var response = await _httpClient.GetAsync("...");
    return await response.Content.ReadAsStringAsync();
}
```

### P: "¿Cuál es la configuración mínima?"
**R:** Una sola línea:
```csharp
builder.Services.AddHttpClient("Api")
    .AddCustomResilienceHandler();
```

---

## Migración

### P: "¿Qué cambió?"
**R:** Lee `[Proyecto]/MIGRATION.md` (10 minutos)

Resumen rápido:
- ✅ Polly.Extensions.Http → Microsoft.Extensions.Http.Resilience
- ✅ 26 líneas de código → 2 líneas
- ✅ 3 vulnerabilidades eliminadas

### P: "¿Mis políticas siguen siendo las mismas?"
**R:** Sí, pero ahora son automáticas:

| Aspecto | Antes | Después |
|---------|-------|---------|
| Retry | Manual | Automático (3 intentos) |
| Circuit Breaker | Manual | Automático (50% fallos) |
| Timeout | Manual | Automático (10 seg) |

### P: "¿Puedo usar múltiples clientes?"
**R:** Claro, de dos formas:

Opción 1 - Uno a uno:
```csharp
builder.Services.AddHttpClient("Api1").AddCustomResilienceHandler();
builder.Services.AddHttpClient("Api2").AddCustomResilienceHandler();
```

Opción 2 - Todos en uno (RECOMENDADO):
```csharp
builder.Services.AddHttpClientsWithResilience("Api1", "Api2", "Api3");
```

### P: "¿Necesito hacer algo especial con los handlers?"
**R:** No, están listos en `Extensions/HttpClientExtensions.cs`

Tienes 3 opciones:
- `AddCustomResilienceHandler()` - Estándar
- `AddAggressiveResilienceHandler()` - Mismo que anterior
- `AddConservativeResilienceHandler()` - Mismo que anterior

### P: "¿Qué pasó con Polly?"
**R:** Se retiró porque:
- ⚠️ 3 vulnerabilidades conocidas
- ❌ Código complejo y difícil de mantener
- ✅ Microsoft tiene su propia solución oficial

Puedes usar `EJEMPLOS_AVANZADOS.cs` para lógica personalizada.

---

## Errores

### P: "Me dice que AddCustomResilienceHandler() no existe"
**R:** Asegúrate de:
1. Importar `using Microsoft.Extensions.Http.Resilience;`
2. Tener el paquete instalado: `Microsoft.Extensions.Http.Resilience` v10.4.0+
3. Revisar que Extensions/HttpClientExtensions.cs existe

### P: "Tengo un error de 'HttpClientFactory not found'"
**R:** Agrega:
```csharp
builder.Services.AddHttpClient("Api");
builder.Services.AddScoped<IHttpClientFactory>(sp => 
    sp.GetRequiredService<IHttpClientFactory>());
```

### P: "No funciona en mi proyecto antiguo"
**R:** Verifica que uses .NET 8 (o actualiza):
```bash
dotnet --version
# Debe ser 8.0.x o superior
```

### P: "El servicio dice que HttpClient es null"
**R:** Asegúrate de inyectarlo:

**Opción 1 - Typed Client (RECOMENDADO):**
```csharp
public Service(HttpClient httpClient)
{
    _httpClient = httpClient;  // ← Se inyecta automáticamente
}
```

**Opción 2 - Factory:**
```csharp
public Service(IHttpClientFactory factory)
{
    var client = factory.CreateClient("Api");  // ← Nombre debe existir
}
```

---

## Opciones

### P: "¿Cuáles son las opciones de configuración?"
**R:** Revisa `GUIA_HANDLERS_GLOBALES.md`

Resumen:
1. **Opción 1** - Un cliente individual
2. **Opción 2** - Múltiples clientes (recomendado)
3. **Opción 3** - Typed HttpClient (mejor patrón)
4. **Opción 4** - Con DelegatingHandler personalizado
5. **Opción 5** - Desde configuración JSON

### P: "¿Puedo personalizar el timeout?"
**R:** Sí, hay dos formas:

Forma 1 - En el cliente:
```csharp
builder.Services.AddHttpClient("Api", client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);  // ← 20 seg en lugar de 10
});
```

Forma 2 - En appsettings.json:
```json
{
  "HttpClient": {
    "Timeout": 20
  }
}
```

### P: "¿Puedo agregar headers personalizados?"
**R:** Sí, con DelegatingHandler:
```csharp
builder.Services.AddHttpClient("Api")
    .AddHttpMessageHandler<CustomHeaderHandler>()
    .AddCustomResilienceHandler();
```

Ver `EJEMPLOS_AVANZADOS.cs` (Ejemplo 3) para implementación completa.

### P: "¿Puedo agregar logging?"
**R:** Sí, con DelegatingHandler:
```csharp
builder.Services.AddHttpClient("Api")
    .AddHttpMessageHandler<LoggingHandler>()
    .AddCustomResilienceHandler();
```

Ver `EJEMPLOS_AVANZADOS.cs` (Ejemplo 2) para implementación completa.

### P: "¿Puedo cambiar la política de reintentos?"
**R:** Para la configuración estándar no, pero puedes:

1. Usar DelegatingHandler personalizado
2. O crear políticas manuales (más complejo)

Recomendación: Mantén los valores por defecto (son optimizados por Microsoft).

---

## Técnico

### P: "¿Cómo funciona el Circuit Breaker?"
**R:** Se abre automáticamente si:
- 50% de las solicitudes fallan en 30 segundos
- Cuando se abre, rechaza nuevas solicitudes por 10 segundos
- Después, intenta recuperarse

### P: "¿Qué es el backoff exponencial?"
**R:** Los reintentos esperan cada vez más:
- Intento 1: Falla → Espera 1-2 segundos
- Intento 2: Falla → Espera 2-4 segundos
- Intento 3: Falla → Espera 4-8 segundos

Evita saturar un servidor que está caído.

### P: "¿El timeout de 10 segundos es suficiente?"
**R:** Depende de tu API:
- Si es rápida: Sí
- Si es lenta: Personalízalo en el cliente

```csharp
builder.Services.AddHttpClient("Api", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddCustomResilienceHandler();
```

### P: "¿Puedo ver qué está pasando (debugging)?"
**R:** Sí, con logging:
1. Ver `EJEMPLOS_AVANZADOS.cs` (Ejemplo 2)
2. O agregar un DelegatingHandler con logs
3. O usar Application Insights

### P: "¿Funciona con autenticación?"
**R:** Sí, de dos formas:

Forma 1 - En el cliente:
```csharp
builder.Services.AddHttpClient("Api", client =>
{
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
})
.AddCustomResilienceHandler();
```

Forma 2 - En DelegatingHandler personalizado:
```csharp
// Ver EJEMPLOS_AVANZADOS.cs (Ejemplo 4)
```

---

## Documentación

### P: "¿Cuánta documentación hay?"
**R:** 
- 25+ documentos
- 3000+ líneas
- 50+ ejemplos
- 5 opciones diferentes

### P: "¿Por dónde empiezo a leer?"
**R:** Según tu caso:

**Rápido (5 min):**
→ `[Proyecto]/QUICKSTART.md`

**Completo (30 min):**
→ README_COMPLETO.txt + QUICKSTART.md + README.md

**Exhaustivo (2 horas):**
→ Anterior + GUIA_HANDLERS_GLOBALES.md + EJEMPLOS_AVANZADOS.cs

### P: "¿Dónde están los ejemplos?"
**R:**
- `[Proyecto]/QUICKSTART.md` - Básicos
- `[Proyecto]/README.md` - Intermedios
- `EJEMPLOS_AVANZADOS.cs` - Avanzados (5 ejemplos)

### P: "¿Dónde busco algo específico?"
**R:** 
→ `INDICE_DOCUMENTACION.md` (búsqueda rápida)

---

## Preguntas de Principiante

### P: "No entiendo qué es HttpClient"
**R:** Es un componente que hace solicitudes HTTP (GET, POST, etc.) a servidores.

**Ejemplo:**
```csharp
// Hace una solicitud GET a un servidor
var response = await httpClient.GetAsync("https://api.example.com/data");
```

### P: "¿Qué es la resiliencia?"
**R:** La capacidad de un sistema de recuperarse de errores automáticamente.

Con resiliencia:
- Si falla → Reintenta automáticamente
- Si sigue fallando → Circuit breaker (protege el sistema)
- Si es muy lento → Timeout (cancela después de X segundos)

### P: "¿Qué es Polly?"
**R:** Era una librería para hacer código resiliente. Ahora Microsoft tiene su propia solución (Microsoft.Extensions.Http.Resilience) que es mejor y oficial.

### P: "¿Necesito aprender sobre Polly?"
**R:** **No**, está completamente retirado. Todo es automático ahora.

---

## Preguntas Avanzadas

### P: "¿Cómo hago métricas/telemetría?"
**R:** Ver `EJEMPLOS_AVANZADOS.cs` (Ejemplo 5)

Usa un DelegatingHandler para capturar tiempos y contar errores.

### P: "¿Puedo combinar múltiples handlers?"
**R:** Sí:
```csharp
builder.Services.AddHttpClient("Api")
    .AddHttpMessageHandler<LoggingHandler>()
    .AddHttpMessageHandler<AuthHandler>()
    .AddHttpMessageHandler<MetricsHandler>()
    .AddCustomResilienceHandler();
```

Se ejecutan en orden: Logging → Auth → Metrics → Resiliencia

### P: "¿Qué pasó con los DelegatingHandlers personalizados?"
**R:** Siguen siendo útiles para:
- Logging
- Headers personalizados
- Autenticación
- Métricas

Ver `EJEMPLOS_AVANZADOS.cs` para 5 ejemplos prácticos.

### P: "¿Puedo usar async/await con reintentos?"
**R:** Sí, todo es async automáticamente:
```csharp
var response = await httpClient.GetAsync("...");  // ← Async
// Los reintentos son automáticos
```

---

## Troubleshooting

### Problema: "Tengo muchos timeouts"
**Solución:**
1. Aumenta el timeout: `client.Timeout = TimeSpan.FromSeconds(30);`
2. O verifica que el servidor responde rápido

### Problema: "El Circuit Breaker se abre constantemente"
**Solución:**
1. Verifica que el servidor está disponible
2. O revisa la configuración de red
3. O aumenta el timeout

### Problema: "No sé si funciona"
**Solución:**
1. Agrega logging (ver `EJEMPLOS_AVANZADOS.cs`)
2. O pon breakpoints en el código
3. O usa Application Insights

### Problema: "Necesito más control"
**Solución:**
1. Crea DelegatingHandler personalizado
2. O revisa `GUIA_HANDLERS_GLOBALES.md` (Opción 4 y 5)

---

## ¿No encontraste la respuesta?

**Opciones:**
1. Busca en `INDICE_DOCUMENTACION.md`
2. Revisa los ejemplos en `EJEMPLOS_AVANZADOS.cs`
3. Lee la documentación del proyecto: `[Proyecto]/README.md`
4. O abre `[Proyecto]/MIGRATION.md` (sección Troubleshooting)

---

**Última actualización:** 2024  
**Versión .NET:** 8.0  
**Estado:** ✅ Completado
