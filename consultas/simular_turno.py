"""
Simulador de generacion de rutinas — Ingenio La Cabana
Replica ObtenerAreaDeterminista() de Generadorrutinas.aspx.cs
para verificar la distribucion de areas antes de que los empleados generen.
"""

import pyodbc
from datetime import datetime, timedelta, time, date

# ── Conexion (cadena del Web.config, ConexionRutinasMTI) ──────────────────────
CONN_STRING = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=10.1.1.222\\SQLEXPRESS;"
    "DATABASE=REPORTES;"
    "UID=su;"
    "PWD=sa;"
    "TrustServerCertificate=yes;"
)

FECHA_INICIO_ZAFRA = date(2025, 11, 25)


# ── Replica de DeterminarTurnoActual() ────────────────────────────────────────
def determinar_turno(ahora: datetime) -> str:
    h = ahora.time()
    if ahora.weekday() == 6:  # domingo
        if time(5, 41) <= h <= time(17, 40, 59):
            return "06:00 A 18:00"
    if time(5, 41) <= h <= time(13, 40, 59):
        return "06:00 A 14:00"
    if time(13, 41) <= h <= time(21, 40, 59):
        return "14:00 A 22:00"
    return "22:00 - 06:00"


# ── Replica de ObtenerAreaDeterminista() ──────────────────────────────────────
def obtener_area(puesto_fijo: int, es_cuarto_turno: bool, ahora: datetime) -> str:
    h = ahora.time()

    # diaLaboral: ancla el turno noche al dia en que inicio
    dia_laboral = (ahora - timedelta(days=1)) if h < time(5, 41) else ahora

    dia_zafra = (dia_laboral.date() - FECHA_INICIO_ZAFRA).days + 1
    es_semana_par = ((dia_zafra // 7) % 2 == 0)
    dia_juliano = dia_laboral.timetuple().tm_yday

    # Correccion turno noche (umbral corregido a 21:41 el 2026-04-15)
    es_turno_noche = h >= time(21, 41) or h < time(5, 41)
    if es_turno_noche:
        dia_juliano += 1

    area_puesto1 = "Extraccion" if (dia_juliano + 1) % 2 == 0 else "Alcalizado"
    area_puesto2 = "Alcalizado" if area_puesto1 == "Extraccion" else "Extraccion"

    if es_cuarto_turno:
        dia = ahora.weekday()  # 0=lun … 6=dom
        h_num = ahora.hour
        m_num = ahora.minute
        releva_puesto1 = False
        if dia in (1, 3):  # martes o jueves
            releva_puesto1 = True
        elif (dia == 6 and h_num >= 22) or (dia == 0 and (h_num > 21 or (h_num == 21 and m_num >= 41))):
            releva_puesto1 = not es_semana_par
        elif dia in (0, 1) and h_num < 6:
            releva_puesto1 = not es_semana_par
        return area_puesto1 if releva_puesto1 else area_puesto2
    else:
        return area_puesto1 if puesto_fijo == 1 else area_puesto2


# ── Consulta de datos del empleado ────────────────────────────────────────────
def consultar_empleados(codigos: list) -> dict:
    empleados = {}
    placeholders = ",".join("?" * len(codigos))
    sql = f"""
        SELECT Codigo_empleado, Nombre, PuestoFijo, EsCuartoTurno
        FROM Empleado
        WHERE Codigo_empleado IN ({placeholders})
    """
    with pyodbc.connect(CONN_STRING) as conn:
        cursor = conn.cursor()
        cursor.execute(sql, codigos)
        for row in cursor.fetchall():
            empleados[row.Codigo_empleado] = {
                "nombre":        row.Nombre.strip(),
                "puesto_fijo":   row.PuestoFijo or 0,
                "es_cuarto_turno": bool(row.EsCuartoTurno),
            }
    return empleados


# ── Simulacion principal ──────────────────────────────────────────────────────
def simular(codigos: list, ahora: datetime = None):
    if ahora is None:
        ahora = datetime.now()

    turno = determinar_turno(ahora)
    print(f"\n{'='*60}")
    print(f"  SIMULACION DE RUTINAS — {ahora.strftime('%d/%m/%Y %H:%M')}")
    print(f"  Turno detectado : {turno}")
    print(f"{'='*60}\n")

    # Debug del calculo de dia
    h = ahora.time()
    dia_laboral = (ahora - timedelta(days=1)) if h < time(5, 41) else ahora
    dia_zafra   = (dia_laboral.date() - FECHA_INICIO_ZAFRA).days + 1
    dia_juliano = dia_laboral.timetuple().tm_yday
    es_noche    = h >= time(21, 41) or h < time(5, 41)
    dj_final    = dia_juliano + (1 if es_noche else 0)
    area_p1     = "Extraccion" if (dj_final + 1) % 2 == 0 else "Alcalizado"
    area_p2     = "Alcalizado" if area_p1 == "Extraccion" else "Extraccion"

    print(f"  Dia zafra       : {dia_zafra}")
    print(f"  Dia juliano     : {dia_juliano}  (+1 noche={es_noche}) => {dj_final}")
    print(f"  (dj+1)%2        : {(dj_final+1) % 2}  =>  Puesto1={area_p1} | Puesto2={area_p2}")
    print()

    try:
        empleados = consultar_empleados(codigos)
    except Exception as e:
        print(f"  [ERROR BD] {e}\n")
        empleados = {}

    print(f"  {'COD':>6}  {'NOMBRE':<30}  {'P.FIJO':>6}  {'4°T':>4}  {'AREA ASIGNADA'}")
    print(f"  {'-'*6}  {'-'*30}  {'-'*6}  {'-'*4}  {'-'*15}")

    areas_asignadas = {}
    for codigo in codigos:
        emp = empleados.get(codigo)
        if emp:
            nombre        = emp["nombre"]
            puesto_fijo   = emp["puesto_fijo"]
            es_cuarto     = emp["es_cuarto_turno"]
        else:
            nombre        = "(no encontrado en BD)"
            puesto_fijo   = 0
            es_cuarto     = False

        area = obtener_area(puesto_fijo, es_cuarto, ahora)
        areas_asignadas[codigo] = area
        cuarto_str = "SI" if es_cuarto else "no"
        print(f"  {codigo:>6}  {nombre:<30}  {puesto_fijo:>6}  {cuarto_str:>4}  {area}")

    # Verificar colisiones
    print()
    valores = list(areas_asignadas.values())
    if len(valores) == len(set(valores)):
        print("  [OK] Distribucion correcta — cada empleado tiene un area distinta.")
    else:
        from collections import Counter
        repetidas = [a for a, c in Counter(valores).items() if c > 1]
        print(f"  [ALERTA] Colision detectada en: {', '.join(repetidas)}")
        print("           Dos o mas empleados recibirian la misma area.")
    print()


# ── Punto de entrada ──────────────────────────────────────────────────────────
if __name__ == "__main__":
    # Empleados del turno tarde de hoy: 837 (normal) y 16134 (reemplaza a 15997)
    CODIGOS_TURNO_TARDE = ["837", "16134"]

    # Simula con la hora actual (turno tarde real)
    simular(CODIGOS_TURNO_TARDE)

    # Para probar un turno especifico, descomenta y ajusta:
    # simular(CODIGOS_TURNO_TARDE, ahora=datetime(2026, 4, 18, 15, 0))
