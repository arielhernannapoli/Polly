# 🚀 Guía de Ejecución - Ambas APIs con Polly

Ahora tienes **dos proyectos completamente funcionales** que demuestran diferentes formas de usar `HttpClientFactory` con **Polly .NET**.

---

## 📋 Resumen

| Característica | Proyecto 1 | Proyecto 2 |
|----------------|-----------|-----------|
| **Nombre** | Named HttpClient | Typed HttpClient |
| **Carpeta** | `Polly.Retry.Example/` | `Polly.Retry.Example.TypedClient/` |
| **Puerto HTTPS** | 7147 | 7056 |
| **Patrón** | `IHttpClientFactory` | Inyección directa |
| **Complejidad** | Media | Baja |
| **Flexibilidad** | Alta | Media |

---

## 🎯 Opción 1: Ejecutar ambos simultáneamente

### Terminal 1 - Proyecto 1 (Named HttpClient)
```bash
cd C:\Users\ariel\Source\Repos\Polly.Retry.Example\Polly.Retry.Example
dotnet run
```

### Terminal 2 - Proyecto 2 (Typed HttpClient)
```bash
cd C:\Users\ariel\Source\Repos\Polly.Retry.Example\Polly.Retry.Example.TypedClient
dotnet run
```

Verás en cada terminal:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7147
```

---

## 🧪 Probar Proyecto 1 (Named HttpClient - Puerto 7147)

### PowerShell
```powershell
# Llamar Consumer (con reintentos)
$uri = "https://localhost:7147/api/consumer/data"
Invoke-WebRequest -Uri $uri -SkipCertificateCheck

# O ver el resultado en JSON
(Invoke-WebRequest -Uri $uri -SkipCertificateCheck).Content | ConvertFrom-Json | ConvertTo-Json

# Resetear
Invoke-WebRequest -Uri "https://localhost:7147/api/backend/reset" -Method Post -SkipCertificateCheck
```

### cURL
```bash
# Llamar Consumer
curl -X GET "https://localhost:7147/api/consumer/data" -k

# Resetear
curl -X POST "https://localhost:7147/api/backend/reset" -k
```

---

## 🧪 Probar Proyecto 2 (Typed HttpClient - Puerto 7056)

### PowerShell
```powershell
# Llamar Consumer (con reintentos)
$uri = "https://localhost:7056/api/consumer/data"
Invoke-WebRequest -Uri $uri -SkipCertificateCheck

# O ver el resultado en JSON
(Invoke-WebRequest -Uri $uri -SkipCertificateCheck).Content | ConvertFrom-Json | ConvertTo-Json

# Resetear
Invoke-WebRequest -Uri "https://localhost:7056/api/backend/reset" -Method Post -SkipCertificateCheck
```

### cURL
```bash
# Llamar Consumer
curl -X GET "https://localhost:7056/api/consumer/data" -k

# Resetear
curl -X POST "https://localhost:7056/api/backend/reset" -k
```

---

## 📊 Respuesta esperada

Verás algo como esto:

```json
{
  "success": true,
  "data": {
    "message": "Datos del backend",
    "timestamp": "2026-03-20T18:59:45.1234567Z",
    "callCount": 3
  },
  "message": "Datos obtenidos del backend con reintentos de Polly",
  "implementation": "Named HttpClient"
}
```

---

## 🔍 Ver los reintentos en consola

En la Terminal de ejecución verás:

```
[Named HttpClient]
Reintentando... Intento 1 después de 2s
Reintentando... Intento 2 después de 4s
Backend: Respondiendo correctamente en llamada 3

[Typed HttpClient]
[TypedClient] Reintentando... Intento 1 después de 2s
[TypedClient] Reintentando... Intento 2 después de 4s
[TypedClient] Backend: Respondiendo correctamente en llamada 3
```

---

## 🔄 Ver diferencias en la consola

### Proyecto 1 (Named HttpClient)
```
info: Polly.Retry.Example.Services.BackendService[0]
      Llamando a Backend API...
