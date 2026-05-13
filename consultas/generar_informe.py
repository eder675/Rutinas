import pyodbc
from datetime import datetime

conn = pyodbc.connect(
    'DRIVER={SQL Server};SERVER=10.1.1.43\\SQLEXPRESS;DATABASE=Vinetas;UID=su;PWD=su;TrustServerCertificate=yes'
)
cur = conn.cursor()

# Correcciones realizadas (area_original -> area_corregida)
correcciones = [
    ('Jugo - Calentador #2',                'Calentadores de Jugo - Calentador #2'),
    ('a de Alcohol - Dilucion',             'Planta de Alcohol - Dilucion'),
    ('acion Electrica - TurboGenerador NG', 'Generacion Electrica - TurboGenerador NG'),
    ('alizadores',                          'Cristalizadores'),
    ('alizadores - Cristalizador Vertical', 'Cristalizadores - Cristalizador Vertical'),
    ('alizadores - Cristalizadora #2',      'Cristalizadores - Cristalizadora #2'),
    ('ccion',                               'Extraccion'),
    ('ccion - Agua Imbibicion',             'Extraccion - Agua Imbibicion'),
    ('ccion - Conductor Principal',         'Extraccion - Conductor Principal'),
    ('ccion - Desfibradora',                'Extraccion - Desfibradora'),
    ('ccion - Molino #5',                   'Extraccion - Molino #5'),
    ('ccion - Picadora',                    'Extraccion - Picadora'),
    ('ccion - PrePicadora',                 'Extraccion - PrePicadora'),
    ('Clarificador de Meladura',            'Clarificacion - Clarificador de Meladura'),
    ('Claro',                               'Clarificacion- Jugo Claro'),
    ('Electrica Matizate - Generador TGM',  'TermoElectrica Matizate - Generador TGM'),
    ('ficacion - Clarificador de Meladura', 'Clarificacion - Clarificador de Meladura'),
    ('ficacion - Clarificador Rapido',      'Clarificacion - Clarificador Rapido'),
    ('ficacion - Filtro de Banda',          'Clarificacion - Filtro de Banda'),
    ('ficacion - Jugo Filtrado',            'Clarificacion - Jugo Filtrado'),
    ('ficacion - Recalentadores de Jugo',   'Clarificacion - Recalentadores de Jugo'),
    ('ifugas - Centrifugas de Primera',     'Centrifugas - Centrifugas de Primera'),
    ('ion - Clarificador de Meladura',      'Clarificacion - Clarificador de Meladura'),
    ('ion - Clarificador Rapido',           'Clarificacion - Clarificador Rapido'),
    ('ion - Filtro de Banda',               'Clarificacion - Filtro de Banda'),
    ('izacion - Jugo Pre-Alcalizado',       'Alcalizacion - Jugo Pre-Alcalizado'),
    ('izacion - Preparacion de Sacarato',   'Alcalizacion - Preparacion de Sacarato'),
    ('on - Preparacion de Sacarato',        'Alcalizacion - Preparacion de Sacarato'),
    ('ora - Azucar Blanca',                 'Secadora - Azucar Blanca'),
    ('ora - Empaqsa',                       'Secadora - Empaqsa'),
    ('Planta de tratamiento de agua',       'Calderas -Planta de tratamiento de agua'),
    ('ra Combustion Engineering #2',        'Caldera Combustion Engineering #2'),
    ('racion',                              'Evaporacion'),
    ('racion - Melador #3',                 'Evaporacion - Melador #3'),
    ('racion - Pre-Evaporador #1',          'Evaporacion - Pre-Evaporador #1'),
    ('ras - Caldera Mitre',                 'Calderas - Caldera Mitre'),
    ('ras -Planta de tratamiento de agua',  'Calderas -Planta de tratamiento de agua'),
    ('rica Matizate - Generador TGM',       'TermoElectrica Matizate - Generador TGM'),
    ('s - Centrifugas de Segunda',          'Centrifugas - Centrifugas de Segunda'),
    ('s - Disolutores',                     'Tachos - Disolutores'),
    ('s - Tacho #1',                        'Tachos - Tacho #1'),
    ('s - Tacho #2',                        'Tachos - Tacho #2'),
    ('s - Tacho #3',                        'Tachos - Tacho #3'),
    ('s - Tacho #4',                        'Tachos - Tacho #4'),
    ('s - Tacho #6',                        'Tachos - Tacho #6'),
    ('s - Tacho #7',                        'Tachos - Tacho #7'),
    ('cion',                                'Planta Osmosis'),
    ('ras',                                 'Calderas'),
    ('ra Mitre - Tratamiento de Agua',      'Caldera Mitre - Tratamiento de Agua'),
    ('racion - Evaporador #8',              'Evaporacion'),
    ('radores',                             'Evaporacion'),
    ('iste',                                'No Existe'),
    ('v',                                   'No Existe'),
    ('a Alcohol - Tanque Alcohol Final #3', 'Planta de Alcohol - Tanque Alcohol Final #3'),
    ('ifugas - Centrifuga 1a',              'Centrifugas - Centrifugas de Primera'),
    ('ifugas - Centrifugas 1a',             'Centrifugas - Centrifugas de Primera'),
]

