using Rutinas.Models; //para usar los modelos de datos en la carpeta Models
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Generadorrutinas : System.Web.UI.Page
    {
        private readonly DateTime FECHA_INICIO_ZAFRA = new DateTime(2025, 11, 25);
        
        private readonly string ConnString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        // ─── CONFIGURACIÓN DE GENERACIÓN ─────────────────────────────────────────
        // Cantidad de instrumentos por prioridad, por área asignada al turno.
        // Modificar estos valores cambia cuántos equipos se incluyen en la rutina.
        private const int CANT_ALTA_G1  = 1; // Alta prioridad  — Grupo 1 (Extracción)
        private const int CANT_MEDIA_G1 = 1; // Media prioridad — Grupo 1 (Extracción)
        private const int CANT_BAJA_G1  = 1; // Baja prioridad  — Grupo 1 (Extracción)

        private const int CANT_ALTA_G2  = 1; // Alta prioridad  — Grupo 2 (Alcalizado)
        private const int CANT_MEDIA_G2 = 1; // Media prioridad — Grupo 2 (Alcalizado)
        private const int CANT_BAJA_G2  = 2; // Baja prioridad  — Grupo 2 (Alcalizado)

        // Equipos en la tabla de comprobaciones obligatorias, indexados por turno:
        //   [0] = Mañana  |  [1] = Tarde  |  [2] = Noche
        private static readonly int[] CANT_OBL_PH_FIJO_G1      = { 1, 1, 1 }; // pH fijo (Transmisor Alcalizado E+H)    — Extracción
        private static readonly int[] CANT_OBL_PH_ALEATORIO_G1 = { 1, 1, 1 }; // pH aleatorio (resto de transmisores)    — Extracción
        private static readonly int[] CANT_OBL_SERVO_G1         = { 0, 0, 0 }; // Báscula servo (SECADORA Y EMPAQUE)      — Extracción
        private static readonly int[] CANT_OBL_NEUMATICA_G1     = { 0, 0, 0 }; // Báscula neumática (SECADORA Y EMPAQUE)  — Extracción
        private static readonly int[] CANT_OBL_BRIX_G2          = { 2, 2, 3 }; // Equipos Brix aleatorios                 — Alcalizado
        // ─────────────────────────────────────────────────────────────────────────

        #region config desmontaje
        private struct ConfigDesmontaje
        {
            public bool MostrarInstrumentos;
            public bool MostrarObligatorios;
            public bool MostrarDesmontaje;
            public int  CantManana;
            public int  CantTarde;
            public int  CantNoche;
        }

        private ConfigDesmontaje ObtenerConfigDesmontaje()
        {
            var cfg = new ConfigDesmontaje
            {
                MostrarInstrumentos = true,
                MostrarObligatorios = true,
                MostrarDesmontaje   = false,
                CantManana = 2, CantTarde = 2, CantNoche = 2
            };
            try
            {
                string sql = "SELECT MostrarTablaInstrumentos, MostrarTablaObligatorios, MostrarTablaDesmontaje, CantManana, CantTarde, CantNoche FROM DesmontajeConfig WHERE Id = 1";
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlDataReader dr = new SqlCommand(sql, conn).ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            cfg.MostrarInstrumentos = Convert.ToBoolean(dr["MostrarTablaInstrumentos"]);
                            cfg.MostrarObligatorios = Convert.ToBoolean(dr["MostrarTablaObligatorios"]);
                            cfg.MostrarDesmontaje   = Convert.ToBoolean(dr["MostrarTablaDesmontaje"]);
                            cfg.CantManana          = Convert.ToInt32(dr["CantManana"]);
                            cfg.CantTarde           = Convert.ToInt32(dr["CantTarde"]);
                            cfg.CantNoche           = Convert.ToInt32(dr["CantNoche"]);
                        }
                    }
                }
            }
            catch { }
            return cfg;
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["CodigoEmpleado"] == null || Session["NombreEmpleado"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                string action = Request.QueryString["Action"];
                if (action == "Reimprimir")
                {
                    CargarUltimaRutina();
                }
                else
                {
                    string codigoEmpleado = Session["CodigoEmpleado"].ToString();
                    string nombreEmpleado = Session["NombreEmpleado"].ToString();

                    // Cargar configuración de visibilidad de tablas
                    ConfigDesmontaje config = ObtenerConfigDesmontaje();

                    // Si la tabla de desmontaje está activa y hay pendientes, redirigir a reporte
                    if (config.MostrarDesmontaje)
                    {
                        int pendiente = ObtenerRutinaConDesmontajePendiente(codigoEmpleado);
                        if (pendiente > 0)
                        {
                            Response.Redirect("RegistroDesmontaje.aspx?RutinaId=" + pendiente);
                            return;
                        }
                    }

                    // Aplicar visibilidad de secciones
                    pnlInstrumentos.Visible = config.MostrarInstrumentos;
                    pnlObligatorios.Visible = config.MostrarObligatorios;
                    pnlDesmontaje.Visible   = config.MostrarDesmontaje;

                    string turnoActual = DeterminarTurnoActual();

                    lblname.Text  = nombreEmpleado;
                    lblfecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    lblturno.Text = turnoActual;
                    MostrarDiaZafra();

                    // Algoritmo principal (siempre corre para obtener el correlativo)
                    string nombreGrupoAsignado;
                    int correlativoGenerado;
                    List<ItemRutina> rutinaGenerada = EjecutarAlgoritmoComplejo(
                        codigoEmpleado, turnoActual,
                        out nombreGrupoAsignado, out correlativoGenerado);

                    List<ItemRutina> obligatorios = SeleccionarEquiposObligatorios(nombreGrupoAsignado, turnoActual);
                    HashSet<string> tagsObligatorios = new HashSet<string>(
                        obligatorios.Select(o => o.TAG.Trim()),
                        StringComparer.OrdinalIgnoreCase);
                    rutinaGenerada = rutinaGenerada
                        .Where(i => !tagsObligatorios.Contains(i.TAG.Trim()))
                        .ToList();

                    if (config.MostrarInstrumentos)
                    {
                        rptRutina.DataSource = rutinaGenerada;
                        rptRutina.DataBind();
                    }
                    if (config.MostrarObligatorios)
                    {
                        rptObligatorios.DataSource = obligatorios;
                        rptObligatorios.DataBind();
                    }
                    GuardarObligatoriosEnRutinaInstrumento(correlativoGenerado, obligatorios);

                    // Tabla de desmontaje
                    if (config.MostrarDesmontaje && correlativoGenerado > 0)
                    {
                        int t = 0;
                        if (turnoActual.Contains("A 22:00"))   t = 1;
                        else if (turnoActual.Contains("22:00") || turnoActual.Contains("A 06:00")) t = 2;
                        int cantTurno = t == 0 ? config.CantManana : t == 1 ? config.CantTarde : config.CantNoche;

                        int idGrupoDesmontaje = (nombreGrupoAsignado == "Extracción") ? 1 : 2;

                        List<ItemRutina> desmontaje = SeleccionarInstrumentosDesmontaje(cantTurno, idGrupoDesmontaje, codigoEmpleado);
                        rptDesmontaje.DataSource = desmontaje;
                        rptDesmontaje.DataBind();
                        GuardarDesmontajeEnRutina(correlativoGenerado, desmontaje);
                    }

                    string logoutUrl = ResolveClientUrl("~/closesession.aspx");
                    string script = $@"
<script type='text/javascript'>
    function cerrarSesionAlTerminarImpresion() {{
        window.location.href = '{logoutUrl}';
    }}
    if (window.matchMedia) {{
        var mediaQueryList = window.matchMedia('print');
        mediaQueryList.addListener(function(mql) {{
            if (!mql.matches) {{ cerrarSesionAlTerminarImpresion(); }}
        }});
    }} else {{
        window.onafterprint = cerrarSesionAlTerminarImpresion;
    }}
    window.onload = function() {{
        var originalTitle = document.title;
        document.title = '';
        window.print();
        document.title = originalTitle;
    }};
</script>";
                    this.ClientScript.RegisterClientScriptBlock(this.GetType(), "ImprimirCerrarSesion", script);
                }
            }
        }

        #region DiaZafra
        private void MostrarDiaZafra()
        {
             //calculo dia zafra
             DateTime fechaInicioZafraReal = new DateTime(2025, 11, 24);
             int diaZafra = (DateTime.Now - fechaInicioZafraReal).Days + 1;
             lblzafra.Text =""+diaZafra;//concateno sin convertir de entero a texto usando las comas
        }
        #endregion

        #region metodos auxiliares
        #region turno actual
        private string DeterminarTurnoActual()
        {
            DateTime ahora = DateTime.Now;
            TimeSpan hora = ahora.TimeOfDay;

            // --- LÓGICA DE DOMINGO (12h / 4h) ---
            if (ahora.DayOfWeek == DayOfWeek.Sunday)
            {
                // Jornada de Día (6am a 6pm): Entrada desde 5:41 AM hasta antes de las 5:41 PM
                if (hora >= new TimeSpan(5, 41, 0) && hora <= new TimeSpan(17, 40, 59))
                {
                    return "06:00 A 18:00";
                }

                // Jornada de Noche: Después de las 5:41 PM (17:41) entra la lógica de sesión
                if (hora >= new TimeSpan(17, 41, 0))
                {
                    if (Session["JornadaDomingo"] != null)
                    {
                        return (Session["JornadaDomingo"].ToString() == "4h") ? "18:00 A 22:00" : "18:00 A 06:00";
                    }
                }
            }

            // --- LÓGICA NORMAL Y RELEVO ---
            // Turno Mañana: 05:41 AM a 01:40 PM
            if (hora >= new TimeSpan(5, 41, 0) && hora <= new TimeSpan(13, 40, 59)) return "06:00 A 14:00";

            // Turno Tarde: 01:41 PM a 09:40 PM
            if (hora >= new TimeSpan(13, 41, 0) && hora <= new TimeSpan(21, 40, 59)) return "14:00 A 22:00";

            // Turno Noche (Relevo y Nocturnos): 09:41 PM a 05:40 AM
            return "22:00 - 06:00";
        }

        private string ObtenerNombreEmpleado(string codigo)
        {
            string nombre = "Instrumentista Desconocido";
            string sql = "SELECT Nombre FROM Empleado WHERE Codigo_empleado = @Codigo;";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Codigo", codigo);
                object resultado = cmd.ExecuteScalar();
                if (resultado != null && resultado != DBNull.Value)
                {
                    nombre = resultado.ToString();
                }
            }
            return nombre;
        }
        #endregion
        #region areas por grupo
        private List<int> ObtenerAreasPorGrupo(int idGrupo)
        {
            List<int> areaIds = new List<int>();
            // Seleccionamos los IDarea que pertenecen al grupo, ordenados por su IDarea (que es el orden geográfico)
            string sql = "SELECT IDarea FROM Area WHERE IDgrupo = @IDGrupo ORDER BY IDarea ASC;";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDGrupo", idGrupo);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    areaIds.Add(reader.GetInt32(0));
                }
            }
            return areaIds;
        }
        #endregion
        // Importante: La función debe recibir el CodigoEmpleado aunque solo se use para el contexto.
        #region obtencion de ID grupo
        public string ObtenerAreaDeterminista(string codigoEmpleado)
        {
            DateTime ahora = DateTime.Now;

            // El turno noche (22:00-06:00) abarca dos días calendario.
            // diaLaboral ancla la jornada al día en que inició el turno para que
            // cruzar medianoche no cambie el área asignada.
            TimeSpan horaActual = ahora.TimeOfDay;
            DateTime diaLaboral = (horaActual < new TimeSpan(5, 41, 0)) ? ahora.AddDays(-1) : ahora;

            int diaZafra = (diaLaboral - FECHA_INICIO_ZAFRA).Days + 1;
            bool esSemanaPar = ((diaZafra / 7) % 2 == 0);
            int diaJuliano = diaLaboral.DayOfYear;

            // CORRECCIÓN TURNO NOCHE: se suma 1 al diaJuliano para que el área nocturna
            // use la paridad del día siguiente. Así el turno del sábado noche ya refleja
            // el domingo, y al regresar el lunes de mañana (día siguiente al domingo)
            // la paridad es naturalmente distinta → no hay repetición de área.
            // A su vez, esto hace que el domingo mañana (sin ajuste) y el lunes tarde
            // sean días consecutivos con paridades opuestas → también sin repetición.
            bool esTurnoNoche = horaActual >= new TimeSpan(21, 41, 0) || horaActual < new TimeSpan(5, 41, 0);
            if (esTurnoNoche)
                diaJuliano += 1;

            var empleado = ConsultarDatosEmpleado(codigoEmpleado);
            bool esCuartoTurno = empleado.EsCuartoTurno;
            int puestoFijo = empleado.PuestoFijo;

            // 1. Calculamos el área que le toca al "Puesto 1" hoy (La base de la rotación)
            // Esta fórmula mantiene la rotación diaria para toda la planta.
            string areaPuesto1 = ((diaJuliano + 1) % 2 == 0) ? "Extracción" : "Alcalizado";
            string areaPuesto2 = (areaPuesto1 == "Extracción") ? "Alcalizado" : "Extracción";

            // 2. Si es Cuarto Turno, decidimos qué puesto está relevando según el día/hora
            if (esCuartoTurno)
            {
                DayOfWeek dia = ahora.DayOfWeek;
                int h = ahora.Hour;
                int m = ahora.Minute;

                // Verificación BD: si hay un trabajador de turno corto (18:00 A 22:00) que
                // ya generó su rutina, el cuarto turno debe acompañarlo (misma área), nunca
                // al trabajador de 12h. Esto cubre el domingo y cualquier escenario similar.
                string areaCompaneroTurnoCorto = ObtenerAreaTrabajadorTurnoCorto();
                if (areaCompaneroTurnoCorto != null)
                    return areaCompaneroTurnoCorto;

                // ¿A quién releva hoy? (lógica determinista para días normales)
                bool relevaAlPuesto1 = false;

                // Martes/Jueves (Mañana/Tarde) -> Releva al Puesto 1
                if (dia == DayOfWeek.Tuesday || dia == DayOfWeek.Thursday) relevaAlPuesto1 = true;

                // Domingo Noche (≥22:00) / Lunes Noche (≥21:41)
                // CORRECCIÓN: el domingo usa h >= 22 porque el turno de 4h termina
                // exactamente a las 22:00; usar 21:41 mezcla la lógica nocturna con
                // los últimos minutos del turno de 4h y puede asignar el área incorrecta.
                else if ((dia == DayOfWeek.Sunday && h >= 22) ||
                         (dia == DayOfWeek.Monday && (h > 21 || (h == 21 && m >= 41))))
                {
                    relevaAlPuesto1 = esSemanaPar ? false : true;
                }
                else if ((dia == DayOfWeek.Monday || dia == DayOfWeek.Tuesday) && h < 6)
                {
                    // Madrugada (continuación del relevo nocturno)
                    relevaAlPuesto1 = esSemanaPar ? false : true;
                }

                return relevaAlPuesto1 ? areaPuesto1 : areaPuesto2;
            }
            else
            {
                // 3. Para los demás (Puestos fijos), se usa la lógica estándar del Día Juliano
                return (puestoFijo == 1) ? areaPuesto1 : areaPuesto2;
            }
        }
        #endregion
        // -------------------------------------------------------------------------------------
        // MÉTODO COORDINADOR PRINCIPAL
        // -------------------------------------------------------------------------------------

        private List<ItemRutina> EjecutarAlgoritmoComplejo(string codigoEmpleado, string turnoActual, out string nombreGrupoAsignado, out int correlativoGenerado)
        {
            // 1. DETERMINAR EL ÁREA USANDO EL NUEVO MOTOR
            // Esto reemplaza la búsqueda de "quién lo hizo hace una hora"
            string areaCalculada = ObtenerAreaDeterminista(codigoEmpleado);

            // 2. OBTENER EL ID DEL GRUPO (1 para Extracción, 2 para Alcalizado)
            // Suponiendo que en tu tabla 'Rotaciongrupos' G1 = Extracción y G2 = Alcalizado
            int idGrupoAsignado = (areaCalculada == "Extracción") ? 1 : 2;

            nombreGrupoAsignado = areaCalculada; // Ahora el nombre es el área misma

            // 3. SELECCIÓN DE INSTRUMENTOS
            // El algoritmo de selección de 5 Alta, 5 Media, 5 Baja sigue igual,
            // pero ahora recibe el grupo correcto por calendario.
            //List<ItemRutina> instrumentosSeleccionados = SeleccionarInstrumentos(idGrupoAsignado);
            List<ItemRutina> instrumentosSeleccionados = SeleccionarInstrumentos(idGrupoAsignado, turnoActual);
            // 4. GUARDADO DE LA RUTINA
            correlativoGenerado = GuardarNuevaRutina(codigoEmpleado, turnoActual, idGrupoAsignado, instrumentosSeleccionados);

            return instrumentosSeleccionados;
        }
        #region metodo de consulta
        private (int PuestoFijo, bool EsCuartoTurno) ConsultarDatosEmpleado(string codigoEmpleado)
        {
            int puesto = 0;
            bool esRelevo = false;
            string sql = "SELECT PuestoFijo, EsCuartoTurno FROM Empleado WHERE Codigo_empleado = @Codigo";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Codigo", codigoEmpleado);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        puesto = dr["PuestoFijo"] != DBNull.Value ? Convert.ToInt32(dr["PuestoFijo"]) : 0;
                        esRelevo = dr["EsCuartoTurno"] != DBNull.Value && Convert.ToBoolean(dr["EsCuartoTurno"]);
                    }
                }
            }
            return (puesto, esRelevo);
        }

        // Busca en BD si un trabajador de puesto fijo (no cuarto turno) ya generó una rutina
        // con el turno corto "18:00 A 22:00" en las últimas 12 horas.
        // Si existe, devuelve su área para que el cuarto turno lo acompañe.
        // Retorna null si no se encontró ese escenario.
        private string ObtenerAreaTrabajadorTurnoCorto()
        {
            string sql = @"
                SELECT TOP 1
                    CASE WHEN R.IDgrupo = 1 THEN 'Extracción' ELSE 'Alcalizado' END
                FROM Rutinas R
                INNER JOIN Empleado E ON R.Codigo_empleado = E.Codigo_empleado
                WHERE R.Fecha >= @LimiteTiempo
                  AND (E.EsCuartoTurno = 0 OR E.EsCuartoTurno IS NULL)
                  AND R.Turno = '18:00 A 22:00'
                ORDER BY R.Correlativo DESC";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LimiteTiempo", DateTime.Now.AddHours(-12));
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }
        #endregion
        #endregion
        #region fase1
        // ----------------------------------------------------
        // FASE 1: ROTACIÓN DE GRUPOS (Lógica de Ciclo)
        // ----------------------------------------------------
        /* private GrupoRotacionAsignado ObtenerGrupoRotacion()
         {
             int ultimoGrupoID = 0;
             DataTable dtGrupo = new DataTable();

             using (SqlConnection conn = new SqlConnection(ConnString))
             {
                 conn.Open();

                 // Buscar el ID del último grupo asignado
                 string sqlUltimoGrupo = "SELECT TOP 1 IDgrupo FROM Rutinas ORDER BY Correlativo DESC;";
                 SqlCommand cmd = new SqlCommand(sqlUltimoGrupo, conn);
                 object resultado = cmd.ExecuteScalar();

                 if (resultado != null && resultado != DBNull.Value)
                 {
                     ultimoGrupoID = Convert.ToInt32(resultado);
                 }

                 // 1. Intentar encontrar el siguiente grupo (ID > Ultimo ID)
                 string sqlProximoGrupo = $@"
                 SELECT TOP 1 IDgrupo, Nombregrupo 
                 FROM Rotaciongrupos
                 WHERE IDgrupo > {ultimoGrupoID}
                 ORDER BY IDgrupo ASC;";

                 // 2. Respaldo: Si no hay, buscar el primer grupo (ID más bajo)
                 string sqlPrimerGrupo = @"
                 SELECT TOP 1 IDgrupo, Nombregrupo 
                 FROM Rotaciongrupos
                 ORDER BY IDgrupo ASC;";

                 cmd.CommandText = sqlProximoGrupo;
                 SqlDataAdapter da = new SqlDataAdapter(cmd);
                 da.Fill(dtGrupo);

                 if (dtGrupo.Rows.Count == 0)
                 {
                     // Si la consulta no devuelve resultados, reiniciar la rotación.
                     cmd.CommandText = sqlPrimerGrupo;
                     da.SelectCommand.Parameters.Clear();
                     da.Fill(dtGrupo);
                 }
             }

             if (dtGrupo.Rows.Count > 0)
             {
                 return new GrupoRotacionAsignado
                 {
                     IdGrupo = Convert.ToInt32(dtGrupo.Rows[0]["IDgrupo"]),
                     NombreGrupo = dtGrupo.Rows[0]["Nombregrupo"].ToString()
                 };
             }

             throw new Exception("Error: La tabla Rotaciongrupos está vacía.");
         }*/
        #endregion
        #region fase2
        // -------------------------------------------------------------------------
        // FASE 2: SELECCIÓN DE INSTRUMENTOS (5x Área: 1 Alta, 1 Media, 3 Baja)
        // -------------------------------------------------------------------------
        private List<ItemRutina> SeleccionarInstrumentos(int idGrupo, string turnoActual)
        {
            List<ItemRutina> rutina = new List<ItemRutina>();
            List<string> tagsExcluidos = new List<string>();

            // 1. Obtener todas las áreas del grupo (12 para Extracción, 9 para Alcalizado)
            List<int> todasLasAreas = ObtenerAreasPorGrupo(idGrupo);
            int totalAreas = todasLasAreas.Count;

            // 2. Determinar el índice del bloque según el turno
            // Bloque 0: Mañana, Bloque 1: Tarde, Bloque 2: Noche
            // NOTA: No usar Contains("14:00") porque "06:00 A 14:00" (mañana) también lo contiene.
            // Se usa "A 22:00" que es exclusivo del turno tarde.
            int indiceBloque = 0;
            if (turnoActual.Contains("A 22:00")) indiceBloque = 1;
            else if (turnoActual.Contains("22:00") || turnoActual.Contains("A 06:00")) indiceBloque = 2;

            // 3. Calcular rango (Ej: 12 áreas / 3 turnos = 4 áreas por turno)
            int areasPorTurno = totalAreas / 3;
            int inicio = indiceBloque * areasPorTurno;

            // Ajuste para el último turno por si la división no es exacta
            int fin = (indiceBloque == 2) ? totalAreas : inicio + areasPorTurno;

            // 4. Tomar solo el subconjunto de áreas que corresponden a este turno
            List<int> areasFiltradas = todasLasAreas.GetRange(inicio, fin - inicio);

            // Cantidades leídas de las constantes de configuración al inicio de la clase.
            int cantAlta  = (idGrupo == 2) ? CANT_ALTA_G2  : CANT_ALTA_G1;
            int cantMedia = (idGrupo == 2) ? CANT_MEDIA_G2 : CANT_MEDIA_G1;
            int cantBaja  = (idGrupo == 2) ? CANT_BAJA_G2  : CANT_BAJA_G1;

            // 5. Generar instrumentos solo para esas áreas
            foreach (int idArea in areasFiltradas)
            {
                var alta = ObtenerInstrumentosPorAreaPrioridad(idArea, "Alta", cantAlta, tagsExcluidos);
                rutina.AddRange(alta);
                tagsExcluidos.AddRange(alta.Select(i => i.TAG).ToList());

                var media = ObtenerInstrumentosPorAreaPrioridad(idArea, "Media", cantMedia, tagsExcluidos);
                rutina.AddRange(media);
                tagsExcluidos.AddRange(media.Select(i => i.TAG).ToList());

                var baja = ObtenerInstrumentosPorAreaPrioridad(idArea, "Baja", cantBaja, tagsExcluidos);
                rutina.AddRange(baja);
                tagsExcluidos.AddRange(baja.Select(i => i.TAG).ToList());
            }

            return rutina;
        }
        /*private List<ItemRutina> SeleccionarInstrumentos(int idGrupo)
        {
            List<ItemRutina> rutina = new List<ItemRutina>();
            List<string> tagsExcluidos = new List<string>();

            // 1. Obtener todas las áreas del grupo asignado en orden geográfico
            List<int> areasDelGrupo = ObtenerAreasPorGrupo(idGrupo);

            // 2. Iterar sobre CADA ÁREA
            foreach (int idArea in areasDelGrupo)
            {
                // Seleccionar 1 equipo de ALTA Prioridad para esta área
                var alta = ObtenerInstrumentosPorAreaPrioridad(idArea, "Alta", 1, tagsExcluidos);
                rutina.AddRange(alta);
                tagsExcluidos.AddRange(alta.Select(i => i.TAG).ToList()); // Excluir en esta ejecución

                // Seleccionar 1 equipo de MEDIA Prioridad para esta área
                var media = ObtenerInstrumentosPorAreaPrioridad(idArea, "Media", 1, tagsExcluidos);
                rutina.AddRange(media);
                tagsExcluidos.AddRange(media.Select(i => i.TAG).ToList());

                // Seleccionar 1 equipo de BAJA Prioridad para esta área
                var baja = ObtenerInstrumentosPorAreaPrioridad(idArea, "Baja", 1, tagsExcluidos);
                rutina.AddRange(baja);

                // La lista 'rutina' se construye secuencialmente, asegurando el orden geográfico.
            }

            return rutina;
        }*/

        // Selecciona instrumentos con tres niveles de tolerancia:
        //   Paso 1 — excluye 24h + sesión actual          (lo más variado)
        //   Paso 2 — excluye solo sesión, ignora 24h      (reutiliza usados ayer)
        //   Paso 3 — sin ninguna exclusión                (reutiliza cualquiera, incluso de esta sesión)
        private List<ItemRutina> ObtenerInstrumentosPorAreaPrioridad(int idArea, string prioridadNombre, int cantidad, List<string> tagsExcluidos)
        {
            // Paso 1: preferido — respeta historial 24h y exclusión de sesión
            var lista = ConsultarInstrumentos(idArea, prioridadNombre, cantidad, tagsExcluidos, excluir24h: true);

            // Paso 2: fallback — ignora 24h pero mantiene exclusión de sesión
            if (lista.Count < cantidad)
                lista = ConsultarInstrumentos(idArea, prioridadNombre, cantidad, tagsExcluidos, excluir24h: false);

            // Paso 3: último recurso — reutiliza cualquier instrumento del área/prioridad
            if (lista.Count < cantidad)
                lista = ConsultarInstrumentos(idArea, prioridadNombre, cantidad, new List<string>(), excluir24h: false);

            return lista;
        }

        private List<ItemRutina> ConsultarInstrumentos(int idArea, string prioridadNombre, int cantidad, List<string> tagsExcluidos, bool excluir24h)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            DataTable dt = new DataTable();

            string filtroSesion = tagsExcluidos.Any()
                ? "AND I.TAG NOT IN ('" + string.Join("','", tagsExcluidos) + "')"
                : string.Empty;

            string filtro24h = excluir24h
                ? @"AND I.TAG NOT IN (
                    SELECT RI.TAG
                    FROM Rutinas R
                    INNER JOIN Rutina_instrumento RI ON R.Correlativo = RI.Correlativo
                    WHERE R.Fecha >= DATEADD(hour, -24, GETDATE())
                )"
                : string.Empty;

            string sql = $@"
            SELECT TOP {cantidad}
                I.TAG,
                I.Nombre AS NombreInstrumento,
                I.Actividad,
                A.Nombre AS NombreArea
            FROM Instrumentos I
            INNER JOIN Area A ON I.IDarea = A.IDarea
            WHERE
                I.IDarea = @IDArea
                AND I.IDprioridad = (SELECT IDprioridad FROM Prioridad WHERE Nombre = @PrioridadNombre)
                {filtroSesion}
                {filtro24h}
            ORDER BY NEWID()";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDArea", idArea);
                cmd.Parameters.AddWithValue("@PrioridadNombre", prioridadNombre);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ItemRutina
                {
                    NombreArea = row["NombreArea"].ToString().Trim(),
                    TAG = row["TAG"].ToString().Trim(),
                    NombreInstrumento = row["NombreInstrumento"].ToString().Trim(),
                    Actividad = row["Actividad"].ToString().Trim()
                });
            }
            return lista;
        }
        #endregion
        #region fase3
        // ----------------------------------------------------
        // FASE 3: GUARDADO DE LA RUTINA (Transacción)
        // ----------------------------------------------------
        private int GuardarNuevaRutina(string codigoEmpleado, string turnoActual, int idGrupo, List<ItemRutina> instrumentos)
        {
            if (instrumentos == null || instrumentos.Count == 0) return 0;

            int nuevoCorrelativo = 0;

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insertar Cabecera de Rutina
                    string sqlInsertRutina = @"
                    INSERT INTO Rutinas (Fecha, Turno, Codigo_empleado, IDgrupo)
                    VALUES (GETDATE(), @Turno, @Empleado, @IDGrupo);
                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdRutina = new SqlCommand(sqlInsertRutina, conn, transaction);
                    cmdRutina.Parameters.AddWithValue("@Turno", turnoActual);
                    cmdRutina.Parameters.AddWithValue("@Empleado", codigoEmpleado);
                    cmdRutina.Parameters.AddWithValue("@IDGrupo", idGrupo);

                    nuevoCorrelativo = Convert.ToInt32(cmdRutina.ExecuteScalar());

                    // 2. Insertar Detalle de Instrumentos (regulares: EsObligatorio = 0)
                    string sqlInsertDetalle = @"
                    INSERT INTO Rutina_instrumento (Correlativo, TAG, EsObligatorio)
                    VALUES (@Correlativo, @TAG, 0);";

                    foreach (var item in instrumentos)
                    {
                        SqlCommand cmdDetalle = new SqlCommand(sqlInsertDetalle, conn, transaction);
                        cmdDetalle.Parameters.AddWithValue("@Correlativo", nuevoCorrelativo);
                        cmdDetalle.Parameters.AddWithValue("@TAG", item.TAG);
                        cmdDetalle.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    nuevoCorrelativo = 0;
                }
            }
            return nuevoCorrelativo;
        }

        private void GuardarObligatoriosEnRutinaInstrumento(int correlativo, List<ItemRutina> obligatorios)
        {
            if (correlativo <= 0 || obligatorios == null || obligatorios.Count == 0) return;

            string sql = @"
                IF EXISTS (SELECT 1 FROM Rutina_instrumento WHERE Correlativo = @Correlativo AND TAG = @TAG)
                    UPDATE Rutina_instrumento SET EsObligatorio = 1 WHERE Correlativo = @Correlativo AND TAG = @TAG
                ELSE
                    INSERT INTO Rutina_instrumento (Correlativo, TAG, EsObligatorio) VALUES (@Correlativo, @TAG, 1)";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                foreach (var item in obligatorios)
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Correlativo", correlativo);
                    cmd.Parameters.AddWithValue("@TAG", item.TAG);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
        #region comprobaciones obligatorias
        private List<ItemRutina> SeleccionarEquiposObligatorios(string areaCalculada, string turnoActual)
        {
            List<ItemRutina> lista = new List<ItemRutina>();

            // Determinar índice de turno: 0=Mañana, 1=Tarde, 2=Noche
            int t = 0;
            if (turnoActual.Contains("A 22:00")) t = 1;
            else if (turnoActual.Contains("22:00") || turnoActual.Contains("A 06:00")) t = 2;

            if (areaCalculada == "Extracción")
            {
                // pH fijo: Transmisor De Ph Jugo Alcalizado E+H
                string sqlPhFijo = $@"
                    SELECT TOP {CANT_OBL_PH_FIJO_G1[t]}
                        I.TAG,
                        I.Nombre AS NombreInstrumento,
                        A.Nombre AS NombreArea
                    FROM Instrumentos I
                    INNER JOIN Area A ON I.IDarea = A.IDarea
                    WHERE I.TipoAnalisis = 'pH'
                      AND I.Nombre = 'Transmisor De Ph Jugo Alcalizado E+H'";
                lista.AddRange(EjecutarConsultaObligatorios(sqlPhFijo));

                // pH aleatorio (excluyendo el fijo y los usados en el turno anterior)
                string sqlPhAleatorio = $@"
                    SELECT TOP {CANT_OBL_PH_ALEATORIO_G1[t]}
                        I.TAG,
                        I.Nombre AS NombreInstrumento,
                        A.Nombre AS NombreArea
                    FROM Instrumentos I
                    INNER JOIN Area A ON I.IDarea = A.IDarea
                    WHERE I.TipoAnalisis = 'pH'
                      AND I.Nombre <> 'Transmisor De Ph Jugo Alcalizado E+H'
                      AND I.TAG NOT IN (
                          SELECT RI.TAG
                          FROM Rutina_instrumento RI
                          INNER JOIN Rutinas R ON RI.Correlativo = R.Correlativo
                          WHERE R.Fecha >= DATEADD(hour, -10, GETDATE())
                            AND RI.EsObligatorio = 1
                      )
                    ORDER BY NEWID()";
                var phAleatorio = EjecutarConsultaObligatorios(sqlPhAleatorio);
                if (phAleatorio.Count < CANT_OBL_PH_ALEATORIO_G1[t])
                {
                    // Fallback: no hay suficientes instrumentos nuevos, se permite repetir
                    string sqlPhFallback = $@"
                        SELECT TOP {CANT_OBL_PH_ALEATORIO_G1[t]}
                            I.TAG,
                            I.Nombre AS NombreInstrumento,
                            A.Nombre AS NombreArea
                        FROM Instrumentos I
                        INNER JOIN Area A ON I.IDarea = A.IDarea
                        WHERE I.TipoAnalisis = 'pH'
                          AND I.Nombre <> 'Transmisor De Ph Jugo Alcalizado E+H'
                        ORDER BY NEWID()";
                    phAleatorio = EjecutarConsultaObligatorios(sqlPhFallback);
                }
                lista.AddRange(phAleatorio);

                // Báscula servo del área SECADORA Y EMPAQUE
                string sqlServo = $@"
                    SELECT TOP {CANT_OBL_SERVO_G1[t]}
                        I.TAG,
                        I.Nombre AS NombreInstrumento,
                        A.Nombre AS NombreArea
                    FROM Instrumentos I
                    INNER JOIN Area A ON I.IDarea = A.IDarea
                    WHERE A.Nombre LIKE '%SECADORA%'
                      AND I.Nombre LIKE '%SERVO%'
                    ORDER BY NEWID()";
                lista.AddRange(EjecutarConsultaObligatorios(sqlServo));

                // Báscula neumática del área SECADORA Y EMPAQUE
                string sqlNeumatica = $@"
                    SELECT TOP {CANT_OBL_NEUMATICA_G1[t]}
                        I.TAG,
                        I.Nombre AS NombreInstrumento,
                        A.Nombre AS NombreArea
                    FROM Instrumentos I
                    INNER JOIN Area A ON I.IDarea = A.IDarea
                    WHERE A.Nombre LIKE '%SECADORA%'
                      AND I.Nombre LIKE '%NEUMATICA%'
                    ORDER BY NEWID()";
                lista.AddRange(EjecutarConsultaObligatorios(sqlNeumatica));
            }
            else // Alcalizado
            {
                // Equipos Brix aleatorios, excluyendo los usados en el turno anterior
                string sqlBrix = $@"
                    SELECT TOP {CANT_OBL_BRIX_G2[t]}
                        I.TAG,
                        I.Nombre AS NombreInstrumento,
                        A.Nombre AS NombreArea
                    FROM Instrumentos I
                    INNER JOIN Area A ON I.IDarea = A.IDarea
                    WHERE I.TipoAnalisis = 'Brix'
                      AND I.TAG NOT IN (
                          SELECT RI.TAG
                          FROM Rutina_instrumento RI
                          INNER JOIN Rutinas R ON RI.Correlativo = R.Correlativo
                          WHERE R.Fecha >= DATEADD(hour, -10, GETDATE())
                            AND RI.EsObligatorio = 1
                      )
                    ORDER BY NEWID()";
                var brix = EjecutarConsultaObligatorios(sqlBrix);
                if (brix.Count < CANT_OBL_BRIX_G2[t])
                {
                    // Fallback: no hay suficientes instrumentos Brix nuevos, se permite repetir
                    string sqlBrixFallback = $@"
                        SELECT TOP {CANT_OBL_BRIX_G2[t]}
                            I.TAG,
                            I.Nombre AS NombreInstrumento,
                            A.Nombre AS NombreArea
                        FROM Instrumentos I
                        INNER JOIN Area A ON I.IDarea = A.IDarea
                        WHERE I.TipoAnalisis = 'Brix'
                        ORDER BY NEWID()";
                    brix = EjecutarConsultaObligatorios(sqlBrixFallback);
                }
                lista.AddRange(brix);
            }

            return lista;
        }

        private List<ItemRutina> EjecutarConsultaObligatorios(string sql)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ItemRutina
                {
                    NombreArea = row["NombreArea"].ToString().Trim(),
                    TAG = row["TAG"].ToString().Trim(),
                    NombreInstrumento = row["NombreInstrumento"].ToString().Trim(),
                    Actividad = string.Empty
                });
            }
            return lista;
        }
        #endregion
        #region ultimarutina
        private void CargarUltimaRutina()
        {
            // --- 1. OBTENER PARÁMETROS DE SESIÓN Y TURNO ---
            if (Session["CodigoEmpleado"] == null || Session["NombreEmpleado"] == null)
            {
                return;
            }

            string codigoEmpleado = Session["CodigoEmpleado"].ToString();
            string nombreEmpleado = Session["NombreEmpleado"].ToString();
            string turnoActual = DeterminarTurnoActual();

            // Almacenará el Correlativo y la Fecha/Hora de la última rutina encontrada
            int correlativoUltimaRutina = 0;
            string fechaRutinaGuardada = "";
            string turnoGuardado = turnoActual; // fallback: usar el calculado

            // --- A. BUSCAR EL CORRELATIVO MÁS RECIENTE ---
            // Se filtra por las últimas 8 horas (igual que RutinaRecienteExiste) en lugar de
            // filtrar por turno, porque al reimprimir la sesión ya fue destruida y
            // DeterminarTurnoActual() puede devolver un turno distinto al que se guardó
            // (especialmente en los turnos especiales de domingo donde se pierde Session["JornadaDomingo"]).
            string sqlBuscarCorrelativo = @"
        SELECT TOP 1
            Correlativo,
            CONVERT(VARCHAR, Fecha, 120) AS FechaRutina,
            Turno
        FROM Rutinas
        WHERE
            Codigo_empleado = @CodigoEmpleado
            AND Fecha >= @LimiteTiempo
        ORDER BY Correlativo DESC";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlBuscarCorrelativo, conn);
                cmd.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                cmd.Parameters.AddWithValue("@LimiteTiempo", DateTime.Now.AddHours(-8));

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    correlativoUltimaRutina = reader.GetInt32(0); // Columna Correlativo
                    fechaRutinaGuardada = reader.GetString(1);    // Columna FechaRutina
                    turnoGuardado = reader.GetString(2);           // Turno tal como fue guardado
                }
                reader.Close();
            }

            if (correlativoUltimaRutina == 0)
            {
                // No se encontró una rutina previa.
                Response.Write("<script>alert('No se encontró una rutina previa generada por usted para este turno actual.');</script>");
                return;
            }

            // --- B. CARGAR TODOS LOS INSTRUMENTOS USANDO EL CORRELATIVO ENCONTRADO ---
            // RI.EsObligatorio = 0: solo instrumentos regulares en rptRutina.
            // Los obligatorios (EsObligatorio = 1) se cargan por separado en rptObligatorios.
            string sqlCargarDetalle = @"
        SELECT
            LTRIM(RTRIM(A.Nombre)) AS NombreArea,
            LTRIM(RTRIM(I.TAG))    AS TAG,
            LTRIM(RTRIM(I.Nombre)) AS NombreInstrumento,
            LTRIM(RTRIM(I.Actividad)) AS Actividad
        FROM Rutina_instrumento RI
        INNER JOIN Instrumentos I ON RI.TAG = I.TAG
        INNER JOIN Area A ON I.IDarea = A.IDarea
        WHERE
            RI.Correlativo = @Correlativo
            AND RI.EsObligatorio = 0
        ORDER BY A.IDarea ASC"; // Ordenamos por área para mantener el orden geográfico

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlCargarDetalle, conn);
                cmd.Parameters.AddWithValue("@Correlativo", correlativoUltimaRutina); // Usamos el Correlativo único

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            // --- C. LLENADO DE ENCABEZADO Y REPEATER ---

            ConfigDesmontaje config = ObtenerConfigDesmontaje();
            pnlInstrumentos.Visible = config.MostrarInstrumentos;
            pnlObligatorios.Visible = config.MostrarObligatorios;
            pnlDesmontaje.Visible   = config.MostrarDesmontaje;

            lblname.Text  = nombreEmpleado;
            lblfecha.Text = fechaRutinaGuardada;
            lblturno.Text = turnoGuardado;
            MostrarDiaZafra();

            if (config.MostrarInstrumentos)
            {
                rptRutina.DataSource = dt;
                rptRutina.DataBind();
                rptRutina.Visible = true;
            }

            if (config.MostrarObligatorios)
            {
                List<ItemRutina> obligatorios = CargarObligatoriosDesdeRutina(correlativoUltimaRutina);
                rptObligatorios.DataSource = obligatorios;
                rptObligatorios.DataBind();
            }

            if (config.MostrarDesmontaje)
            {
                List<ItemRutina> desmontaje = CargarDesmontajeDesdeRutina(correlativoUltimaRutina);
                rptDesmontaje.DataSource = desmontaje;
                rptDesmontaje.DataBind();
            }

            // --- 4. IMPRESIÓN AUTOMÁTICA ---
            //string script = @"window.onload = function() { var originalTitle = document.title; document.title = ''; window.print(); document.title = originalTitle; };";
            string logoutUrl = ResolveClientUrl("~/closesession.aspx");

            // 2. Definimos el script que se ejecutará en el cliente (navegador)
            string scriptReimpresion = $@"
