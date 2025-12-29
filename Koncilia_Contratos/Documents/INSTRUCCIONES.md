# Sistema de Gestión de Contratos - Koncilia

## ✅ Funcionalidades Implementadas

El sistema ahora cuenta con todas las funcionalidades solicitadas:

### 1. **Gestión Completa de Contratos (CRUD)**
- ✅ Crear nuevos contratos
- ✅ Editar contratos existentes
- ✅ Eliminar contratos
- ✅ Ver detalles completos de contratos
- ✅ Listar contratos con filtros avanzados (por estado, año, búsqueda por texto)

### 2. **Campos del Contrato**
Todos los campos solicitados han sido implementados:
- ✅ Año
- ✅ Empresa
- ✅ Cliente
- ✅ Número de contrato
- ✅ Valor en pesos (sin IVA)
- ✅ Valor en dólares
- ✅ Descripción
- ✅ Categoría
- ✅ Valor mensual
- ✅ Observaciones
- ✅ Fecha de inicio
- ✅ Fecha de vencimiento
- ✅ Valor facturado
- ✅ % de ejecución según facturación (calculado automáticamente)
- ✅ Valor pendiente por ejecutar (calculado automáticamente)
- ✅ Estado
- ✅ Número de horas
- ✅ Número de factura
- ✅ Número de póliza
- ✅ Fecha de vencimiento de la póliza

### 3. **Importación y Exportación**
- ✅ Importar contratos desde Excel (.xlsx)
- ✅ Exportar contratos a Excel con todos los datos
- ✅ Plantilla con instrucciones para importación

### 4. **Sistema de Reportes**
- ✅ Dashboard de reportes con gráficas interactivas
- ✅ Reporte de gestión de clientes con análisis detallado
- ✅ Reporte ejecutivo con KPIs principales
- ✅ Gráficas de:
  - Contratos por estado
  - Contratos por categoría
  - Evolución anual
  - Top 10 empresas por valor
  - Top 10 clientes por valor
  - Distribución financiera

### 5. **Configuración del Sistema**
- ✅ Módulo de configuración por categorías
- ✅ Gestión de parámetros del sistema
- ✅ CRUD completo para configuraciones

### 6. **Características Adicionales**
- ✅ Cálculo automático de % de ejecución
- ✅ Cálculo automático de valor pendiente
- ✅ Alertas de contratos por vencer (30 días)
- ✅ Identificación de contratos vencidos
- ✅ Indicadores visuales de estado
- ✅ Barras de progreso para ejecución
- ✅ Búsqueda y filtros avanzados

## 🚀 Instrucciones de Configuración

### Paso 1: Detener la Aplicación
**IMPORTANTE**: Debes detener la aplicación que está corriendo actualmente para poder aplicar las migraciones.

### Paso 2: Restaurar Paquetes NuGet
Abre PowerShell en la carpeta del proyecto y ejecuta:
```powershell
cd Koncilia_Contratos
dotnet restore
```

### Paso 3: Aplicar Migraciones a la Base de Datos
Ejecuta los siguientes comandos para crear las migraciones y actualizar la base de datos:

```powershell
# Crear la migración para los nuevos modelos
dotnet ef migrations add AddContratoYConfiguracionModels

# Aplicar la migración a la base de datos
dotnet ef database update
```

### Paso 4: Ejecutar la Aplicación
```powershell
dotnet run
```

O simplemente presiona F5 en Visual Studio.

## 📋 Cómo Usar el Sistema

### Acceder al Sistema
1. Ejecuta la aplicación
2. Inicia sesión con tu usuario
3. Serás redirigido al Dashboard

### Gestionar Contratos

#### Crear un Contrato
1. Ve a **Contratos** en el menú principal
2. Haz clic en **"Nuevo Contrato"**
3. Llena el formulario con todos los datos
4. El sistema calculará automáticamente:
   - % de ejecución (basado en valor facturado / valor total)
   - Valor pendiente (valor total - valor facturado)
5. Haz clic en **"Guardar Contrato"**

#### Editar un Contrato
1. En la lista de contratos, haz clic en el ícono de lápiz (✏️)
2. Modifica los campos necesarios
3. Haz clic en **"Actualizar Contrato"**

#### Eliminar un Contrato
1. En la lista de contratos, haz clic en el ícono de basura (🗑️)
2. Confirma la eliminación

### Importar Contratos desde Excel

1. Ve a **Contratos > Importar**
2. Prepara tu archivo Excel con las siguientes columnas en orden:
   1. Año
   2. Empresa
   3. Cliente
   4. Número de Contrato
   5. Valor en Pesos (sin IVA)
   6. Valor en Dólares
   7. Descripción
   8. Categoría
   9. Valor Mensual
   10. Observaciones
   11. Fecha de Inicio (formato fecha)
   12. Fecha de Vencimiento (formato fecha)
   13. Valor Facturado
   14. Estado
   15. Número de Horas
   16. Número de Factura
   17. Número de Póliza
   18. Fecha de Vencimiento de la Póliza (formato fecha)

