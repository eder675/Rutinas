-- ============================================================
-- TABLAS DE DESMONTAJE DE EQUIPOS — Rutinas Ingenio La Cabaña
-- Ejecutar una sola vez en la BD ConexionRutinasMTI
-- ============================================================

-- 5. Asignación de áreas de desmontaje por empleado (máximo 2 áreas)
--    Si un empleado no tiene fila aquí, usa la rotación automática.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DesmontajeEmpleadoArea')
BEGIN
    CREATE TABLE DesmontajeEmpleadoArea (
        Id               INT IDENTITY(1,1) PRIMARY KEY,
        Codigo_empleado  VARCHAR(25)  NOT NULL,
        Area1Id          INT          NULL,
        Area2Id          INT          NULL,
        CONSTRAINT FK_DesmontajeEmpleadoArea_Empleado FOREIGN KEY (Codigo_empleado) REFERENCES Empleado(Codigo_empleado),
        CONSTRAINT FK_DesmontajeEmpleadoArea_Area1    FOREIGN KEY (Area1Id) REFERENCES Area(IDarea),
        CONSTRAINT FK_DesmontajeEmpleadoArea_Area2    FOREIGN KEY (Area2Id) REFERENCES Area(IDarea),
        CONSTRAINT UQ_DesmontajeEmpleadoArea          UNIQUE (Codigo_empleado)
    );
END

-- Agregar columnas de palabra clave si la tabla ya existía sin ellas
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DesmontajeEmpleadoArea') AND name = 'Keyword1')
    ALTER TABLE DesmontajeEmpleadoArea ADD Keyword1 VARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DesmontajeEmpleadoArea') AND name = 'Keyword2')
    ALTER TABLE DesmontajeEmpleadoArea ADD Keyword2 VARCHAR(100) NULL;

-- ============================================================
-- INDEPENDIZAR DesmontajeInstrumento de REPORTES.Instrumentos
-- El pool ahora usa instrumentos de Vinetas (cualquier equipo
-- de planta, no solo los del recorrido de rutinas).
-- ============================================================

-- Quitar FK de TAG hacia Instrumentos (ya no es necesaria)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DesmontajeInstrumento_Instrumento')
    ALTER TABLE DesmontajeInstrumento DROP CONSTRAINT FK_DesmontajeInstrumento_Instrumento;

-- Quitar FK de TAG en Rutina_desmontaje hacia Instrumentos
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RutinaDesmontaje_Instrumento')
    ALTER TABLE Rutina_desmontaje DROP CONSTRAINT FK_RutinaDesmontaje_Instrumento;

-- Agregar Nombre y AreaId directamente en el pool
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DesmontajeInstrumento') AND name = 'Nombre')
    ALTER TABLE DesmontajeInstrumento ADD Nombre VARCHAR(200) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('DesmontajeInstrumento') AND name = 'AreaId')
BEGIN
    ALTER TABLE DesmontajeInstrumento ADD AreaId INT NULL;
    ALTER TABLE DesmontajeInstrumento
        ADD CONSTRAINT FK_DesmontajeInstrumento_Area FOREIGN KEY (AreaId) REFERENCES Area(IDarea);
END

-- Migrar entradas existentes del pool (si venían de REPORTES.Instrumentos)
UPDATE DI
SET    DI.Nombre = I.Nombre,
       DI.AreaId = I.IDarea
FROM   DesmontajeInstrumento DI
INNER JOIN Instrumentos I ON DI.TAG = I.TAG
WHERE  (DI.Nombre = '' OR DI.Nombre IS NULL)
  AND  DI.AreaId IS NULL;

-- 1. Configuración global de visibilidad de tablas y cantidades
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DesmontajeConfig')
BEGIN
    CREATE TABLE DesmontajeConfig (
        Id                       INT  NOT NULL DEFAULT 1,
        MostrarTablaInstrumentos BIT  NOT NULL DEFAULT 1,
        MostrarTablaObligatorios BIT  NOT NULL DEFAULT 1,
        MostrarTablaDesmontaje   BIT  NOT NULL DEFAULT 0,
        CantManana               INT  NOT NULL DEFAULT 2,
        CantTarde                INT  NOT NULL DEFAULT 2,
        CantNoche                INT  NOT NULL DEFAULT 2,
        CONSTRAINT PK_DesmontajeConfig PRIMARY KEY (Id),
        CONSTRAINT CK_DesmontajeConfig_SingleRow CHECK (Id = 1)
    );

    INSERT INTO DesmontajeConfig (Id, MostrarTablaInstrumentos, MostrarTablaObligatorios, MostrarTablaDesmontaje, CantManana, CantTarde, CantNoche)
    VALUES (1, 1, 1, 0, 2, 2, 2);
END

-- 2. Areas habilitadas para aparecer en la tabla de desmontaje
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DesmontajeAreaHabilitada')
BEGIN
    CREATE TABLE DesmontajeAreaHabilitada (
        Id     INT IDENTITY(1,1) PRIMARY KEY,
        AreaId INT NOT NULL,
        CONSTRAINT FK_DesmontajeAreaHabilitada_Area FOREIGN KEY (AreaId) REFERENCES Area(IDarea)
    );
END

-- Verificar el tipo exacto de Instrumentos.TAG antes de continuar:
-- SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'Instrumentos' AND COLUMN_NAME = 'TAG'
--
-- TAG se declara con el mismo tipo que Instrumentos.TAG (probable NVARCHAR).
-- Si tu columna usa VARCHAR, cambia NVARCHAR por VARCHAR en las dos tablas de abajo.

-- 3. Pool de instrumentos pendientes de desmontar
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DesmontajeInstrumento')
BEGIN
    CREATE TABLE DesmontajeInstrumento (
        Id                 INT IDENTITY(1,1) PRIMARY KEY,
        TAG                VARCHAR(50) NOT NULL,
        Estado             TINYINT      NOT NULL DEFAULT 0,  -- 0=Pendiente  1=Desmontado  2=Excluido
        RazonExclusion     NVARCHAR(500) NULL,
        FechaActualizacion DATETIME     NULL,
        EmpleadoId         NVARCHAR(20) NULL,
        CONSTRAINT FK_DesmontajeInstrumento_Instrumento FOREIGN KEY (TAG) REFERENCES Instrumentos(TAG)
    );
END

-- 4. Detalle de desmontaje por rutina
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Rutina_desmontaje')
BEGIN
    CREATE TABLE Rutina_desmontaje (
        Id                INT IDENTITY(1,1) PRIMARY KEY,
        RutinaId          INT           NOT NULL,
        TAG               VARCHAR(50)  NOT NULL,
        Reportado         BIT           NOT NULL DEFAULT 0,
        Desmontado        BIT           NOT NULL DEFAULT 0,
        RazonNoDesmontaje NVARCHAR(500) NULL,
        CONSTRAINT FK_RutinaDesmontaje_Rutina      FOREIGN KEY (RutinaId) REFERENCES Rutinas(Correlativo),
        CONSTRAINT FK_RutinaDesmontaje_Instrumento FOREIGN KEY (TAG)      REFERENCES Instrumentos(TAG)
    );
END