Reintentando... Intento 1 después de 2s
Reintentando... Intento 2 después de 4s
info: Polly.Retry.Example.Services.BackendService[0]
      Backend API respondió correctamente
```

### Proyecto 2 (Typed HttpClient)
```
info: Polly.Retry.Example.TypedClient.Services.BackendService[0]
      [TypedClient] Llamando a Backend API...
[TypedClient] Reintentando... Intento 1 después de 2s
[TypedClient] Reintentando... Intento 2 después de 4s
info: Polly.Retry.Example.TypedClient.Services.BackendService[0]
      [TypedClient] Backend API respondió correctamente
```

---

## 🎯 Endpoints en ambos proyectos

### Consumer (con reintentos)
```
GET /api/consumer/data
```

### Backend (simula fallos)
```
GET /api/backend/data
```

### Reset (resetea contador)
```
POST /api/backend/reset
```

---

## 📝 Script de prueba completo

### PowerShell (prueba ambas)
```powershell
Write-Host "=== PROYECTO 1: Named HttpClient (Puerto 7147) ===" -ForegroundColor Cyan
$result1 = Invoke-WebRequest -Uri "https://localhost:7147/api/consumer/data" -SkipCertificateCheck
$json1 = $result1.Content | ConvertFrom-Json
Write-Host "Implementation: $($json1.implementation)" -ForegroundColor Green
Write-Host "Success: $($json1.success)" -ForegroundColor Green
Write-Host ""

Write-Host "=== PROYECTO 2: Typed HttpClient (Puerto 7056) ===" -ForegroundColor Cyan
$result2 = Invoke-WebRequest -Uri "https://localhost:7056/api/consumer/data" -SkipCertificateCheck
$json2 = $result2.Content | ConvertFrom-Json
Write-Host "Implementation: $($json2.implementation)" -ForegroundColor Green
Write-Host "Success: $($json2.success)" -ForegroundColor Green
```

---

## 🐛 Solución de problemas

### Certificado SSL rechazado
Usa `-SkipCertificateCheck` o `-k` en curl

### Puerto en uso
```powershell
# Encontrar proceso en puerto 7147
netstat -ano | findstr :7147

# O cambiar puerto en launchSettings.json
```

### DLL no encontrada
```bash
dotnet restore
dotnet build
```

---

## 📚 Estructura de archivos

```
Polly.Retry.Example\
├── Polly.Retry.Example\              (Proyecto 1: Named HttpClient)
│   ├── Services\BackendService.cs    (Usa IHttpClientFactory)
│   └── Program.cs                    (AddHttpClient("BackendApi"))
│
├── Polly.Retry.Example.TypedClient\  (Proyecto 2: Typed HttpClient)
│   ├── Services\BackendService.cs    (Inyecta HttpClient directo)
│   └── Program.cs                    (AddHttpClient<IBackendService, BackendService>())
│
└── Polly.Retry.Example.sln           (Solución con ambos proyectos)
```

---

## ✅ Verificación rápida

Si ves estos logs, ¡todo está funcionando! ✨

```
✅ Polly reintentos activos
✅ Circuit Breaker configurado  
✅ Ambos proyectos ejecutándose
✅ Logs con reintentos visibles
```

---

## 💡 Próximos pasos

1. **Comparar**: Abre ambas terminales lado a lado
2. **Analizar**: Lee los logs de reintentos
3. **Experimentar**: Modifica parámetros de Polly
4. **Elegir**: Decide cuál patrón es mejor para tu caso

---

## 🎓 Resumen de aprendizaje

Ahora comprendes:
- ✅ `HttpClientFactory` con clientes nombrados
- ✅ Typed `HttpClient` para inyección de tipo
- ✅ Políticas de Polly (Retry + Circuit Breaker)
- ✅ Cómo gestionar reintentos exponenciales
- ✅ Diferencias entre ambos patrones

¡Felicidades! 🚀
