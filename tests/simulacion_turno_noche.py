"""
Simulación: bug de cambio de área al cruzar medianoche en turno noche
Proyecto: Rutinas — Ingenio La Cabaña
No requiere conexión a BD (puramente matemática).

El turno noche corre de 22:00 a 06:00 abarcando dos días calendario.
La lógica actual usa DateTime.Now.DayOfYear, que cambia a las 00:00 e
invierte la paridad del día → el área asignada flipa al cruzar medianoche.
"""

from datetime import datetime, timedelta

FECHA_INICIO_ZAFRA = datetime(2025, 11, 25)

# ---------------------------------------------------------------------------
# Réplica del algoritmo ACTUAL (con el bug)
# ---------------------------------------------------------------------------
def dia_juliano_actual(ahora: datetime) -> int:
    return ahora.timetuple().tm_yday

def area_puesto1_actual(ahora: datetime) -> str:
    dj = dia_juliano_actual(ahora)
    return "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"

def obtener_area_actual(ahora: datetime, puesto_fijo: int) -> str:
    """Replica de ObtenerAreaDeterminista() con el bug."""
    dj = ahora.timetuple().tm_yday
    area1 = "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"
    area2 = "Alcalizado" if area1 == "Extracción" else "Extracción"
    return area1 if puesto_fijo == 1 else area2

# ---------------------------------------------------------------------------
# Réplica del algoritmo CORREGIDO
# ---------------------------------------------------------------------------
def dia_laboral(ahora: datetime) -> datetime:
    """
    Para el turno noche (00:00–05:40) el turno empezó el día anterior.
    Se retrocede un día para que toda la jornada use el mismo día de referencia.
    """
    if ahora.hour < 5 or (ahora.hour == 5 and ahora.minute < 41):
        return ahora - timedelta(days=1)
    return ahora

def obtener_area_corregido(ahora: datetime, puesto_fijo: int) -> str:
    """Replica de ObtenerAreaDeterminista() CON la corrección."""
    dl = dia_laboral(ahora)
    dj = dl.timetuple().tm_yday
    area1 = "Extracción" if (dj + 1) % 2 == 0 else "Alcalizado"
    area2 = "Alcalizado" if area1 == "Extracción" else "Extracción"
    return area1 if puesto_fijo == 1 else area2

# ---------------------------------------------------------------------------
# Simulación 1: comportamiento hora a hora a lo largo del turno noche
# ---------------------------------------------------------------------------
def simular_turno_noche(noche_inicio: datetime, puesto_fijo: int = 1):
    """
    Muestra el área asignada cada 30 min durante un turno noche completo,
    comparando el algoritmo actual (con bug) vs el corregido.
    """
    print("\n" + "="*72)
    print(f"TURNO NOCHE — inicio {noche_inicio.strftime('%A %d-%b-%Y')} 22:00")
    print(f"PuestoFijo = {puesto_fijo}")
    print("="*72)
    print(f"  {'Hora':^20}  {'DíaJuliano':^10}  {'ACTUAL':^12}  {'CORREGIDO':^12}  Estado")
    print(f"  {'-'*20}  {'-'*10}  {'-'*12}  {'-'*12}  ------")

    # Iterar desde 22:00 del día de inicio hasta 06:00 del día siguiente
    t = noche_inicio.replace(hour=22, minute=0, second=0)
    fin = (noche_inicio + timedelta(days=1)).replace(hour=6, minute=0)

    area_referencia = None  # área que se asignó al generar (22:00)
    while t <= fin:
        actual    = obtener_area_actual(t, puesto_fijo)
        corregido = obtener_area_corregido(t, puesto_fijo)
        dj        = t.timetuple().tm_yday

        if t.replace(hour=22, minute=0) == noche_inicio.replace(hour=22, minute=0) and t.hour == 22 and t.minute == 0:
            area_referencia = actual

        cambio_bug = " *** FLIP (bug)" if (area_referencia and actual != area_referencia) else ""
        print(f"  {t.strftime('%a %d-%b %H:%M'):^20}  {dj:^10}  {actual:^12}  {corregido:^12}{cambio_bug}")

        t += timedelta(minutes=30)

# ---------------------------------------------------------------------------
# Simulación 2: comparar múltiples noches consecutivas
# ---------------------------------------------------------------------------
def simular_semana(fecha_inicio: datetime, dias: int = 7):
    print("\n" + "="*72)
    print(f"RESUMEN SEMANAL — área asignada a las 23:00 vs 01:00 (siguiente día)")
    print(f"PuestoFijo=1 | {dias} noches desde {fecha_inicio.strftime('%d-%b-%Y')}")
    print("="*72)
    print(f"  {'Noche':^20}  {'23:00 actual':^13}  {'01:00 actual':^13}  "
          f"{'23:00 fix':^13}  {'01:00 fix':^13}  ¿Bug?")
    print(f"  {'-'*20}  {'-'*13}  {'-'*13}  {'-'*13}  {'-'*13}  -----")

    for i in range(dias):
        noche = fecha_inicio + timedelta(days=i)
        t2300 = noche.replace(hour=23, minute=0)
        t0100 = (noche + timedelta(days=1)).replace(hour=1, minute=0)

        a2300 = obtener_area_actual(t2300, 1)
        a0100 = obtener_area_actual(t0100, 1)
        f2300 = obtener_area_corregido(t2300, 1)
        f0100 = obtener_area_corregido(t0100, 1)

        bug = "SÍ <<<" if a2300 != a0100 else "no"
        print(f"  {noche.strftime('%a %d-%b-%Y'):^20}  {a2300:^13}  {a0100:^13}  "
              f"{f2300:^13}  {f0100:^13}  {bug}")