3. Selecciona tu archivo y haz clic en **"Importar Contratos"**

### Exportar Contratos
1. Ve a **Contratos**
2. Haz clic en **"Exportar"**
3. Se descargará un archivo Excel con todos los contratos

### Ver Reportes

#### Reporte de Análisis General
1. Ve a **Reportes** en el menú
2. Verás:
   - Tarjetas con estadísticas clave
   - Gráficas interactivas
   - Análisis por categoría, año, empresa y cliente

#### Reporte de Gestión de Clientes
1. Ve a **Reportes > Reporte de Clientes**
2. Verás una tabla detallada con:
   - Total de contratos por cliente
   - Contratos activos
   - Valor total
   - Valor facturado
   - Valor pendiente
   - % de ejecución promedio

#### Reporte Ejecutivo
1. Ve a **Reportes > Reporte Ejecutivo**
2. Verás:
   - KPIs principales
   - Alertas de contratos por vencer o vencidos
   - Distribución por estado
   - Evolución anual
3. Puedes imprimir el reporte usando el botón **"Imprimir Reporte"**

### Configurar el Sistema
1. Ve a **Configuración** en el menú
2. Aquí puedes:
   - Agregar nuevas configuraciones
   - Editar configuraciones existentes
   - Organizar por categorías

## 📊 Características de las Gráficas

Las gráficas utilizan Chart.js e incluyen:
- **Interactividad**: Pasa el mouse sobre los elementos para ver detalles
- **Gráficas de pastel**: Para distribución por estado y categoría
- **Gráficas de barras**: Para evolución temporal y comparativas
- **Gráficas combinadas**: Con múltiples ejes Y para cantidad y valor
- **Colores diferenciados**: Para mejor visualización

## 🎨 Características de la Interfaz

- **Diseño responsivo**: Funciona en desktop, tablet y móvil
- **Bootstrap 5**: Interfaz moderna y limpia
- **Font Awesome**: Iconos intuitivos
- **Alertas visuales**: 
  - Amarillo para contratos que vencen en 30 días
  - Rojo para contratos vencidos
- **Badges de estado**: Colores según el estado del contrato
- **Barras de progreso**: Para visualizar % de ejecución

## ⚠️ Notas Importantes

1. **Cálculos Automáticos**: El % de ejecución y valor pendiente se calculan automáticamente al guardar o editar un contrato.

2. **Validaciones**: El sistema valida:
   - Campos obligatorios
   - Formatos de fecha
   - Valores numéricos

3. **Filtros**: En la lista de contratos puedes filtrar por:
   - Texto (busca en cliente, empresa, número de contrato)
   - Estado
   - Año

4. **Exportación**: El archivo Excel exportado mantiene todos los formatos y puede ser importado de nuevo.

## 🔧 Solución de Problemas

### Error al ejecutar migraciones
Si obtienes un error al ejecutar las migraciones:
1. Asegúrate de que la aplicación NO está corriendo
2. Verifica la cadena de conexión en `appsettings.json`
3. Asegúrate de que SQL Server está corriendo

### Error al importar Excel
Si falla la importación:
1. Verifica que el archivo tenga exactamente 18 columnas
2. Asegúrate de que las fechas estén en formato de fecha de Excel
3. Verifica que los valores numéricos sean válidos

### Las gráficas no se muestran
1. Verifica tu conexión a internet (Chart.js se carga desde CDN)
2. Asegúrate de que hay datos en la base de datos

## 📞 Estructura del Proyecto

```
Koncilia_Contratos/
├── Controllers/
│   ├── ContratosController.cs      # CRUD de contratos + Import/Export
│   ├── ReportesController.cs       # Reportes y análisis
│   └── ConfiguracionController.cs  # Configuración del sistema
├── Models/
│   ├── Contrato.cs                 # Modelo de contrato
│   └── Configuracion.cs            # Modelo de configuración
├── Views/
│   ├── Contratos/                  # Vistas de contratos
│   ├── Reportes/                   # Vistas de reportes
│   └── Configuracion/              # Vistas de configuración
└── Data/
    └── ApplicationDbContext.cs     # Contexto de EF Core
```

## ✨ Próximos Pasos Sugeridos

Algunas mejoras que podrías implementar en el futuro:
- Notificaciones por email de contratos por vencer
- Historial de cambios en contratos
- Adjuntar documentos PDF a contratos
- Dashboard con datos en tiempo real
- Exportar reportes a PDF
- API REST para integración con otros sistemas

---

**¡El sistema está completo y listo para usar!** 🎉

