# Cómo Verificar que la Tabla Empleados Existe

## Problema: No veo la tabla Empleados en SQL Server Management Studio

Si no ves la tabla `Empleados` en tu explorador de SQL Server Management Studio, sigue estos pasos:

### 1. Refrescar la Vista en SQL Server Management Studio

1. Haz clic derecho en la carpeta **"Tables"** 
2. Selecciona **"Refresh"** (Actualizar)
3. La tabla `dbo.Empleados` debería aparecer

### 2. Verificar que la Migración se Aplicó

La migración `AddEmpleadoModel` debería haber creado la tabla. Verifica:

```sql
-- Ejecuta este comando en SQL Server Management Studio
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Empleados'
```

Si la tabla existe, verás un resultado. Si no existe, ejecuta la migración:

```powershell
cd Koncilia_Contratos
dotnet ef database update
```

### 3. Ver la Estructura de la Tabla

Una vez que veas la tabla, puedes ver su estructura:

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Empleados'
ORDER BY ORDINAL_POSITION
```

### 4. La Tabla Debería Tener Estas Columnas:

- `Id` (int, Identity)
- `Nombre` (nvarchar(100))
- `Apellido` (nvarchar(100))
- `FechaCumpleanos` (datetime2)
- `CorreoElectronico` (nvarchar(200))

### 5. Si la Tabla No Existe

Si después de refrescar y verificar, la tabla no existe, ejecuta:

```powershell
cd C:\Users\SergioAlejandroQuiro\Documents\GitHub\Koncilia_Contratos\Koncilia_Contratos
dotnet ef database update
```

Esto aplicará la migración y creará la tabla.

### 6. Verificar en el Explorador de Objetos

En SQL Server Management Studio:
- Expande `KONTRATOSQA`
- Expande `Tables`
- Busca `dbo.Empleados`
- Si no la ves, haz clic derecho en "Tables" → "Refresh"

### Nota Importante

La tabla se guarda en la base de datos `KONTRATOSQA` en el servidor Azure SQL Server.

