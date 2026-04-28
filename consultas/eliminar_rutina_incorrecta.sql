-- ============================================================
--  ELIMINAR RUTINA INCORRECTA — Rutinas · Ingenio La Cabaña
--  Ejecutar en: USE REPORTES
-- ============================================================

USE REPORTES;

-- ===========================================================
--  SECCIÓN 1 — RUTINA NORMAL (Instrumentistas)
-- ===========================================================
--  Usar cuando un instrumentista recibió área incorrecta o
--  necesita regenerar su rutina en el mismo turno.

--  PASO 1A: Identificar el correlativo
SELECT TOP 5
    R.Correlativo,
    R.Codigo_empleado,
    E.Nombre,
    R.Fecha,
    R.Turno,
    CASE R.IDgrupo WHEN 1 THEN 'Extracción' ELSE 'Alcalizado' END AS Area
FROM Rutinas R
INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
WHERE R.Codigo_empleado = 1055           -- <-- código del instrumentista
ORDER BY R.Correlativo DESC;

--  PASO 1B: Verificar instrumentos del correlativo encontrado
SELECT
    RI.Correlativo,
    RI.TAG,
    I.Nombre,
    CASE RI.EsObligatorio WHEN 1 THEN 'OBLIGATORIO' ELSE 'regular' END AS Tipo
FROM Rutina_instrumento RI
INNER JOIN Instrumentos I ON RI.TAG = I.TAG
WHERE RI.Correlativo = 999;              -- <-- reemplazar con el Correlativo

--  PASO 1C: Verificar equipos de desmontaje
SELECT RD.RutinaId, RD.TAG, RD.Reportado, RD.Desmontado
FROM Rutina_desmontaje RD
WHERE RD.RutinaId = 999;                -- <-- reemplazar con el Correlativo

--  ELIMINACIÓN — rutina normal
BEGIN TRANSACTION;
    DELETE FROM Rutina_desmontaje  WHERE RutinaId  = 999;   -- <-- Correlativo
    DELETE FROM Rutina_instrumento WHERE Correlativo = 999;  -- <-- Correlativo
    DELETE FROM Rutinas            WHERE Correlativo = 999;  -- <-- Correlativo
COMMIT TRANSACTION;
-- Si algo falla: reemplazar COMMIT por ROLLBACK TRANSACTION;

--  Verificación post-eliminación (deben devolver 0 filas):
SELECT * FROM Rutinas            WHERE Correlativo = 999;
SELECT * FROM Rutina_instrumento WHERE Correlativo = 999;
SELECT * FROM Rutina_desmontaje  WHERE RutinaId    = 999;


-- ===========================================================
--  SECCIÓN 2 — RUTINA AUXILIAR: TACHOS
-- ===========================================================
--  Usar cuando el empleado Auxiliar (16134) generó una rutina
--  de TACHOS incorrecta. Solo tiene registros en Rutina_instrumento
--  con EsObligatorio=1. No tiene entradas en Rutina_desmontaje.
USE REPORTES
--  PASO 2A: Identificar el correlativo de TACHOS del Auxiliar
SELECT TOP 5
    R.Correlativo,
    R.Codigo_empleado,
    E.Nombre,
    R.Fecha,
    R.Turno,
    CASE R.IDgrupo WHEN 1 THEN 'Extracción' ELSE 'Alcalizado' END AS Area
FROM Rutinas R
INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
WHERE R.Codigo_empleado = 16134          -- código del Auxiliar
  AND R.Turno = '7AM a 5PM'             -- identifica rutinas TACHOS
ORDER BY R.Correlativo DESC;

--  PASO 2B: Verificar equipos Brix del correlativo TACHOS
SELECT
    RI.Correlativo,
    RI.TAG,
    I.Nombre,
    I.TipoAnalisis,
    CASE RI.EsObligatorio WHEN 1 THEN 'OBLIGATORIO/TACHOS' ELSE 'regular' END AS Tipo
FROM Rutina_instrumento RI
INNER JOIN Instrumentos I ON RI.TAG = I.TAG
WHERE RI.Correlativo = 204;              -- <-- reemplazar con el Correlativo

--  ELIMINACIÓN — rutina TACHOS (no hay entradas en Rutina_desmontaje)
BEGIN TRANSACTION;
    DELETE FROM Rutina_instrumento WHERE Correlativo = 204;  -- <-- Correlativo
    DELETE FROM Rutinas            WHERE Correlativo = 204;  -- <-- Correlativo
COMMIT TRANSACTION;

--  Verificación post-eliminación (deben devolver 0 filas):
SELECT * FROM Rutinas            WHERE Correlativo = 204;
SELECT * FROM Rutina_instrumento WHERE Correlativo = 204;


-- ===========================================================
--  SECCIÓN 3 — RUTINA AUXILIAR: RELEVADO
-- ===========================================================
--  Usar cuando el Auxiliar (16134) generó una rutina de relevado
--  incorrecta. Se guarda bajo el código del Auxiliar con el turno
--  calculado. No tiene entradas en Rutina_desmontaje.

--  PASO 3A: Identificar el correlativo del relevado del Auxiliar
SELECT TOP 5
    R.Correlativo,
    R.Codigo_empleado,
    E.Nombre,
    R.Fecha,
    R.Turno,
    CASE R.IDgrupo WHEN 1 THEN 'Extracción' ELSE 'Alcalizado' END AS Area
FROM Rutinas R
INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
WHERE R.Codigo_empleado = 16134          -- código del Auxiliar
  AND R.Turno <> '7AM a 5PM'            -- excluye TACHOS, muestra solo relevados
ORDER BY R.Correlativo DESC;

--  PASO 3B: Verificar instrumentos del relevado
SELECT
    RI.Correlativo,
    RI.TAG,
    I.Nombre,
    CASE RI.EsObligatorio WHEN 1 THEN 'OBLIGATORIO' ELSE 'regular' END AS Tipo
FROM Rutina_instrumento RI
INNER JOIN Instrumentos I ON RI.TAG = I.TAG
WHERE RI.Correlativo = 205;              -- <-- reemplazar con el Correlativo

--  ELIMINACIÓN — rutina relevado (no hay entradas en Rutina_desmontaje)
BEGIN TRANSACTION;
    DELETE FROM Rutina_instrumento WHERE Correlativo = 205;  -- <-- Correlativo
    DELETE FROM Rutinas            WHERE Correlativo = 205;  -- <-- Correlativo
COMMIT TRANSACTION;

--  Verificación post-eliminación (deben devolver 0 filas):
SELECT * FROM Rutinas            WHERE Correlativo = 205;
SELECT * FROM Rutina_instrumento WHERE Correlativo = 205;


-- ============================================================
--  HISTORIAL DE USOS
-- ============================================================
-- 15-abr-2026 | Exiles Ceren (1055) | Corr. 130
--   Causa: generó a las 21:57 sin corrección nocturna.
--   Fix aplicado en Generadorrutinas.aspx.cs.
