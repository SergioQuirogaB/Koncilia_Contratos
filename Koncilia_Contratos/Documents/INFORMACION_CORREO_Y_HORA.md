# 📧 Información sobre el Correo y la Hora de Envío

## 1. ¿Dónde está el mensaje del correo? ✉️

El mensaje del correo de cumpleaños está en el archivo:
**`Koncilia_Contratos/Services/EmailService.cs`**

📍 **Ubicación exacta:**
- **Archivo**: `Services/EmailService.cs`
- **Método**: `SendBirthdayEmailAsync` (línea 18)
- **Mensaje HTML**: Líneas 23-88 (variable `body`)
- **Asunto**: Línea 21

### Cómo modificarlo:

1. Abre el archivo: `Koncilia_Contratos/Services/EmailService.cs`
2. Busca el método `SendBirthdayEmailAsync` (línea 18)
3. El mensaje HTML está en la variable `body` (líneas 23-88)

Puedes modificar:
- El **asunto** del correo (línea 21)
- El **contenido HTML** del correo (líneas 62-86)
- Los **colores y estilos** del correo (líneas 28-60)

---

## 2. ¿Qué hora se usa para enviar el correo?

### La hora se configura en `appsettings.json`:

```json
"Birthday": {
  "CheckTime": "09:00"
}
```

**Esta es la hora que se usa.** Si no está configurada, usa el valor por defecto.

### Cómo funciona:

1. **Prioridad 1**: El sistema lee la hora desde `appsettings.json` en la sección `Birthday:CheckTime`
2. **Prioridad 2**: Si no existe en `appsettings.json`, usa el valor por defecto (8:00 AM) definido en `BirthdayBackgroundService.cs` línea 16

### Archivos relacionados:

- **`appsettings.json`** (línea 20-22): Aquí defines la hora que quieres usar
- **`Services/BirthdayBackgroundService.cs`** (líneas 29-43): Aquí se lee la configuración

### Ejemplo:

Si configuras `"CheckTime": "09:00"` en `appsettings.json`:
- El sistema enviará correos todos los días a las **9:00 AM**

Si lo cambias a `"CheckTime": "08:30"`:
- El sistema enviará correos todos los días a las **8:30 AM**

---

## 3. Resumen

| Item | Ubicación | Archivo |
|------|-----------|---------|
| **Mensaje del correo** | Método `SendBirthdayEmailAsync` | `Services/EmailService.cs` (líneas 18-91) |
| **Hora de envío** | Configuración `Birthday:CheckTime` | `appsettings.json` (línea 21) |
| **Valor por defecto de hora** | Constante `_scheduledTime` | `Services/BirthdayBackgroundService.cs` (línea 16) |

---

## 💡 Consejos

- **Para cambiar el mensaje**: Edita `EmailService.cs`
- **Para cambiar la hora**: Edita `appsettings.json` (más fácil)
- **Para cambiar el valor por defecto**: Edita `BirthdayBackgroundService.cs` (solo si quieres cambiar el fallback)

