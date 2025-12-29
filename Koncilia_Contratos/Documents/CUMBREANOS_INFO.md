# 📧 Información sobre el Sistema de Cumpleaños

## ¿Cómo funciona el envío automático de correos?

El sistema envía correos automáticamente usando **el correo electrónico que guardas en cada empleado**. Aquí te explico cómo funciona:

### 🔄 Flujo del Sistema

1. **Cuando creas un empleado:**
   - Ingresas: Nombre, Apellido, Fecha de Cumpleaños y **Correo Electrónico** (obligatorio)
   - El correo se guarda en la base de datos junto con los demás datos

2. **Servicio en segundo plano (Background Service):**
   - El sistema revisa **todos los días** a las 00:00 horas
   - Busca empleados cuya fecha de cumpleaños coincida con la fecha actual
   - Para cada empleado que cumple años, toma su correo electrónico guardado
   - Envía automáticamente un correo de felicitación a ese correo

3. **Ejemplo práctico:**
   ```
   Empleado: Juan Pérez
   Correo: juan.perez@empresa.com
   Fecha Cumpleaños: 15 de Marzo
   
   → El 15 de Marzo a las 00:00, el sistema:
   1. Detecta que Juan cumple años hoy
   2. Busca su correo: juan.perez@empresa.com
   3. Envía el correo de felicitación a esa dirección
   ```

### 📝 Campos del Empleado

- **Nombre**: Obligatorio
- **Apellido**: Obligatorio  
- **Fecha de Cumpleaños**: Obligatorio (el sistema usa día y mes para verificar)
- **Correo Electrónico**: **OBLIGATORIO** - Este es el correo al que se enviará el mensaje automáticamente

### ✅ Validaciones

- El correo debe tener un formato válido (ejemplo@email.com)
- El correo es obligatorio (no puedes guardar un empleado sin correo)
- El sistema valida automáticamente el formato antes de guardar

### 🔧 Configuración Necesaria

Para que los correos se envíen, debes configurar las credenciales SMTP en `appsettings.json`:

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "tu-email@gmail.com",
    "SmtpPassword": "tu-contraseña-de-aplicacion",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "Koncilia Contratos"
  }
}
```

Ver el archivo `CONFIGURACION_EMAIL.md` para más detalles.

### 💡 Características Adicionales

- **Envío manual**: Puedes enviar un correo manualmente desde la lista de empleados si alguien cumple años hoy
- **Búsqueda por correo**: Puedes buscar empleados por su correo electrónico en la lista
- **Vista detallada**: Puedes ver y editar el correo de cada empleado desde la vista de detalles

### ❓ Preguntas Frecuentes

**P: ¿Puedo cambiar el correo de un empleado después de crearlo?**  
R: Sí, puedes editarlo desde la opción "Editar" en cualquier momento.

**P: ¿El sistema envía correos a múltiples empleados el mismo día?**  
R: Sí, si varios empleados cumplen años el mismo día, el sistema enviará correos a todos automáticamente.

**P: ¿Qué pasa si un empleado no tiene correo?**  
R: No puedes crear un empleado sin correo, es un campo obligatorio. Si intentas guardar sin correo, el sistema te mostrará un error.

**P: ¿Cuándo se envía el correo exactamente?**  
R: El servicio verifica cada 24 horas. Si alguien cumple años, el correo se envía en la próxima verificación después de las 00:00.

