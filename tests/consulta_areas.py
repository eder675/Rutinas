"""
Diagnostico de distribucion de areas por turno
Proyecto: Rutinas - Ingenio La Cabaña
Requiere: pip install pyodbc
"""

import pyodbc
from datetime import datetime

# --- Conexion (ConexionRutinasMTI activa en Web.config) ---
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

def conectar():
    try:
        return pyodbc.connect(conn_str)
    except Exception as e:
        print(f"[ERROR] No se pudo conectar: {e}")
        return None

def mostrar_grupos(cursor):
    print("\n" + "="*60)
    print("TABLA: Rotaciongrupos")
    print("="*60)
    cursor.execute("SELECT IDgrupo, Nombregrupo FROM Rotaciongrupos ORDER BY IDgrupo")
    rows = cursor.fetchall()
    for r in rows:
        print(f"  IDgrupo={r[0]}  Nombre={r[1]}")

def mostrar_areas(cursor):
    print("\n" + "="*60)
    print("TABLA: Area (todas, en orden geografico IDarea ASC)")
    print("="*60)
    cursor.execute("""
        SELECT A.IDarea, A.Nombre, A.IDgrupo, G.Nombregrupo
        FROM Area A
        INNER JOIN Rotaciongrupos G ON A.IDgrupo = G.IDgrupo
        ORDER BY A.IDgrupo, A.IDarea
    """)
    rows = cursor.fetchall()
    grupo_actual = None
    indice = 0
    for r in rows:
        if r[2] != grupo_actual:
            grupo_actual = r[2]
            indice = 0
            print(f"\n  --- Grupo {r[2]}: {r[3]} ---")
        print(f"  [{indice:02d}] IDarea={r[0]:3d}  Nombre={r[1]}")
        indice += 1
    return rows

def simular_distribucion(cursor):
    print("\n" + "="*60)
    print("SIMULACION: Distribucion de areas por turno")
    print("="*60)

    cursor.execute("SELECT DISTINCT IDgrupo FROM Area ORDER BY IDgrupo")
    grupos = [r[0] for r in cursor.fetchall()]

    for id_grupo in grupos:
        cursor.execute("""
            SELECT IDarea FROM Area WHERE IDgrupo = ? ORDER BY IDarea ASC
        """, id_grupo)
        areas = [r[0] for r in cursor.fetchall()]
        total = len(areas)
        por_turno = total // 3

        cursor.execute("SELECT Nombregrupo FROM Rotaciongrupos WHERE IDgrupo = ?", id_grupo)
        nombre_grupo = cursor.fetchone()[0]

        print(f"\n  Grupo {id_grupo} - {nombre_grupo}")
        print(f"  Total areas: {total}  |  Por turno (//3): {por_turno}")
        print(f"  IDs en orden: {areas}")

        for bloque, nombre_turno in enumerate(["Mañana (bloque 0)", "Tarde  (bloque 1)", "Noche  (bloque 2)"]):
            inicio = bloque * por_turno
            if bloque == 2:
                fin = total
            else:
                fin = inicio + por_turno

            sub = areas[inicio:fin]
            print(f"    {nombre_turno}: indices [{inicio}:{fin}] -> IDareas={sub}  ({len(sub)} areas)")

        # Verificar si hay areas sin cubrir o solapadas
        cubierto = set()
        for bloque in range(3):
            inicio = bloque * por_turno
            fin = total if bloque == 2 else inicio + por_turno
            for a in areas[inicio:fin]:
                cubierto.add(a)

        sin_cubrir = set(areas) - cubierto
        if sin_cubrir:
            print(f"    [ADVERTENCIA] Areas SIN cubrir: {sin_cubrir}")
        else:
            print(f"    OK: Todas las {total} areas son cubiertas sin repeticion.")

def detectar_turno_actual():
    """
    Replica exacta de DeterminarTurnoActual() del C#
    para verificar que indiceBloque es correcto a la hora actual.
    """
    print("\n" + "="*60)
    print("DETECCION DEL TURNO ACTUAL")
    print("="*60)
    ahora = datetime.now()
    hora = ahora.hour + ahora.minute / 60.0
    print(f"  Hora actual: {ahora.strftime('%H:%M:%S')}")

    # Replica de DeterminarTurnoActual
    if ahora.weekday() == 6:  # Domingo
        if 5 + 41/60 <= hora <= 17 + 40/60:
            turno = "06:00 A 18:00"
        elif hora >= 17 + 41/60:
            turno = "18:00 A 22:00 / 18:00 A 06:00 (domingo noche)"
        else:
            turno = "22:00 - 06:00"
    elif 5 + 41/60 <= hora <= 13 + 40/60:
        turno = "06:00 A 14:00"
    elif 13 + 41/60 <= hora <= 21 + 40/60:
        turno = "14:00 A 22:00"
    else:
        turno = "22:00 - 06:00"

    print(f"  Turno detectado: {turno}")

    # Replica de la deteccion de bloque CORREGIDA
    if "A 22:00" in turno:
        bloque = 1
    elif "22:00" in turno or "A 06:00" in turno:
        bloque = 2
    else:
        bloque = 0

    nombres = {0: "Mañana", 1: "Tarde", 2: "Noche"}
    print(f"  indiceBloque asignado: {bloque} ({nombres[bloque]})")

    # Deteccion ANTIGUA (con el bug) para comparacion
    if "14:00" in turno or "18:00 A 22:00" in turno:
        bloque_viejo = 1
    elif "22:00" in turno or "18:00 A 06:00" in turno:
        bloque_viejo = 2
    else:
        bloque_viejo = 0

    if bloque != bloque_viejo:
        print(f"  [CAMBIO] Deteccion anterior (con bug): bloque={bloque_viejo} ({nombres[bloque_viejo]})")
        print(f"           Deteccion nueva (corregida):  bloque={bloque} ({nombres[bloque]})")
    else:
        print(f"  Sin diferencia entre deteccion antigua y nueva para este horario.")

    return bloque

