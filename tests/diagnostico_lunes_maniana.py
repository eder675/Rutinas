"""
Diagnóstico: repetición de área sábado-noche → lunes-mañana
Proyecto: Rutinas — Ingenio La Cabaña

Escenario:
  - Empleados 837 y 15997 trabajan el sábado noche (22:00 Sab → 06:00 Dom).
  - Descansan el domingo.
  - Regresan el lunes de mañana (06:00 Lun → 14:00).
  - El algoritmo les asigna la MISMA área que el sábado (bug de paridad par).

Pregunta: ¿el fix de diaLaboral (turno noche / medianoche) soluciona esto?
Respuesta anticipada: NO — el lunes 07:00 tiene hora >= 05:41, por lo que
diaLaboral = lunes (D+2), que tiene la misma paridad que sábado (D).
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
EMPLEADOS          = ["837", "15997"]

def conectar():
    try:
        return pyodbc.connect(conn_str)
    except Exception as e:
        print(f"[ERROR] No se pudo conectar: {e}")
        return None

# ---------------------------------------------------------------------------
# Réplica del algoritmo ANTES del fix (diaJuliano = ahora)
# ---------------------------------------------------------------------------
def area_sin_fix(ahora: datetime, puesto_fijo: int) -> str:
    dj    = ahora.timetuple().tm_yday
    area1 = "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"
    area2 = "Alcalizado" if area1 == "Extracción" else "Extracción"
    return area1 if puesto_fijo == 1 else area2

# ---------------------------------------------------------------------------
# Réplica del algoritmo CON el fix de diaLaboral (turno noche / medianoche)
# ---------------------------------------------------------------------------
def dia_laboral(ahora: datetime) -> datetime:
    """Ancla las horas 00:00–05:40 al día anterior (turno noche)."""
    if ahora.hour < 5 or (ahora.hour == 5 and ahora.minute < 41):
        return ahora - timedelta(days=1)
    return ahora

def area_con_fix(ahora: datetime, puesto_fijo: int) -> str:
    """Fix v1: solo diaLaboral (ancla medianoche). NO corrige el bug sáb→lun."""
    dl    = dia_laboral(ahora)
    dj    = dl.timetuple().tm_yday
    area1 = "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"
    area2 = "Alcalizado" if area1 == "Extracción" else "Extracción"
    return area1 if puesto_fijo == 1 else area2

# ---------------------------------------------------------------------------
# Réplica del algoritmo CON ambos fixes (diaLaboral + dj-1 turno mañana)
# Equivale exactamente al código en Generadorrutinas.aspx.cs
# ---------------------------------------------------------------------------
def es_turno_maniana(dl: datetime, hora_actual) -> bool:
    """True si la hora cae dentro del bloque de turno mañana."""
    h = hora_actual
    inicio = timedelta(hours=5, minutes=41)
    if dl.weekday() == 6:  # Domingo
        fin = timedelta(hours=17, minutes=40, seconds=59)
    else:
        fin = timedelta(hours=13, minutes=40, seconds=59)
    return inicio <= h <= fin

def area_fix_completo(ahora: datetime, puesto_fijo: int) -> str:
    """Fix v2: diaLaboral + dj-1 para turno mañana (fix implementado en producción)."""
    hora_actual = timedelta(hours=ahora.hour, minutes=ahora.minute, seconds=ahora.second)
    dl  = dia_laboral(ahora)
    dj  = dl.timetuple().tm_yday
    if es_turno_maniana(dl, hora_actual):
        dj -= 1
    area1 = "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"
    area2 = "Alcalizado" if area1 == "Extracción" else "Extracción"
    return area1 if puesto_fijo == 1 else area2

# ---------------------------------------------------------------------------
# 1. Registros reales de la BD para 837 y 15997
# ---------------------------------------------------------------------------
def registros_empleados(cursor, desde: datetime, hasta: datetime):
    print("\n" + "="*70)
    print(f"REGISTROS BD — {desde.strftime('%d-%b-%Y')} al {hasta.strftime('%d-%b-%Y')}")
    print("="*70)
    cursor.execute("""
        SELECT R.Correlativo, R.Fecha, R.Turno, R.Codigo_empleado,
               R.IDgrupo, E.PuestoFijo
        FROM Rutinas R
        INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
        WHERE R.Codigo_empleado IN (?, ?)
          AND R.Fecha >= ? AND R.Fecha <= ?
        ORDER BY R.Fecha
    """, EMPLEADOS[0], EMPLEADOS[1], desde, hasta)

    rows = cursor.fetchall()
    if not rows:
        print("  (sin registros en ese rango)")
        return {}

    puesto_por_empleado = {}
    print(f"  {'Corr':>5}  {'Fecha':^22}  {'Turno':^18}  {'Empl':^6}  {'IDgrupo':^7}  {'PuestoFijo':^10}")
    print(f"  {'-'*5}  {'-'*22}  {'-'*18}  {'-'*6}  {'-'*7}  {'-'*10}")
    for r in rows:
        print(f"  {r.Correlativo:>5}  {str(r.Fecha):^22}  {r.Turno:^18}  "
              f"{r.Codigo_empleado:^6}  {r.IDgrupo:^7}  {str(r.PuestoFijo):^10}")
        puesto_por_empleado[r.Codigo_empleado] = r.PuestoFijo
    return puesto_por_empleado

# ---------------------------------------------------------------------------
# 2. Simulación matemática del bug
# ---------------------------------------------------------------------------
def simular_paridad(sabado: datetime, puestos: dict):
    lunes = sabado + timedelta(days=2)

    print("\n" + "="*70)
    print("ANÁLISIS DE PARIDAD — bug sábado noche → lunes mañana")
    print("="*70)

    for dia, etiqueta in [(sabado, "Sábado noche (23:00)"), (lunes, "Lunes mañana (07:00)")]:
        hora = 23 if dia == sabado else 7
        t    = dia.replace(hour=hora, minute=0)
        dj   = t.timetuple().tm_yday
        par  = (dj + 1) % 2 == 0
        print(f"\n  {etiqueta}  →  {t.strftime('%d-%b-%Y %H:%M')}")
        print(f"    DíaJuliano = {dj}   (dj+1)%2 = {(dj+1)%2}   ¿Par? {par}")
        print(f"    areaPuesto1 = {'Extracción' if par else 'Alcalizado'}")
        for cod, puesto in sorted(puestos.items()):
            sf  = area_sin_fix(t, puesto)
            cf  = area_con_fix(t, puesto)
            print(f"      Empleado {cod} (PuestoFijo={puesto})  "
                  f"sin fix: {sf:12}  con fix: {cf:12}")

    print()
    # Comparación directa sábado vs lunes
    t_sab = sabado.replace(hour=23, minute=0)
    t_lun = lunes.replace(hour=7,  minute=0)
    print("  COMPARACIÓN DIRECTA:")
    print(f"  {'Empleado':^10}  {'Sáb 23:00':^14}  {'Lun 07:00 (sin fix)':^20}  "
          f"{'Lun 07:00 (con fix)':^20}  ¿Fix ayuda?")
    print(f"  {'-'*10}  {'-'*14}  {'-'*20}  {'-'*20}  ----------")
    for cod, puesto in sorted(puestos.items()):
        a_sab     = area_sin_fix(t_sab, puesto)
        a_lun_sf  = area_sin_fix(t_lun, puesto)
        a_lun_cf  = area_con_fix(t_lun, puesto)
        deberia   = "Alcalizado" if a_sab == "Extracción" else "Extracción"
        fix_ayuda = "NO" if a_lun_cf == a_sab else "SÍ"
        marca_sf  = " ← BUG" if a_lun_sf == a_sab else " ✓"
        marca_cf  = " ← BUG" if a_lun_cf == a_sab else " ✓"
        print(f"  {cod:^10}  {a_sab:^14}  {a_lun_sf:^20}{marca_sf}  "
              f"{a_lun_cf:^20}{marca_cf}  {fix_ayuda}")
    print(f"\n  → El fix de diaLaboral NO resuelve este caso porque")
    print(f"    el lunes a las 07:00 (>= 05:41) diaLaboral = lunes (D+2),")
    print(f"    que tiene la MISMA paridad que sábado (D).")
    print(f"    Este es un bug de paridad independiente, no de cruce de medianoche.")

# ---------------------------------------------------------------------------
# 3. Demostrar el fix correcto para este caso
# ---------------------------------------------------------------------------
def simular_fix_correcto(sabado: datetime, puestos: dict):
    """
    Fix propuesto: cuando un empleado genera en turno mañana (>= 05:41)
    y tiene una rutina reciente de hace 2 días (sábado noche),
    usar (diaJuliano - 1) para obtener la paridad correcta (la del domingo).
    """
    lunes = sabado + timedelta(days=2)
    t_sab = sabado.replace(hour=23, minute=0)
    t_lun = lunes.replace(hour=7, minute=0)

    print("\n" + "="*70)
    print("FIX PROPUESTO: ajustar paridad cuando hay gap de un día (día de descanso)")
    print("="*70)
    print()
    print("  Lógica del fix adicional:")
    print("  Si la última rutina del empleado fue hace exactamente 2 días naturales")
    print("  (sábado noche → lunes mañana), restar 1 al diaJuliano del lunes")
    print("  para usar la paridad del domingo (D+1) en lugar del lunes (D+2).")
    print()

    dj_sab = t_sab.timetuple().tm_yday
    dj_lun = t_lun.timetuple().tm_yday
    dj_lun_ajustado = dj_lun - 1  # paridad del domingo

    print(f"  Sábado diaJuliano = {dj_sab}  → paridad {(dj_sab+1)%2}")
    print(f"  Lunes  diaJuliano = {dj_lun}  → paridad {(dj_lun+1)%2}  (igual que sábado: BUG)")
    print(f"  Lunes  diaJuliano - 1 = {dj_lun_ajustado} → paridad {(dj_lun_ajustado+1)%2}  (opuesto: CORRECTO)")
    print()

    print(f"  {'Empleado':^10}  {'Sáb 23:00':^14}  {'Lun 07:00 actual':^18}  "
          f"{'Lun 07:00 con fix':^18}  ¿Correcto?")
    print(f"  {'-'*10}  {'-'*14}  {'-'*18}  {'-'*18}  ----------")
    for cod, puesto in sorted(puestos.items()):
        a_sab  = area_sin_fix(t_sab, puesto)
        a_actual = area_sin_fix(t_lun, puesto)

        # Simular el fix: usar dj-1
        area1_adj = "Extracción" if (dj_lun_ajustado + 1) % 2 == 0 else "Alcalizado"
        area2_adj = "Alcalizado" if area1_adj == "Extracción" else "Extracción"
        a_fix  = area1_adj if puesto == 1 else area2_adj

        correcto = "✓ SÍ" if a_fix != a_sab else "✗ NO"
        print(f"  {cod:^10}  {a_sab:^14}  {a_actual:^18}  {a_fix:^18}  {correcto}")

# ---------------------------------------------------------------------------
# 4. Tabla de paridades para los próximos días
# ---------------------------------------------------------------------------
def tabla_paridades(desde: datetime, dias: int = 10):
    print("\n" + "="*70)
    print(f"PARIDADES — {dias} días desde {desde.strftime('%d-%b-%Y')}")
    print("="*70)
    print(f"  {'Fecha':^14}  {'DíaJuliano':^10}  {'(dj+1)%2':^8}  {'areaPuesto1':^14}  Turno esperado")
    print(f"  {'-'*14}  {'-'*10}  {'-'*8}  {'-'*14}  ---")
    for i in range(dias):
        d   = desde + timedelta(days=i)
        dj  = d.timetuple().tm_yday
        par = (dj + 1) % 2
        a1  = "Extracción" if par == 0 else "Alcalizado"
        marca = " ← mismo que Sáb" if (dj - (desde - timedelta(days=2)).timetuple().tm_yday) % 2 == 0 else ""
        print(f"  {d.strftime('%a %d-%b-%Y'):^14}  {dj:^10}  {par:^8}  {a1:^14}{marca}")

# ---------------------------------------------------------------------------
# 5. Verificar semana completa de turno mañana con el fix implementado
# ---------------------------------------------------------------------------
def simular_semana_maniana(sabado: datetime, puestos: dict):
    """
    Muestra el área asignada cada día de la semana de turno mañana (Lun–Sáb)
    a las 07:00, comparando sin fix vs fix completo (diaLaboral + dj-1).
    La noche del sábado se incluye como referencia para ver la alternación.
    """
    print("\n" + "="*70)
    print("SEMANA COMPLETA — turno mañana con fix implementado (dj-1)")
    print("="*70)
    print("  Referencia: sábado noche 23:00 y luego lun–sáb siguiente a 07:00")
    print()
    print(f"  {'Día / Hora':^22}  {'Sin fix':^14}  {'Fix completo':^14}  Nota")
    print(f"  {'-'*22}  {'-'*14}  {'-'*14}  ----")

    # Sábado noche como referencia
    t_ref = sabado.replace(hour=23, minute=0)
    for cod, puesto in sorted(puestos.items()):
        sf = area_sin_fix(t_ref, puesto)
        fc = area_fix_completo(t_ref, puesto)
        print(f"  {t_ref.strftime('%a %d-%b %H:%M'):^22}  {sf:^14}  {fc:^14}  (noche referencia, empl {cod})")

    print()
    # Lunes a sábado siguiente a 07:00
    for offset in range(2, 8):  # lunes=+2, martes=+3, … sábado=+7
        d = sabado + timedelta(days=offset)
        t = d.replace(hour=7, minute=0)
        primera = True
        for cod, puesto in sorted(puestos.items()):
            sf = area_sin_fix(t, puesto)
            fc = area_fix_completo(t, puesto)
            # detectar si el area es la misma que la noche de sábado del empleado
            sf_ref = area_sin_fix(t_ref, puesto)
            nota_sf = " ← BUG (igual que sáb)" if sf == sf_ref else " ✓ alterna"
            nota_fc = " ← BUG" if fc == sf_ref else " ✓ alterna"
            label = t.strftime('%a %d-%b %H:%M') if primera else " "*22
            print(f"  {label:^22}  {sf:^14}  {fc:^14}  empl {cod}: sin={sf}{nota_sf} | fix={fc}{nota_fc}")
            primera = False
        print()

# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    conn = conectar()

    # Fechas del caso reportado: sábado 04-Abr, lunes 06-Abr
    sabado = datetime(2026, 4, 4)
    lunes  = sabado + timedelta(days=2)

    puestos = {}
    if conn:
        cursor = conn.cursor()
        puestos = registros_empleados(
            cursor,
            desde=sabado.replace(hour=0),
            hasta=lunes.replace(hour=23, minute=59)
        )
        cursor.close()
        conn.close()

    # Si no hay conexión o no se encontraron registros, usar valores reportados
    # (837 → PuestoFijo desconocido, asumir por lógica del reporte)
    if not puestos:
        print("\n  [Sin conexión BD — usando PuestoFijo inferidos del reporte del usuario]")
        # Sábado: 837=G2(Alcalizado), 15997=G1(Extracción)
        # dj(Sab=Apr4)=94 → (95)%2=1 → areaPuesto1=Alcalizado
        # PuestoFijo=1 → Alcalizado → 837 tiene PuestoFijo=1
        # PuestoFijo=2 → Extracción → 15997 tiene PuestoFijo=2
        puestos = {"837": 1, "15997": 2}

    simular_paridad(sabado, puestos)
    simular_fix_correcto(sabado, puestos)
    simular_semana_maniana(sabado, puestos)
    tabla_paridades(sabado - timedelta(days=1), dias=8)

    print("\n" + "="*70)
    print("CONCLUSIÓN")
    print("="*70)
    print("  FIX v1 (diaLaboral): resuelve el flip de medianoche, pero NO corrige sáb→lun.")
    print("  Causa raíz: sábado (D) y lunes (D+2) tienen IGUAL paridad (dj%2 == 0).")
    print()
    print("  FIX v2 IMPLEMENTADO en Generadorrutinas.aspx.cs:")
    print("    bool esTurnoManiana = (día == Domingo)")
    print("        ? horaActual ∈ [05:41, 17:40]")
    print("        : horaActual ∈ [05:41, 13:40];")
    print("    if (esTurnoManiana) diaJuliano -= 1;")
    print()
    print("  → Ancla la semana de mañana al domingo intermedio (D+1) cuya paridad es")
    print("    OPUESTA a sábado (D) y lunes (D+2). La rotación es correcta toda la semana.")
    print("  → Turno tarde y noche NO se ven afectados (sus horas quedan fuera del rango).")
    print()
    input("Presiona Enter para cerrar...")
