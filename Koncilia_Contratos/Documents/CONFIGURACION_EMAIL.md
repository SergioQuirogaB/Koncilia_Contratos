# Configuración de Email para Cumpleaños

Para que el módulo de Cumpleaños pueda enviar correos electrónicos automáticamente, necesitas configurar las credenciales de email en el archivo `appsettings.json`.

## Configuración en appsettings.json

Agrega o actualiza la sección `Email` en tu archivo `appsettings.json`:

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

## Configuración para Gmail

Si usas Gmail, necesitas crear una "Contraseña de aplicación" en lugar de usar tu contraseña regular:

1. Ve a tu cuenta de Google: https://myaccount.google.com/
2. Activa la verificación en dos pasos si no la tienes activada
3. Ve a "Seguridad" > "Contraseñas de aplicaciones"
4. Genera una nueva contraseña de aplicación para "Correo"
5. Usa esa contraseña en el campo `SmtpPassword`

## Otros proveedores de email

### Office365/Outlook (Configuración actual)
```json
{
  "SmtpServer": "smtp.office365.com",
  "SmtpPort": "587",
  "SmtpUsername": "recursoshumanos@koncilia.com.co",
  "SmtpPassword": "tu-contraseña",
  "FromEmail": "recursoshumanos@koncilia.com.co",
  "FromName": "Koncilia Recursos Humanos"
}
```

**Nota:** Esta es la configuración actual del sistema. Usa Office365 para el envío de correos.

### Otros servidores SMTP
Ajusta los valores según la configuración de tu proveedor de correo.

## Importante

- **NO** subas el archivo `appsettings.json` con las credenciales a un repositorio público
- Considera usar variables de entorno o Azure Key Vault en producción
- El servicio en segundo plano verifica los cumpleaños cada 24 horas

