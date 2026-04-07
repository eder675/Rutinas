"""
Diagnóstico: Fallo en asignación 12h/4h del domingo 05-Apr-2026
Proyecto: Rutinas — Ingenio La Cabaña
Requiere: pip install pyodbc
"""

import pyodbc
from datetime import datetime, timedelta

SERVER   = r"10.1.1.222\SQLEXPRESS"
DATABASE = "REPORTES"
USER     = "su"
PASSWORD = "sa"

conn_str = (
    f"DRIVER={{ODBC Driver 17 for SQL Server}};"
    f"SERVER={SERVER};"
    f"DATABASE={DATABASE};"
    f"UID={USER};"
    f"PWD={PASSWORD};"
    f"TrustServerCertificate=yes;"
)

FECHA_INICIO_ZAFRA = datetime(2025, 11, 25)
EMPLEADOS_INTERES  = ["1164", "15967"]   # los dos del fallo reportado

def conectar():
    try:
        return pyodbc.connect(conn_str)
    except Exception as e:
        print(f"[ERROR] No se pudo conectar: {e}")
        return None

# ---------------------------------------------------------------------------
# 1. Replica exacta del algoritmo de Default.aspx.cs
# ---------------------------------------------------------------------------
def calcular_jornada_domingo(fecha: datetime, puesto_fijo: int) -> str:
    """
    Replica de la lógica CORREGIDA en Default.aspx.cs btnGenerarRutina_Click.
    Retorna '12h' o '4h' para el puesto dado en esa fecha.

    Calibrado con FECHA_INICIO_ZAFRA = 25-Nov-2025:
      PuestoFijo=1: semana PAR  → 4h  | semana IMPAR → 12h
      PuestoFijo=2: semana PAR  → 12h | semana IMPAR → 4h
    """
    dia_zafra    = (fecha - FECHA_INICIO_ZAFRA).days + 1
    semana_zafra = dia_zafra // 7
    es_semana_par = (semana_zafra % 2 == 0)

    if puesto_fijo == 1:
        return "4h" if es_semana_par else "12h"   # PAR → 4h, IMPAR → 12h
    elif puesto_fijo == 2:
        return "12h" if es_semana_par else "4h"   # PAR → 12h, IMPAR → 4h
    return "desconocido"

# ---------------------------------------------------------------------------
# 2. Consultar registros del domingo 05-Apr-2026
# ---------------------------------------------------------------------------
def registros_domingo(cursor, fecha_domingo: datetime):
    inicio = fecha_domingo.replace(hour=17, minute=30, second=0)
    fin    = fecha_domingo.replace(hour=23, minute=59, second=59)

    print("\n" + "="*65)
    print(f"REGISTROS del {fecha_domingo.strftime('%A %d-%b-%Y')} (17:30 – 23:59)")
    print("="*65)

    cursor.execute("""
        SELECT R.Correlativo, R.Fecha, R.Turno, R.Codigo_empleado,
               E.PuestoFijo, E.EsCuartoTurno
        FROM Rutinas R
        INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
        WHERE R.Fecha >= ? AND R.Fecha <= ?
        ORDER BY R.Fecha
    """, inicio, fin)

    rows = cursor.fetchall()
    if not rows:
        print("  (sin registros en ese rango)")
        return rows

    print(f"  {'Corr':>6}  {'Fecha':^22}  {'Turno':^18}  {'Empleado':^10}  {'PuestoFijo':^10}  {'CuartoTurno':^11}")
    print(f"  {'-'*6}  {'-'*22}  {'-'*18}  {'-'*10}  {'-'*10}  {'-'*11}")
    for r in rows:
        marca = " <<< REPORTADO" if r.Codigo_empleado in EMPLEADOS_INTERES else ""
        print(f"  {r.Correlativo:>6}  {str(r.Fecha):^22}  {r.Turno:^18}  "
              f"{r.Codigo_empleado:^10}  {str(r.PuestoFijo):^10}  {str(r.EsCuartoTurno):^11}{marca}")
    return rows

