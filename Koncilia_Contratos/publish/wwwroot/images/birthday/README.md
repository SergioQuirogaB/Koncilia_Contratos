# 🎉 Carpeta para GIF de Cumpleaños

## 📍 Ubicación del GIF

Coloca tu archivo GIF de cumpleaños en esta carpeta:

```
Koncilia_Contratos/
└── wwwroot/
    └── images/
        └── birthday/
            └── [tu-archivo.gif]  ← Coloca tu GIF aquí
```

## 📝 Instrucciones

1. **Coloca tu GIF aquí**: Pon cualquier archivo `.gif` en esta carpeta
2. **Nombre del archivo**: Puede tener cualquier nombre, pero debe tener la extensión `.gif`
3. **Automático**: El sistema detectará automáticamente el GIF y lo incluirá en el correo de cumpleaños
4. **Múltiples GIFs**: Si hay varios GIFs, se usará el primero que encuentre

## ✅ Ejemplo

Si colocas un archivo llamado `feliz-cumpleanos.gif` en esta carpeta, el sistema:
- Lo detectará automáticamente
- Lo incluirá en el correo de cumpleaños
- Se enviará automáticamente cuando un empleado cumpla años

## 🎯 Nota Importante

- El GIF se enviará **automáticamente** sin necesidad de hacer clic en ninguna acción
- El servicio se ejecuta todos los días a la hora configurada en `appsettings.json` (por defecto 9:00 AM)
- El GIF aparecerá embebido en el correo HTML

