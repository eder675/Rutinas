-- ============================================================
--  ELIMINAR RUTINA INCORRECTA — Rutinas · Ingenio La Cabaña
--  Usar cuando un empleado recibió un área incorrecta y necesita
--  regenerar su rutina en el mismo turno.
-- ============================================================
--
--  CUÁNDO USARLO
--  -------------
--  - Dos empleados con PuestoFijo distinto recibieron la misma área.
--  - Un empleado generó su rutina en la ventana 21:41–21:59 y el
--    algoritmo no aplicó la corrección nocturna correctamente.
--  - Cualquier otro caso donde la rutina asignada sea incorrecta
--    y el empleado deba volver a generarla.
--
--  PASOS PREVIOS
--  -------------
--  1. Identificar el Correlativo incorrecto:
--
USE REPORTES
SELECT TOP 5
    R.Correlativo,
    R.Codigo_empleado,
    E.Nombre,
    R.Fecha,
    R.Turno,
    CASE R.IDgrupo WHEN 1 THEN 'Extracción' ELSE 'Alcalizado' END AS Area
FROM Rutinas R
INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
WHERE R.Codigo_empleado = 16134          -- <-- reemplazar con el código del empleado
ORDER BY R.Correlativo DESC;

--  2. Verificar los instrumentos asociados al correlativo encontrado:
--
SELECT
    RI.Correlativo,
    RI.TAG,
    I.Nombre,
    CASE RI.EsObligatorio WHEN 1 THEN 'OBLIGATORIO' ELSE 'regular' END AS Tipo
FROM Rutina_instrumento RI
INNER JOIN Instrumentos I ON RI.TAG = I.TAG
WHERE RI.Correlativo = 148;              -- <-- reemplazar con el Correlativo

--  3. Verificar equipos de desmontaje asociados al correlativo:
--
SELECT
    RD.RutinaId,
    RD.TAG,
    RD.Reportado,
    RD.Desmontado
FROM Rutina_desmontaje RD
WHERE RD.RutinaId = 148;                -- <-- reemplazar con el Correlativo

-- ============================================================
--  ELIMINACIÓN (ejecutar en este orden — respetar las FK)
-- ============================================================

BEGIN TRANSACTION;

    -- Paso 1: eliminar equipos de desmontaje (tabla hija nueva)
    DELETE FROM Rutina_desmontaje
    WHERE RutinaId = 148;               -- <-- reemplazar con el Correlativo

    -- Paso 2: eliminar el detalle de instrumentos (tabla hija)
    DELETE FROM Rutina_instrumento
    WHERE Correlativo = 148;            -- <-- reemplazar con el Correlativo

    -- Paso 3: eliminar la cabecera de la rutina (tabla padre)
    DELETE FROM Rutinas
    WHERE Correlativo = 148;            -- <-- reemplazar con el Correlativo

COMMIT TRANSACTION;
-- Si algo falla, reemplazar COMMIT por ROLLBACK TRANSACTION;

-- ============================================================
--  VERIFICACIÓN POST-ELIMINACIÓN
-- ============================================================

-- Confirmar que ya no existen registros:
SELECT * FROM Rutinas              WHERE Correlativo = 148;
SELECT * FROM Rutina_instrumento   WHERE Correlativo = 148;
SELECT * FROM Rutina_desmontaje    WHERE RutinaId    = 148;
-- Las tres consultas deben devolver 0 filas.

-- ============================================================
--  EJEMPLO REAL — 15-abr-2026, Exiles Ceren (1055), Corr. 130
--  Causa: generó a las 21:57 (ventana sin corrección nocturna).
--  El bug fue corregido en Generadorrutinas.aspx.cs línea 277:
--  TimeSpan(22,0,0) → TimeSpan(21,41,0)
-- ============================================================
