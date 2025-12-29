# 🔍 Diagnóstico: Problemas con Envío de Cumpleaños en Azure App Service

## Problema Común

Si los correos de cumpleaños no se están enviando a la hora configurada, puede deberse a varios factores relacionados con Azure App Service.

---

## 🌍 Información sobre East US 2

**Tu App Service está ubicado en: East US 2**

### Zonas Horarias

- **Zona horaria del servidor (East US 2)**: 
  - Eastern Standard Time (EST) / Eastern Daylight Time (EDT)
  - UTC-5 durante horario estándar (noviembre-marzo)
  - UTC-4 durante horario de verano (marzo-noviembre)

- **Zona horaria configurada en el sistema**:
  - SA Pacific Standard Time (Bogotá/Lima/Quito)
  - UTC-5 siempre (sin horario de verano)

### ¿Cómo funciona?

El código **siempre usa la zona horaria configurada** (`SA Pacific Standard Time`) para:
- Calcular cuándo enviar los correos
- Determinar qué fecha es "hoy" para buscar cumpleaños
- Convertir las horas a UTC para programar la ejecución

**Esto significa que**:
- Si configuras `"CheckTime": "09:00"`, los correos se enviarán a las **9:00 AM hora de Bogotá**, no a las 9:00 AM hora de East US 2
- Durante el horario de verano, cuando East US 2 está en UTC-4 y Bogotá en UTC-5, habrá una diferencia de 1 hora
- El sistema maneja esto automáticamente usando conversiones UTC

### Ejemplo Práctico

Si configuras `"CheckTime": "09:00"` (9:00 AM hora Bogotá):

**Durante horario estándar (noviembre-marzo)**:
- 9:00 AM Bogotá (UTC-5) = 2:00 PM UTC = 9:00 AM East US 2 (UTC-5)
- ✅ Misma hora en ambas zonas

**Durante horario de verano (marzo-noviembre)**:
- 9:00 AM Bogotá (UTC-5) = 2:00 PM UTC = 10:00 AM East US 2 (UTC-4)
- ⚠️ Diferencia de 1 hora (esto es normal y esperado)

**El sistema enviará los correos a las 9:00 AM hora Bogotá en ambos casos.**

---

## ✅ Checklist de Verificación

### 1. Verificar "Always On" en Azure App Service

**Este es el problema más común.**

1. Ve a **Azure Portal** → Tu **App Service** → **Configuration**
2. Busca la sección **"General settings"**
3. Verifica que **"Always On"** esté habilitado (debe estar en **"On"**)
4. Si está en **"Off"**, cámbialo a **"On"** y guarda los cambios

**⚠️ IMPORTANTE**: 
- Si "Always On" está deshabilitado, el App Service puede "dormirse" después de 20 minutos de inactividad
- Cuando el servicio está "dormido", los Background Services se detienen
- Esto significa que el servicio de cumpleaños no se ejecutará a la hora programada

**Nota**: "Always On" solo está disponible en planes de pago (no en el plan gratuito F1).

---

### 2. Verificar la Configuración en appsettings.json

Asegúrate de que la configuración esté correcta en `appsettings.json`:

```json
{
  "Birthday": {
    "CheckTime": "09:00",
    "TimeZoneId": "SA Pacific Standard Time"
  }
}
```

**Verifica**:
- `CheckTime` está en formato `"HH:mm"` (ejemplo: `"09:00"`, `"08:30"`)
- `TimeZoneId` es correcto para tu región

**Zonas horarias comunes**:
- Bogotá/Lima/Quito: `"SA Pacific Standard Time"`
- México: `"Central Standard Time"`
- España: `"W. Europe Standard Time"`

---

### 3. Verificar los Logs en Azure

Para ver si el servicio se está ejecutando:

1. Ve a **Azure Portal** → Tu **App Service** → **Log stream**
2. Busca mensajes como:
   - `"Servicio de verificación de cumpleaños iniciado."`
   - `"Próxima verificación de cumpleaños programada para: ..."`
   - `"Ejecutando verificación de cumpleaños. Hora actual - UTC: ..."`

**Si NO ves estos mensajes**, el servicio no se está ejecutando.

**Si ves los mensajes pero no se envían correos**, revisa:
- `"Se encontraron X empleado(s) que cumple(n) años hoy."`
- `"Correo de cumpleaños enviado exitosamente a ..."`
- `"Error al enviar correo de cumpleaños a ..."`

---

### 4. Verificar la Zona Horaria del Servidor

**⚠️ IMPORTANTE**: Tu App Service está en **East US 2**, que tiene una zona horaria diferente a la configurada.