# ---------------------------------------------------------------------------
# Simulación 3: verificar que el área es CONSISTENTE con el compañero de turno
# ---------------------------------------------------------------------------
def simular_pareja(noche_inicio: datetime):
    """
    Replica el caso real: un empleado (P1) imprime a las 23:00 y el otro (P2)
    imprime a las 01:00 del día siguiente. ¿Reciben áreas opuestas en ambos casos?
    """
    print("\n" + "="*72)
    print(f"CONSISTENCIA DE PAREJA — noche {noche_inicio.strftime('%A %d-%b-%Y')}")
    print("="*72)

    horas_prueba = [
        noche_inicio.replace(hour=22, minute=30),
        noche_inicio.replace(hour=23, minute=0),
        noche_inicio.replace(hour=23, minute=59),
        (noche_inicio + timedelta(days=1)).replace(hour=0, minute=1),
        (noche_inicio + timedelta(days=1)).replace(hour=1, minute=0),
        (noche_inicio + timedelta(days=1)).replace(hour=3, minute=0),
        (noche_inicio + timedelta(days=1)).replace(hour=5, minute=30),
        (noche_inicio + timedelta(days=1)).replace(hour=5, minute=45),
    ]

    print(f"  {'Hora impresión':^20}  {'P1 actual':^10}  {'P2 actual':^10}  "
          f"{'P1 fix':^10}  {'P2 fix':^10}  ¿Opuestos fix?")
    print(f"  {'-'*20}  {'-'*10}  {'-'*10}  {'-'*10}  {'-'*10}  -------------")

    for t in horas_prueba:
        p1_act = obtener_area_actual(t, 1)
        p2_act = obtener_area_actual(t, 2)
        p1_fix = obtener_area_corregido(t, 1)
        p2_fix = obtener_area_corregido(t, 2)
        opuestos = "✓ OK" if p1_fix != p2_fix else "✗ MISMO ÁREA"
        bug_str  = " (BUG!)" if p1_act == p2_act else ""
        print(f"  {t.strftime('%a %d-%b %H:%M'):^20}  {p1_act:^10}  {p2_act:^10}  "
              f"{p1_fix:^10}  {p2_fix:^10}  {opuestos}{bug_str}")

# ---------------------------------------------------------------------------
# Simulación 4: caso de Reimprimir después de medianoche
# ---------------------------------------------------------------------------
def simular_reimprimir(noche_inicio: datetime):
    """
    Replica el flujo: genera a las 22:30, reimprime a las 00:30 y a las 05:00.
    Con el bug, el área de obligatorios cambia. Con el fix, se mantiene igual.
    """
    print("\n" + "="*72)
    print(f"FLUJO REIMPRIMIR — noche {noche_inicio.strftime('%A %d-%b-%Y')}")
    print("="*72)

    eventos = [
        (noche_inicio.replace(hour=22, minute=30), "Generación original"),
        (noche_inicio.replace(hour=23, minute=45), "Reimpresión (antes midnight)"),
        ((noche_inicio + timedelta(days=1)).replace(hour=0, minute=1),  "Reimpresión (1 min después midnight)"),
        ((noche_inicio + timedelta(days=1)).replace(hour=0, minute=30), "Reimpresión (00:30)"),
        ((noche_inicio + timedelta(days=1)).replace(hour=3, minute=0),  "Reimpresión (03:00)"),
    ]

    area_generada_actual   = obtener_area_actual(eventos[0][0], 1)
    area_generada_corregida = obtener_area_corregido(eventos[0][0], 1)

    for t, descripcion in eventos:
        act = obtener_area_actual(t, 1)
        fix = obtener_area_corregido(t, 1)
        bug = " *** ÁREA DIFERENTE (bug)" if act != area_generada_actual else ""
        ok  = " ✓" if fix == area_generada_corregida else " ✗"
        print(f"  {t.strftime('%a %d-%b %H:%M')}  [{descripcion}]")
        print(f"    Actual   → {act}{bug}")
        print(f"    Corregido→ {fix}{ok}")
        print()

# ---------------------------------------------------------------------------
# MAIN
# ---------------------------------------------------------------------------
if __name__ == "__main__":
    # Noche del lunes 6 de abril (turno 22:00 Lun → 06:00 Mar) — próxima noche
    noche_prueba = datetime(2026, 4, 6)

    simular_turno_noche(noche_prueba, puesto_fijo=1)
    simular_semana(datetime(2026, 4, 1), dias=10)
    simular_pareja(noche_prueba)
    simular_reimprimir(noche_prueba)

    print("\n" + "="*72)
    print("CONCLUSIÓN")
    print("="*72)
    print("  BUG:  int diaJuliano = ahora.DayOfYear;")
    print("        → cambia al cruzar 00:00, invierte paridad, flipa el área.")
    print()
    print("  FIX:  DateTime diaLaboral = (hora < 05:41) ? ahora.AddDays(-1) : ahora;")
    print("        int diaJuliano = diaLaboral.DayOfYear;")
    print("        → ancla toda la jornada nocturna al día en que inició el turno.")
    print()
    input("Presiona Enter para cerrar...")