def instrumentos_por_area(cursor):
    print("\n" + "="*60)
    print("INSTRUMENTOS POR AREA (cantidad por prioridad)")
    print("="*60)
    cursor.execute("""
        SELECT
            A.IDgrupo,
            G.Nombregrupo,
            base.IDarea,
            A.Nombre AS NombreArea,
            base.Prioridad,
            base.Total,
            base.Disponibles_24h
        FROM (
            SELECT
                I.IDarea,
                P.Nombre AS Prioridad,
                COUNT(*) AS Total,
                COUNT(CASE WHEN usados.TAG IS NULL THEN 1 END) AS Disponibles_24h
            FROM Instrumentos I
            INNER JOIN Prioridad P ON I.IDprioridad = P.IDprioridad
            LEFT JOIN (
                SELECT DISTINCT RI.TAG
                FROM Rutinas R
                INNER JOIN Rutina_instrumento RI ON R.Correlativo = RI.Correlativo
                WHERE R.Fecha >= DATEADD(hour, -24, GETDATE())
            ) usados ON I.TAG = usados.TAG
            GROUP BY I.IDarea, P.Nombre
        ) base
        INNER JOIN Area A ON base.IDarea = A.IDarea
        INNER JOIN Rotaciongrupos G ON A.IDgrupo = G.IDgrupo
        ORDER BY A.IDgrupo, base.IDarea, base.Prioridad
    """)
    rows = cursor.fetchall()
    grupo_actual = None
    area_actual = None
    for r in rows:
        if r[0] != grupo_actual:
            grupo_actual = r[0]
            print(f"\n  Grupo {r[0]}: {r[1]}")
        if r[2] != area_actual:
            area_actual = r[2]
            print(f"    IDarea={r[2]:3d} {r[3]}")
        disponibles = r[6]
        alerta = " <<< SIN DISPONIBLES" if disponibles == 0 else ""
        print(f"      {r[4]:6s}: Total={r[5]:3d}  Disponibles(24h)={disponibles:3d}{alerta}")

def bloques_con_problema(cursor):
    print("\n" + "="*60)
    print("RESUMEN: Areas SIN instrumentos disponibles por turno")
    print("="*60)
    cursor.execute("SELECT IDgrupo FROM Rotaciongrupos ORDER BY IDgrupo")
    grupos = [r[0] for r in cursor.fetchall()]

    for id_grupo in grupos:
        cursor.execute("SELECT IDarea FROM Area WHERE IDgrupo=? ORDER BY IDarea", id_grupo)
        areas = [r[0] for r in cursor.fetchall()]
        total = len(areas)
        por_turno = total // 3

        for bloque, nombre in enumerate(["Mañana", "Tarde ", "Noche "]):
            inicio = bloque * por_turno
            fin = total if bloque == 2 else inicio + por_turno
            sub = areas[inicio:fin]

            areas_con_problema = []
            for id_area in sub:
                for prior in ["Alta", "Media", "Baja"]:
                    cursor.execute("""
                        SELECT COUNT(*) FROM Instrumentos I
                        INNER JOIN Prioridad P ON I.IDprioridad = P.IDprioridad
                        WHERE I.IDarea = ?
                          AND P.Nombre = ?
                          AND I.TAG NOT IN (
                              SELECT RI.TAG FROM Rutinas R
                              INNER JOIN Rutina_instrumento RI ON R.Correlativo=RI.Correlativo
                              WHERE R.Fecha >= DATEADD(hour,-24,GETDATE())
                          )
                    """, id_area, prior)
                    count = cursor.fetchone()[0]
                    if count == 0:
                        areas_con_problema.append(f"IDarea={id_area}({prior})")

            estado = "OK" if not areas_con_problema else f"PROBLEMA en: {', '.join(areas_con_problema)}"
            print(f"  Grupo{id_grupo} {nombre} (bloque {bloque}) IDareas={sub}: {estado}")


if __name__ == "__main__":
    print("Conectando a la base de datos...")
    conn = conectar()
    if conn is None:
        exit(1)

    cursor = conn.cursor()
    mostrar_grupos(cursor)
    mostrar_areas(cursor)
    simular_distribucion(cursor)
    detectar_turno_actual()
    instrumentos_por_area(cursor)
    bloques_con_problema(cursor)

    cursor.close()
    conn.close()
    print("\nDone.")

os.system("pause")