# ---------------------------------------------------------------------------
# 3. Simular qué debió haber calculado el algoritmo
# ---------------------------------------------------------------------------
def simular_algoritmo(cursor, fecha_domingo: datetime):
    print("\n" + "="*65)
    print(f"SIMULACIÓN del algoritmo para {fecha_domingo.strftime('%d-%b-%Y')}")
    print("="*65)

    dia_zafra    = (fecha_domingo - FECHA_INICIO_ZAFRA).days + 1
    semana_zafra = dia_zafra // 7
    es_semana_par = (semana_zafra % 2 == 0)

    print(f"  Fecha inicio zafra : {FECHA_INICIO_ZAFRA.strftime('%d-%b-%Y')}")
    print(f"  Día de zafra       : {dia_zafra}")
    print(f"  Semana de zafra    : {semana_zafra}  (dia_zafra // 7)")
    print(f"  ¿Es semana par?    : {es_semana_par}  (semana_zafra % 2 == 0)")
    print()
    print("  Resultado por fórmula actual:")
    print(f"    PuestoFijo=1 → {'12h' if es_semana_par else '4h'}  "
          f"(esSemanaPar={es_semana_par} → '12h' if par, '4h' si impar)")
    print(f"    PuestoFijo=2 → {'12h' if not es_semana_par else '4h'}  "
          f"(opuesto al PuestoFijo=1)")

    # Mostrar para cada empleado de interés
    print()
    print("  Empleados reportados:")
    for cod in EMPLEADOS_INTERES:
        cursor.execute(
            "SELECT PuestoFijo, EsCuartoTurno FROM Empleado WHERE Codigo_empleado = ?", cod)
        row = cursor.fetchone()
        if row:
            jornada = calcular_jornada_domingo(fecha_domingo, row.PuestoFijo)
            print(f"    Empleado {cod}: PuestoFijo={row.PuestoFijo}, "
                  f"EsCuartoTurno={row.EsCuartoTurno}  →  algoritmo asignó [{jornada}]")
        else:
            print(f"    Empleado {cod}: NO encontrado en tabla Empleado")

# ---------------------------------------------------------------------------
# 4. Historial de domingos noche anteriores para los mismos empleados
# ---------------------------------------------------------------------------
def historial_domingos_noche(cursor, desde: datetime, semanas=8):
    print("\n" + "="*65)
    print(f"HISTORIAL domingos noche — últimas {semanas} semanas")
    print("="*65)

    limite = desde - timedelta(weeks=semanas)

    cursor.execute("""
        SELECT R.Correlativo, R.Fecha, R.Turno, R.Codigo_empleado,
               E.PuestoFijo
        FROM Rutinas R
        INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
        WHERE R.Codigo_empleado IN (?, ?)
          AND R.Fecha >= ?
          AND (R.Turno LIKE '%18:00%' OR R.Turno LIKE '%22:00%')
        ORDER BY R.Fecha
    """, EMPLEADOS_INTERES[0], EMPLEADOS_INTERES[1], limite)

    rows = cursor.fetchall()
    if not rows:
        print("  (sin registros en el período)")
        return

    print(f"  {'Fecha':^22}  {'Turno':^18}  {'Empleado':^10}  {'PuestoFijo':^10}")
    print(f"  {'-'*22}  {'-'*18}  {'-'*10}  {'-'*10}")
    for r in rows:
        print(f"  {str(r.Fecha):^22}  {r.Turno:^18}  {r.Codigo_empleado:^10}  {str(r.PuestoFijo):^10}")

# ---------------------------------------------------------------------------
# 5. Verificar paridad semana a semana para los próximos domingos
# ---------------------------------------------------------------------------
def tabla_paridad_semanas(cursor, desde: datetime, n=6):
    print("\n" + "="*65)
    print(f"PARIDAD de las próximas {n} semanas (domingos)")
    print("="*65)

    # Encontrar el próximo domingo desde 'desde'
    dias_hasta_domingo = (6 - desde.weekday()) % 7
    primer_domingo = desde + timedelta(days=dias_hasta_domingo)

    print(f"  {'Domingo':^12}  {'DíaZafra':^9}  {'SemZafra':^9}  {'Par?':^6}  "
          f"{'PuestoFijo=1':^13}  {'PuestoFijo=2':^13}")
    print(f"  {'-'*12}  {'-'*9}  {'-'*9}  {'-'*6}  {'-'*13}  {'-'*13}")

    for i in range(n):
        d = primer_domingo + timedelta(weeks=i)
        dia_z = (d - FECHA_INICIO_ZAFRA).days + 1
        sem_z = dia_z // 7
        par   = (sem_z % 2 == 0)
        j1 = "12h" if par else "4h"
        j2 = "4h" if par else "12h"
        marca = " ← FALLO" if d.date() == datetime(2026, 4, 5).date() else ""
        print(f"  {d.strftime('%d-%b-%Y'):^12}  {dia_z:^9}  {sem_z:^9}  "
              f"{'PAR' if par else 'IMPAR':^6}  {j1:^13}  {j2:^13}{marca}")

# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    print("Conectando a la base de datos...")
    conn = conectar()
    if conn is None:
        exit(1)

    cursor = conn.cursor()
    domingo_fallo = datetime(2026, 4, 5)

    registros_domingo(cursor, domingo_fallo)
    simular_algoritmo(cursor, domingo_fallo)
    historial_domingos_noche(cursor, domingo_fallo)
    tabla_paridad_semanas(cursor, datetime.now())

    cursor.close()
    conn.close()
    print("\nDone.")
    input("\nPresiona Enter para cerrar...")