correcciones_tag = [
    ('COMP-0700',   's',        'Cristalizadores'),
    ('FVVGCV-0600', 's',        'Tachos'),
    ('LMCLT-0600',  's',        'Tachos'),
    ('LWT10T-0600', 's',        'Tachos'),
    ('PCV-0209',    'ra Mitre', 'Caldera Mitre'),
    ('FMFV-0800',   'ifugas',   'Centrifugas'),
    ('LWCT-0501',   'es',       'Evaporacion - Pre-Evaporador #1'),
    ('LAZT-0900',   'ora',      'Secadora'),
    ('LWRT-1011',   'ca',       'Evaporacion'),
    ('COMP-0500',   'n',        'Evaporacion'),
]

lines = []
sep  = '=' * 80
sep2 = '-' * 80

lines.append(sep)
lines.append('INFORME DE CORRECCIONES DE AREAS - BASE DE DATOS VINETAS')
lines.append('Ingenio La Cabana - Departamento MTI')
lines.append('Generado: ' + datetime.now().strftime('%d/%m/%Y %H:%M'))
lines.append(sep)
lines.append('')
lines.append('DESCRIPCION: Se corrigieron nombres de areas truncados o erroneos en la')
lines.append('tabla [Vinetas].[dbo].[equipos]. El campo Area tenia valores con los')
lines.append('primeros caracteres faltantes. A continuacion se listan los instrumentos')
lines.append('afectados por area corregida.')
lines.append('')

# Agrupar correcciones por area_corregida
areas_mapa = {}
for orig, corr in correcciones:
    if corr not in areas_mapa:
        areas_mapa[corr] = []
    areas_mapa[corr].append(orig)

total_inst = 0
seccion = 1

for area_corr in sorted(areas_mapa.keys()):
    origenes = areas_mapa[area_corr]
    cur.execute('SELECT RTRIM(TAG) as TAG, RTRIM(Descripcion) as Descripcion FROM equipos WHERE RTRIM(Area)=? ORDER BY TAG', area_corr)
    equipos = cur.fetchall()

    lines.append(sep2)
    lines.append(f'{seccion}. AREA CORREGIDA: {area_corr}')
    lines.append(f'   Valores originales erroneos: {", ".join([repr(o) for o in origenes])}')
    lines.append(f'   Instrumentos en esta area actualmente: {len(equipos)}')
    lines.append('')
    if equipos:
        lines.append(f'   {"TAG":<22} {"DESCRIPCION"}')
        lines.append(f'   {"-"*20} {"-"*50}')
        for eq in equipos:
            lines.append(f'   {eq.TAG:<22} {eq.Descripcion}')
    else:
        lines.append('   (Sin instrumentos registrados actualmente)')
    lines.append('')
    total_inst += len(equipos)
    seccion += 1

lines.append(sep)
lines.append('CORRECCIONES INDIVIDUALES POR TAG')
lines.append(sep)
lines.append('')
lines.append(f'   {"TAG":<22} {"AREA ORIGINAL":<20} {"AREA CORREGIDA":<45} {"DESCRIPCION"}')
lines.append(f'   {"-"*20} {"-"*18} {"-"*43} {"-"*40}')
for tag, orig, corr in correcciones_tag:
    cur.execute('SELECT RTRIM(Descripcion) FROM equipos WHERE RTRIM(TAG)=?', tag)
    row = cur.fetchone()
    desc = row[0].strip() if row else '(no encontrado)'
    lines.append(f'   {tag:<22} {repr(orig):<20} {corr:<45} {desc}')

lines.append('')
lines.append(sep)
lines.append(f'RESUMEN: {total_inst} instrumentos en areas corregidas por patron truncado.')
lines.append(f'         {len(correcciones_tag)} instrumentos corregidos individualmente por TAG.')
lines.append(f'         TOTAL AFECTADOS: {total_inst + len(correcciones_tag)} registros.')
lines.append(sep)

report = '\n'.join(lines)
path = 'C:/Users/WL/source/repos/eder675/Rutinas/consultas/informe_correcciones_areas.txt'
with open(path, 'w', encoding='utf-8') as f:
    f.write(report)

print('Informe generado:', path)
print(f'Total instrumentos en areas corregidas: {total_inst}')
conn.close()
