-- ============================================================
-- CREAR TABLA DesmontajeAvance en BD REPORTES
-- Servidor: 10.1.1.222\SQLEXPRESS
-- ============================================================

USE REPORTES;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[DesmontajeAvance]')
      AND type = N'U'
)
BEGIN
    CREATE TABLE DesmontajeAvance (
        ID               INT IDENTITY(1,1) NOT NULL,
        TAG              NVARCHAR(50)      NOT NULL,
        Descripcion      NVARCHAR(500)     NULL,
        Area             NVARCHAR(200)     NULL,
        Desmontado       BIT               NOT NULL DEFAULT 0,
        FechaDeclaracion DATETIME          NOT NULL DEFAULT GETDATE(),
        CodigoEmpleado   NVARCHAR(50)      NOT NULL,
        NombreEmpleado   NVARCHAR(200)     NULL,
        CONSTRAINT PK_DesmontajeAvance    PRIMARY KEY (ID),
        CONSTRAINT UQ_DesmontajeAvance_TAG UNIQUE (TAG)
    );

    PRINT 'Tabla DesmontajeAvance creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla DesmontajeAvance ya existe.';
END
GO
