-- Tabla: AreasDesmontajeExcluida
-- Base de datos: REPORTES (10.1.1.222\SQLEXPRESS)
-- Ejecutar una sola vez

CREATE TABLE AreasDesmontajeExcluida (
    ID   INT           IDENTITY(1,1) NOT NULL,
    Area NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_AreasDesmontajeExcluida PRIMARY KEY (ID),
    CONSTRAINT UQ_AreasDesmontajeExcluida_Area UNIQUE (Area)
);