- **Zona horaria del servidor (East US 2)**: Eastern Time (UTC-5 con DST, UTC-4 durante horario de verano)
- **Zona horaria configurada**: SA Pacific Standard Time (UTC-5 sin DST, siempre UTC-5)

**El código maneja esto correctamente**, pero es importante entender la diferencia:

1. Durante el horario de verano (marzo-noviembre):
   - East US 2: UTC-4
   - Bogotá (configurado): UTC-5
   - **Diferencia: 1 hora**

2. Durante el horario estándar (noviembre-marzo):
   - East US 2: UTC-5
   - Bogotá (configurado): UTC-5
   - **Diferencia: 0 horas**

**El sistema siempre usa la zona horaria configurada** (`SA Pacific Standard Time`) para calcular cuándo enviar los correos, independientemente de dónde esté el servidor.

**Verifica en los logs**:
- Busca: `"Zona horaria para cumpleaños: SA Pacific Standard Time (Offset actual: UTC-05:00) | Zona horaria del servidor: Eastern Standard Time (Offset: UTC-04:00)"`
- Si ves una advertencia sobre diferencia de zona horaria, es normal y esperado
- Verifica que la hora calculada sea correcta: `"Próxima verificación de cumpleaños programada para: ..."`

---

### 5. Probar Manualmente el Endpoint

Puedes probar manualmente si el sistema funciona llamando al endpoint:

```
https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE
```

**Reemplaza**:
- `tu-dominio.com` con tu dominio de Azure
- `TU_CLAVE` con la clave configurada en `Birthday:SchedulerKey`

**Si funciona manualmente pero no automáticamente**, el problema es con el Background Service (probablemente "Always On" deshabilitado).

---

## 🔧 Soluciones

### Solución 1: Habilitar "Always On" (Recomendado)

1. Ve a **Azure Portal** → Tu **App Service** → **Configuration**
2. En **"General settings"**, habilita **"Always On"**
3. Guarda los cambios
4. Reinicia el App Service si es necesario

**Nota**: Si estás en el plan gratuito F1, necesitarás actualizar a un plan de pago.

---

### Solución 2: Usar Cron Job Externo (Alternativa)

Si no puedes habilitar "Always On", usa un servicio de cron job externo:

1. Configura un cron job en un servicio externo (cron-job.org, EasyCron, etc.)
2. Configura el cron job para llamar al endpoint cada día a la hora deseada
3. El endpoint despertará la aplicación y ejecutará la verificación

**Ver documentación completa**: `CONFIGURACION_CRON_JOBS.md`

---

### Solución 3: Verificar Variables de Entorno en Azure

Si estás usando variables de entorno en Azure (recomendado para producción):

1. Ve a **Azure Portal** → Tu **App Service** → **Configuration** → **Application settings**
2. Verifica que existan:
   - `Birthday:CheckTime` = `"09:00"` (o la hora que quieras)
   - `Birthday:TimeZoneId` = `"SA Pacific Standard Time"`
3. Si no existen, agrégalas
4. Guarda y reinicia el App Service

---

## 📊 Interpretación de Logs

### Logs Normales (Todo funciona bien)

**Durante horario estándar (noviembre-marzo)**:
```
Servicio de verificación de cumpleaños iniciado.
Zona horaria para cumpleaños: SA Pacific Standard Time (Offset actual: UTC-05:00) | Zona horaria del servidor: Eastern Standard Time (Offset: UTC-05:00)
Hora de verificación de cumpleaños configurada: 09:00
Estado actual - UTC: 2024-01-15 14:00:00 | Hora servidor (East US 2): 2024-01-15 09:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-01-15 09:00:00 | Hora programada: 09:00:00
Próxima verificación de cumpleaños programada para: 2024-01-16 09:00:00 (hora SA Pacific Standard Time) | Hora servidor (East US 2): 2024-01-16 09:00:00 | UTC: 2024-01-16 14:00:00 | Tiempo restante: 24.00 horas (1440 minutos)
```

**Durante horario de verano (marzo-noviembre)**:
```
Servicio de verificación de cumpleaños iniciado.
Zona horaria para cumpleaños: SA Pacific Standard Time (Offset actual: UTC-05:00) | Zona horaria del servidor: Eastern Daylight Time (Offset: UTC-04:00)
⚠️ DIFERENCIA DE ZONA HORARIA DETECTADA: El servidor está en Eastern Daylight Time pero la configuración usa SA Pacific Standard Time. Esto es normal y el sistema calculará correctamente usando la zona horaria configurada (SA Pacific Standard Time).
Hora de verificación de cumpleaños configurada: 09:00
Estado actual - UTC: 2024-06-15 14:00:00 | Hora servidor (East US 2): 2024-06-15 10:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-06-15 09:00:00 | Hora programada: 09:00:00
Próxima verificación de cumpleaños programada para: 2024-06-16 09:00:00 (hora SA Pacific Standard Time) | Hora servidor (East US 2): 2024-06-16 10:00:00 | UTC: 2024-06-16 14:00:00 | Tiempo restante: 24.00 horas (1440 minutos)
```

