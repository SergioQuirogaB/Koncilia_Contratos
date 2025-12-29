-- Script SQL para agregar campos de Tipo de Documento a la tabla Contratos
-- Ejecutar este script directamente sobre la base de datos

USE [NombreDeTuBaseDeDatos]; -- Reemplazar con el nombre de tu base de datos
GO

-- Verificar si las columnas ya existen antes de agregarlas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contratos]') AND name = 'TipoDocumento')
BEGIN
    ALTER TABLE [dbo].[Contratos]
    ADD [TipoDocumento] NVARCHAR(50) NULL;
    PRINT 'Columna TipoDocumento agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna TipoDocumento ya existe.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contratos]') AND name = 'Numero')
BEGIN
    ALTER TABLE [dbo].[Contratos]
    ADD [Numero] NVARCHAR(100) NULL;
    PRINT 'Columna Numero agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna Numero ya existe.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contratos]') AND name = 'Valor')
BEGIN
    ALTER TABLE [dbo].[Contratos]
    ADD [Valor] DECIMAL(18,2) NULL;
    PRINT 'Columna Valor agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna Valor ya existe.';
END
GO

-- Verificar que las columnas se agregaron correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Contratos'
    AND COLUMN_NAME IN ('TipoDocumento', 'Numero', 'Valor')
ORDER BY COLUMN_NAME;
GO

PRINT 'Script ejecutado correctamente.';
GO

