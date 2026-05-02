-- Tabla: RegistroFallos
-- Base de datos: REPORTES (10.1.1.222\SQLEXPRESS)
-- Ejecutar una sola vez
USE REPORTES;
CREATE TABLE RegistroFallos (
    ID             INT            IDENTITY(1,1)  NOT NULL,
    Instrumentista NVARCHAR(500)  NOT NULL,
    TAG            NVARCHAR(500)  NULL,
    Instrumento    NVARCHAR(500)  NULL,
    FalloInducido  BIT            NOT NULL,
    NivelDano      NVARCHAR(500)  NULL,
    TipoDano       NVARCHAR(500)  NULL,
    PosibleCausa   NVARCHAR(500)  NULL,
    Comentarios    NVARCHAR(500)  NULL,
    Reemplaza      BIT            NOT NULL,
    Interviene     BIT            NOT NULL,
    Solucion       NVARCHAR(500)  NULL,

    CONSTRAINT PK_RegistroFallos PRIMARY KEY (ID)
);
