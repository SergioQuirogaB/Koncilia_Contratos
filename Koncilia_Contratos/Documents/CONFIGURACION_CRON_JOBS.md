# ⏰ Configuración de Cron Jobs para Correos de Cumpleaños Automáticos

## Problema

En Azure App Service, si "Always On" no está habilitado o el sitio está inactivo, los Background Services pueden detenerse y los correos automáticos no se enviarán.

## Solución: Endpoint HTTP para Cron Jobs

Se ha creado un endpoint HTTP que puede ser llamado por servicios de cron jobs externos (como Azure Functions, cron-job.org, EasyCron, etc.) para ejecutar la verificación de cumpleaños de manera confiable.

---

## 🔧 Configuración Inicial

### 1. Configurar la Clave de Seguridad

Edita `appsettings.json` y agrega/cambia la clave de seguridad:

```json
{
  "Birthday": {
    "CheckTime": "09:00",
    "TimeZoneId": "SA Pacific Standard Time",
    "SchedulerKey": "tu-clave-segura-aqui-12345"
  }
}
```

**⚠️ IMPORTANTE**: 
- Cambia `"tu-clave-segura-aqui-12345"` por una clave larga y segura (mínimo 20 caracteres)
- Esta clave se usará para autenticar las solicitudes al endpoint
- En producción, usa variables de entorno de Azure App Service en lugar de guardarla en appsettings.json

### 2. Configurar en Azure App Service (Producción)

En lugar de poner la clave en `appsettings.json`, configúrala como variable de entorno:

1. Ve a Azure Portal → Tu App Service → Configuration
2. En "Application settings", agrega:
   - **Nombre**: `Birthday:SchedulerKey`
   - **Valor**: Tu clave segura
3. Guarda los cambios

---

## 🌐 Endpoints Disponibles

### 1. Endpoint de Verificación de Cumpleaños

**URL**: `https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE`

**Métodos**: GET o POST

**Parámetros**:
- `key` (query string, requerido): La clave configurada en `Birthday:SchedulerKey`

**Respuesta exitosa**:
```json
{
  "success": true,
  "message": "Verificación completada. Enviados: 2, Errores: 0",
  "fecha": "2024-03-15",
  "empleadosEncontrados": 2,
  "enviadosExitosos": 2,
  "errores": 0,
  "resultados": [
    {
      "empleado": "Juan Pérez",
      "email": "juan@empresa.com",
      "estado": "enviado",
      "bccCount": 25
    }
  ]
}
```

### 2. Endpoint de Keep-Alive (Opcional)

**URL**: `https://tu-dominio.com/api/BirthdayScheduler/ping`

**Métodos**: GET o POST

**Propósito**: Mantener la aplicación activa. No requiere autenticación.

**Respuesta**:
```json
{
  "status": "ok",
  "timestamp": "2024-03-15T09:00:00Z",
  "message": "Servicio activo"
}
```

---

## 📅 Opciones de Servicios de Cron Jobs

### Opción 1: cron-job.org (Gratuito)

1. Regístrate en https://cron-job.org (gratis)
2. Crea un nuevo cron job:
   - **URL**: `https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE`
   - **Intervalo**: Diario
   - **Hora**: 9:00 AM (o la hora que prefieras)
   - **Zona horaria**: America/Bogota (o la que uses)
   - **Método**: GET

### Opción 2: EasyCron (Gratuito hasta 100 ejecuciones/mes)

1. Regístrate en https://www.easycron.com
2. Crea un nuevo cron job:
   - **URL**: `https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE`
   - **Schedule**: `0 9 * * *` (todos los días a las 9:00 AM)
   - **Método**: GET

### Opción 3: Azure Functions + Timer Trigger

Si tienes acceso a Azure Functions, puedes crear una función con Timer Trigger:

```csharp
[FunctionName("CheckBirthdays")]
public static async Task Run([TimerTrigger("0 0 9 * * *")] TimerInfo myTimer, ILogger log)
{
    using var httpClient = new HttpClient();
    var url = "https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE";
    var response = await httpClient.GetAsync(url);
    log.LogInformation($"Birthday check executed. Status: {response.StatusCode}");
}
```

**Cron expression**: `0 0 9 * * *` = Todos los días a las 9:00 AM UTC

### Opción 4: UptimeRobot (Monitoreo + Keep-Alive)

1. Regístrate en https://uptimerobot.com (gratis hasta 50 monitores)
2. Crea un monitor HTTP(S):
   - **URL**: `https://tu-dominio.com/api/BirthdayScheduler/ping`
   - **Intervalo**: Cada 5 minutos
   - Esto mantendrá tu aplicación activa

3. Crea otro monitor para el cron job:
   - **URL**: `https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE`
   - **Tipo**: Cron Job
   - **Horario**: Diario a las 9:00 AM

---

## ✅ Recomendación Final

**Para máxima confiabilidad, combina ambas soluciones:**

1. **Mantén el Background Service activo** (ya está configurado en `Program.cs`)
2. **Configura un cron job externo** como respaldo que llame al endpoint cada día

Esto asegura que:
- Si Always On está habilitado, el Background Service funcionará normalmente
- Si el Background Service falla, el cron job externo lo respaldará
- Si la aplicación se duerme, el cron job la despertará y ejecutará la verificación

---

## 🔍 Verificación y Monitoreo

### Ver logs en Azure App Service

1. Ve a Azure Portal → Tu App Service → Log stream
2. Busca mensajes como:
   - `"Verificación de cumpleaños iniciada mediante endpoint HTTP"`
   - `"Correo de cumpleaños enviado exitosamente a..."`

### Probar manualmente

Abre en tu navegador:
```
https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE
```

Deberías ver una respuesta JSON con el resultado de la verificación.

---

## ⚠️ Seguridad

- **NUNCA** compartas tu clave de seguridad
- **NUNCA** subas el `appsettings.json` con la clave real a repositorios públicos
- En producción, usa variables de entorno de Azure
- Considera usar HTTPS siempre para las llamadas al endpoint



