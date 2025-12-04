# Script para crear ZIP de despliegue a Azure
# Ejecutar desde el directorio raíz del proyecto (donde está el .sln)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Creando ZIP de Despliegue a Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "Koncilia_Contratos\Koncilia_Contratos.csproj")) {
    Write-Host "ERROR: No se encontró el proyecto. Ejecuta este script desde el directorio raíz del proyecto." -ForegroundColor Red
    exit 1
}

# Paso 1: Limpiar compilaciones anteriores
Write-Host "[1/5] Limpiando compilaciones anteriores..." -ForegroundColor Yellow
Remove-Item -Path "Koncilia_Contratos\publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Koncilia_Contratos\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Koncilia_Contratos\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  ✓ Limpieza completada" -ForegroundColor Green

# Paso 2: Limpiar proyecto
Write-Host "[2/5] Limpiando proyecto..." -ForegroundColor Yellow
Set-Location "Koncilia_Contratos"
dotnet clean | Out-Null
Write-Host "  ✓ Proyecto limpiado" -ForegroundColor Green

# Paso 3: Compilar proyecto
Write-Host "[3/5] Compilando proyecto en modo Release..." -ForegroundColor Yellow
$buildResult = dotnet build -c Release --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Error en la compilación" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Write-Host "  ✓ Compilación exitosa" -ForegroundColor Green

# Paso 4: Publicar proyecto
Write-Host "[4/5] Publicando proyecto..." -ForegroundColor Yellow
$publishResult = dotnet publish -c Release -o publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ✗ Error en la publicación" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Write-Host "  ✓ Publicación completada" -ForegroundColor Green

# Volver al directorio raíz
Set-Location ..

# Paso 5: Crear ZIP
Write-Host "[5/5] Creando archivo ZIP..." -ForegroundColor Yellow

# Eliminar ZIP anterior si existe
if (Test-Path "Koncilia_Contratos_Azure_Deploy.zip") {
    Remove-Item "Koncilia_Contratos_Azure_Deploy.zip" -Force
}

# Intentar crear el ZIP
$maxRetries = 3
$retryCount = 0
$zipCreated = $false

while ($retryCount -lt $maxRetries -and -not $zipCreated) {
    try {
        Compress-Archive -Path "Koncilia_Contratos\publish\*" -DestinationPath "Koncilia_Contratos_Azure_Deploy.zip" -Force -ErrorAction Stop
        $zipCreated = $true
        Write-Host "  ✓ ZIP creado exitosamente" -ForegroundColor Green
    }
    catch {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Host "  ⚠ Intento $retryCount/$maxRetries falló. Esperando 2 segundos..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
        else {
            Write-Host "  ✗ Error al crear el ZIP después de $maxRetries intentos" -ForegroundColor Red
            Write-Host "  Error: $_" -ForegroundColor Red
            exit 1
        }
    }
}

# Mostrar información del ZIP creado
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ✓ Proceso completado exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$zipInfo = Get-Item "Koncilia_Contratos_Azure_Deploy.zip"
$sizeMB = [math]::Round($zipInfo.Length / 1MB, 2)

Write-Host "Archivo creado: $($zipInfo.Name)" -ForegroundColor White
Write-Host "Tamaño: $sizeMB MB" -ForegroundColor White
Write-Host "Fecha: $($zipInfo.LastWriteTime)" -ForegroundColor White
Write-Host ""
Write-Host "Ubicación: $($zipInfo.FullName)" -ForegroundColor Gray
Write-Host ""
Write-Host "¡Listo para subir a Azure App Service!" -ForegroundColor Green