<script type='text/javascript'>
    
    // Función que se llama cuando el usuario cierra el diálogo de impresión
    function cerrarSesionAlTerminarImpresion() {{
        // Redirigimos la ventana actual a la página de servicio para destruir la sesión.
        window.location.href = '{logoutUrl}'; 
    }}

    // Preparamos el listener para el evento 'onafterprint' (post-impresión)
    if (window.matchMedia) {{
        // Listener moderno: Mejor detección del cierre del diálogo
        var mediaQueryList = window.matchMedia('print');
        mediaQueryList.addListener(function(mql) {{
            // mql.matches es true durante la impresión, false cuando se cierra el diálogo
            if (!mql.matches) {{
                cerrarSesionAlTerminarImpresion();
            }}
        }});
    }} else {{
        // Fallback para navegadores antiguos
        window.onafterprint = cerrarSesionAlTerminarImpresion;
    }}
    
    // Ejecutamos la impresión justo después de registrar los manejadores
    window.onload = function() {{
        // Lógica para el título
        var originalTitle = document.title;
        document.title = '';
        
        // Abre el diálogo de impresión
        window.print();
        
        // Restaura el título
        document.title = originalTitle;
    }};
</script>";

            // 3. Registramos el script en el cliente
            // Usamos RegisterClientScriptBlock para que cargue en el HEAD y maneje el evento onload.
            ClientScript.RegisterClientScriptBlock(this.GetType(), "ReimprimirRutina", scriptReimpresion, false);
            // Nota: Se usa 'false' para indicar que la etiqueta <script> ya está incluida en la cadena.
        }
        #endregion
        #region desmontaje
        private int ObtenerRutinaConDesmontajePendiente(string codigoEmpleado)
        {
            string sql = @"
                SELECT TOP 1 r.Correlativo
                FROM Rutinas r
                INNER JOIN Rutina_desmontaje rd ON r.Correlativo = rd.RutinaId
                WHERE r.Codigo_empleado = @Codigo AND rd.Reportado = 0
                ORDER BY r.Correlativo DESC";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Codigo", codigoEmpleado);
                object result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        private struct AsignacionDesmontaje
        {
            public int    AreaId;
            public string Keyword;
        }

        private List<AsignacionDesmontaje> ObtenerAreasDesmontajeEmpleado(string codigoEmpleado)
        {
            var lista = new List<AsignacionDesmontaje>();
            string sql = "SELECT Area1Id, Area2Id, Keyword1, Keyword2 FROM DesmontajeEmpleadoArea WHERE Codigo_empleado = @Codigo";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Codigo", codigoEmpleado);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        if (dr["Area1Id"] != DBNull.Value)
                            lista.Add(new AsignacionDesmontaje
                            {
                                AreaId  = Convert.ToInt32(dr["Area1Id"]),
                                Keyword = dr["Keyword1"] != DBNull.Value ? dr["Keyword1"].ToString().Trim() : null
                            });
                        if (dr["Area2Id"] != DBNull.Value)
                            lista.Add(new AsignacionDesmontaje
                            {
                                AreaId  = Convert.ToInt32(dr["Area2Id"]),
                                Keyword = dr["Keyword2"] != DBNull.Value ? dr["Keyword2"].ToString().Trim() : null
                            });
                    }
                }
            }
            return lista;
        }

        private List<ItemRutina> SeleccionarInstrumentosDesmontaje(int cantidad, int idGrupoDefault, string codigoEmpleado)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            if (cantidad <= 0) return lista;

            List<AsignacionDesmontaje> asignaciones = ObtenerAreasDesmontajeEmpleado(codigoEmpleado);

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd;

                if (asignaciones.Count > 0)
                {
                    // Construir condición por área con keyword opcional — usa DI.Nombre y DI.AreaId
                    var condiciones = new List<string>();
                    cmd = new SqlCommand();
                    cmd.Connection = conn;

                    for (int i = 0; i < asignaciones.Count; i++)
                    {
                        string pArea = "@Area" + i;
                        cmd.Parameters.AddWithValue(pArea, asignaciones[i].AreaId);

                        if (string.IsNullOrEmpty(asignaciones[i].Keyword))
                        {
                            condiciones.Add($"DI.AreaId = {pArea}");
                        }
                        else
                        {
                            string pKw = "@Kw" + i;
                            cmd.Parameters.AddWithValue(pKw, "%" + asignaciones[i].Keyword.ToUpper() + "%");
                            condiciones.Add($"(DI.AreaId = {pArea} AND UPPER(DI.Nombre) LIKE {pKw})");
                        }
                    }

                    cmd.CommandText = $@"
                        SELECT TOP {cantidad}
                            DI.TAG, DI.Nombre AS NombreInstrumento, A.Nombre AS NombreArea
                        FROM DesmontajeInstrumento DI
                        INNER JOIN Area A ON DI.AreaId = A.IDarea
                        WHERE DI.Estado = 0
                          AND ({string.Join(" OR ", condiciones)})
                        ORDER BY NEWID()";
                }
                else
                {
                    // Sin asignación específica: rotación automática + áreas habilitadas
                    cmd = new SqlCommand($@"
                        SELECT TOP {cantidad}
                            DI.TAG, DI.Nombre AS NombreInstrumento, A.Nombre AS NombreArea
                        FROM DesmontajeInstrumento DI
                        INNER JOIN Area A ON DI.AreaId = A.IDarea
                        INNER JOIN DesmontajeAreaHabilitada DAH ON DI.AreaId = DAH.AreaId
                        WHERE DI.Estado = 0
                          AND A.IDgrupo = @IdGrupo
                        ORDER BY NEWID()", conn);
                    cmd.Parameters.AddWithValue("@IdGrupo", idGrupoDefault);
                }

                new SqlDataAdapter(cmd).Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ItemRutina
                {
                    TAG               = row["TAG"].ToString().Trim(),
                    NombreInstrumento = row["NombreInstrumento"].ToString().Trim(),
                    NombreArea        = row["NombreArea"].ToString().Trim(),
                    Actividad         = string.Empty
                });
            }
            return lista;
        }

        private void GuardarDesmontajeEnRutina(int correlativo, List<ItemRutina> desmontaje)
        {
            if (correlativo <= 0 || desmontaje == null || desmontaje.Count == 0) return;
            string sql = "INSERT INTO Rutina_desmontaje (RutinaId, TAG, Reportado, Desmontado) VALUES (@RutinaId, @TAG, 0, 0)";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                foreach (var item in desmontaje)
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@RutinaId", correlativo);
                    cmd.Parameters.AddWithValue("@TAG",      item.TAG);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<ItemRutina> CargarDesmontajeDesdeRutina(int correlativo)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            string sql = @"
                SELECT DI.TAG, DI.Nombre AS NombreInstrumento, A.Nombre AS NombreArea
                FROM Rutina_desmontaje RD
                INNER JOIN DesmontajeInstrumento DI ON RD.TAG = DI.TAG
                INNER JOIN Area A ON DI.AreaId = A.IDarea
                WHERE RD.RutinaId = @Correlativo";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Correlativo", correlativo);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new ItemRutina
                        {
                            TAG               = reader["TAG"].ToString(),
                            NombreInstrumento = reader["NombreInstrumento"].ToString(),
                            NombreArea        = reader["NombreArea"].ToString(),
                            Actividad         = string.Empty
                        });
                    }
                }
            }
            return lista;
        }
        #endregion

        #region carga obligatorios reimpresion
        private List<ItemRutina> CargarObligatoriosDesdeRutina(int correlativo)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            string sql = @"
                SELECT
                    LTRIM(RTRIM(A.Nombre)) AS NombreArea,
                    LTRIM(RTRIM(I.TAG))    AS TAG,
                    LTRIM(RTRIM(I.Nombre)) AS NombreInstrumento,
                    ''                     AS Actividad
                FROM Rutina_instrumento RI
                INNER JOIN Instrumentos I ON RI.TAG = I.TAG
                INNER JOIN Area A ON I.IDarea = A.IDarea
                WHERE RI.Correlativo = @Correlativo
                  AND RI.EsObligatorio = 1";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Correlativo", correlativo);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new ItemRutina
                    {
                        NombreArea        = reader["NombreArea"].ToString(),
                        TAG               = reader["TAG"].ToString(),
                        NombreInstrumento = reader["NombreInstrumento"].ToString(),
                        Actividad         = string.Empty
                    });
                }
            }
            return lista;
        }
        #endregion
    }
}
