# Comandos para Crear ZIP de Despliegue a Azure

## Pasos para Compilar y Empaquetar el Proyecto

### 1. Navegar al directorio del proyecto
```powershell
cd Koncilia_Contratos
```

### 2. Limpiar compilaciones anteriores (OPCIONAL pero recomendado)
```powershell
# Limpiar carpeta publish
Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue

# Limpiar carpetas bin y obj
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
```

### 3. Limpiar y compilar el proyecto
```powershell
dotnet clean
dotnet build -c Release --no-incremental
```

### 4. Publicar el proyecto
```powershell
dotnet publish -c Release -o publish
```

### 5. Volver al directorio raíz y crear el ZIP
```powershell
cd ..
powershell -Command "Compress-Archive -Path 'Koncilia_Contratos\publish\*' -DestinationPath 'Koncilia_Contratos_Azure_Deploy.zip' -Force"
```

## Comando Completo en una Línea (Script)

Si prefieres ejecutar todo de una vez, aquí está el script completo:

```powershell
cd Koncilia_Contratos
dotnet clean
dotnet build -c Release --no-incremental
dotnet publish -c Release -o publish
cd ..
powershell -Command "Compress-Archive -Path 'Koncilia_Contratos\publish\*' -DestinationPath 'Koncilia_Contratos_Azure_Deploy.zip' -Force"
```

## Verificar que el ZIP se creó correctamente

```powershell
Get-Item "Koncilia_Contratos_Azure_Deploy.zip" | Select-Object Name, @{Name="Size(MB)";Expression={[math]::Round($_.Length/1MB,2)}}, LastWriteTime | Format-List
```

## Notas Importantes

1. **Asegúrate de actualizar la versión en `_Layout.cshtml`** antes de compilar:
   - Archivo: `Koncilia_Contratos/Views/Shared/_Layout.cshtml`
   - Línea 211: Cambiar `<p class="text-sm text-gray-600">Versión X.X.X</p>`

2. **Si hay errores de archivos en uso**, espera unos segundos y vuelve a intentar:
   ```powershell
   Start-Sleep -Seconds 2
   powershell -Command "Compress-Archive -Path 'Koncilia_Contratos\publish\*' -DestinationPath 'Koncilia_Contratos_Azure_Deploy.zip' -Force"
   ```

3. **El ZIP se crea en el directorio raíz del proyecto** (donde está el `.sln`)

4. **Después de subir a Azure**, recuerda:
   - Reiniciar el App Service
   - Limpiar caché del navegador (Ctrl+Shift+Delete o Ctrl+F5)