**Nota**: La advertencia sobre diferencia de zona horaria es **normal y esperada** durante el horario de verano. El sistema funcionará correctamente.

### Logs cuando se Ejecuta

**Durante horario estándar**:
```
Ejecutando verificación de cumpleaños. Hora actual - UTC: 2024-01-16 14:00:00 | Hora servidor (East US 2): 2024-01-16 09:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-01-16 09:00:00
Verificando cumpleaños para la fecha: 2024-01-16 | UTC: 2024-01-16 14:00:00 | Hora servidor (East US 2): 2024-01-16 09:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-01-16 09:00:00
Se encontraron 2 empleado(s) que cumple(n) años hoy.
Correo de cumpleaños enviado exitosamente a Juan Pérez (juan@empresa.com) con copia a 25 empleado(s)
```

**Durante horario de verano**:
```
Ejecutando verificación de cumpleaños. Hora actual - UTC: 2024-06-16 14:00:00 | Hora servidor (East US 2): 2024-06-16 10:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-06-16 09:00:00
Verificando cumpleaños para la fecha: 2024-06-16 | UTC: 2024-06-16 14:00:00 | Hora servidor (East US 2): 2024-06-16 10:00:00 | Hora local configurada (SA Pacific Standard Time): 2024-06-16 09:00:00
Se encontraron 2 empleado(s) que cumple(n) años hoy.
Correo de cumpleaños enviado exitosamente a Juan Pérez (juan@empresa.com) con copia a 25 empleado(s)
```

**Nota**: Observa que durante el horario de verano, la hora del servidor (East US 2) muestra 10:00 AM, pero la hora local configurada (Bogotá) muestra 9:00 AM. Esto es correcto y el sistema enviará los correos a las 9:00 AM hora Bogotá.

### Problemas Comunes en los Logs

**Problema 1: No hay logs del servicio**
- **Causa**: El servicio no se está iniciando (probablemente "Always On" deshabilitado)
- **Solución**: Habilitar "Always On" o usar cron job externo

**Problema 2: Logs muestran hora incorrecta**
- **Causa**: Zona horaria mal configurada
- **Solución**: Verificar `Birthday:TimeZoneId` en appsettings.json o variables de entorno

**Problema 3: Logs muestran "No hay empleados que cumplan años hoy"**
- **Causa**: No hay empleados con cumpleaños en esa fecha, o la fecha está mal calculada
- **Solución**: Verificar que la fecha en los logs sea correcta

**Problema 4: Logs muestran errores al enviar correos**
- **Causa**: Problema con la configuración de email (SMTP)
- **Solución**: Verificar configuración de email en `appsettings.json`

---

## 🧪 Pruebas

### Prueba 1: Verificar que el Servicio se Inicia

1. Reinicia el App Service
2. Ve a **Log stream**
3. Busca: `"Servicio de verificación de cumpleaños iniciado."`
4. Si no aparece, el servicio no se está iniciando

### Prueba 2: Verificar el Cálculo de Hora

1. Ve a **Log stream**
2. Busca: `"Próxima verificación de cumpleaños programada para: ..."`
3. Verifica que la hora sea correcta según tu zona horaria
4. Verifica que el tiempo restante sea razonable (menos de 25 horas)

### Prueba 3: Probar Manualmente

1. Llama al endpoint: `https://tu-dominio.com/api/BirthdayScheduler/check?key=TU_CLAVE`
2. Verifica la respuesta JSON
3. Si funciona, el problema es con el Background Service
4. Si no funciona, revisa la configuración del endpoint

---

## 📝 Resumen

**Problema más común**: "Always On" deshabilitado en Azure App Service

**Solución más rápida**: Habilitar "Always On" en Azure Portal

**Alternativa**: Usar cron job externo que llame al endpoint HTTP

**Para diagnosticar**: Revisar los logs en Azure Portal → Log stream

---

## 🔗 Referencias

- `CONFIGURACION_CRON_JOBS.md` - Configuración de cron jobs externos
- `INFORMACION_CORREO_Y_HORA.md` - Información sobre configuración de hora
- `Services/BirthdayBackgroundService.cs` - Código del servicio

