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
        private readonly DateTime FECHA_INICIO_ZAFRA = new DateTime(2025, 11, 24);
        
        private readonly string ConnString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. OBTENER DATOS DE LA SESIÓN
                // Validamos la sesión. Si no hay código, redirigimos al login.
                /*if (Session["CodigoEmpleado"] == null || Session["NombreEmpleado"] == null)
                {
                    // Opcional: Redirigir al usuario si no ha iniciado sesión
                    // Response.Redirect("Login.aspx");
                    return;
                }*/

                if (Session["CodigoEmpleado"] == null || Session["NombreEmpleado"] == null)
                {
                    // Opcional: Redirigir si la sesión no existe
                    Response.Redirect("Login.aspx");
                    return;
                }

                // 2. Obtener la acción de la URL (QueryString)
                string action = Request.QueryString["Action"];
                if (action == "Reimprimir")
                {
                    CargarUltimaRutina();
                }
                else
                {
                    string codigoEmpleado = Session["CodigoEmpleado"].ToString();
                    string nombreEmpleado = Session["NombreEmpleado"].ToString(); // ¡Directo de la sesión!

                    // 2. PREPARACIÓN DE DATOS LOCALES
                    string turnoActual = DeterminarTurnoActual();

                    // 3. EJECUCIÓN DEL ALGORITMO COMPLEJO
                    string nombreGrupoAsignado;
                    List<ItemRutina> rutinaGenerada = EjecutarAlgoritmoComplejo(codigoEmpleado, turnoActual, out nombreGrupoAsignado);

                    // 4. LLENADO DE ENCABEZADO Y REPEATER

                    // Descomenta y ajusta estos IDs según tu ASPX

                    lblname.Text = nombreEmpleado;
                    lblfecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    lblturno.Text = turnoActual;
                    //lblGrupoAsignado.Text = nombreGrupoAsignado;


                    // Llenar el Repeater
                    rptRutina.DataSource = rutinaGenerada;
                    rptRutina.DataBind();

                    // --- Dentro del Page_Load de la página de Impresión/PDF ---

                    // 1. Definimos la URL de nuestra página de servicio para cerrar la sesión
                    // Usamos ResolveClientUrl para garantizar la ruta correcta
                    string logoutUrl = ResolveClientUrl("~/closesession.aspx");

                    // 2. Definimos el script que se ejecutará en el cliente (navegador)
                    string script = $@"
<script type='text/javascript'>
    
    // Función que se llama cuando el usuario cierra el diálogo de impresión
    function cerrarSesionAlTerminarImpresion() {{
        // Redirigimos la ventana actual a la página de servicio.
        // Esto asegura que la autenticación (FormsAuth) y las variables de Session sean destruidas.
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
    
    window.onload = function() {{
        // Lógica existente para la impresión
        var originalTitle = document.title;
        document.title = '';
        
        // Abre el diálogo de impresión
        window.print();
        
        // Restaura el título (lo hace después de que el navegador inicia la impresión)
        document.title = originalTitle;
    }};
</script>";

                    // 3. Registramos el script en el cliente
                    this.ClientScript.RegisterClientScriptBlock(this.GetType(), "ImprimirCerrarSesion", script);

                    // -----------------------------------------------------------
                    // IMPORTANTE: Si estás usando Response.End() o Server.Transfer(), 
                    // asegúrate de que el código de impresión esté en un bloque que 
                    // permita que el ResponseBuffer se complete antes de la redirección.
                    // -----------------------------------------------------------
                }
            }
        }

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
            int puestoEfectivo = 0;

            // Sincronizado para que el 22 Feb 2026 sea Día 90 (Semana 12 Par)
            DateTime fechaInicioZafra = new DateTime(2025, 11, 24);
            int diaZafra = (ahora - fechaInicioZafra).Days + 1;
            bool esSemanaPar = ((diaZafra / 7) % 2 == 0);

            var empleado = ConsultarDatosEmpleado(codigoEmpleado);
            bool esCuartoTurno = empleado.EsCuartoTurno;
            int puestoFijo = empleado.PuestoFijo;

            if (esCuartoTurno)
            {
                DayOfWeek dia = ahora.DayOfWeek;
                int h = ahora.Hour;
                int m = ahora.Minute;

                // Martes a Viernes
                if (dia == DayOfWeek.Tuesday || dia == DayOfWeek.Thursday) puestoEfectivo = 1;
                else if (dia == DayOfWeek.Wednesday || dia == DayOfWeek.Friday) puestoEfectivo = 2;

                // DOMINGO NOCHE (Inicia 21:41)
                else if ((dia == DayOfWeek.Sunday && (h > 21 || (h == 21 && m >= 41))) || (dia == DayOfWeek.Monday && h < 6))
                {
                    // Semana Par: A(1) hizo 12h, Relevo cubre a B(2).
                    puestoEfectivo = esSemanaPar ? 2 : 1;
                }
                // LUNES NOCHE (Inicia 21:41)
                else if ((dia == DayOfWeek.Monday && (h > 21 || (h == 21 && m >= 41))) || (dia == DayOfWeek.Tuesday && h < 6))
                {
                    // Relevo cubre al que hizo 12h el domingo (descansa lunes)
                    puestoEfectivo = esSemanaPar ? 1 : 2;
                }
            }
            else
            {
                puestoEfectivo = puestoFijo;
            }

            int diaJuliano = ahora.DayOfYear;
            return ((diaJuliano + puestoEfectivo) % 2 == 0) ? "Extracción" : "Alcalizado";
        }
        #endregion
        // -------------------------------------------------------------------------------------
        // MÉTODO COORDINADOR PRINCIPAL
        // -------------------------------------------------------------------------------------

        private List<ItemRutina> EjecutarAlgoritmoComplejo(string codigoEmpleado, string turnoActual, out string nombreGrupoAsignado)
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
            GuardarNuevaRutina(codigoEmpleado, turnoActual, idGrupoAsignado, instrumentosSeleccionados);

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
        // FASE 2: SELECCIÓN DE INSTRUMENTOS (3x Área: 1 Alta, 1 Media, 1 Baja)
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
            int indiceBloque = 0;
            if (turnoActual.Contains("14:00") || turnoActual.Contains("18:00 A 22:00")) indiceBloque = 1;
            else if (turnoActual.Contains("22:00") || turnoActual.Contains("18:00 A 06:00")) indiceBloque = 2;

            // 3. Calcular rango (Ej: 12 áreas / 3 turnos = 4 áreas por turno)
            int areasPorTurno = totalAreas / 3;
            int inicio = indiceBloque * areasPorTurno;

            // Ajuste para el último turno por si la división no es exacta
            int fin = (indiceBloque == 2) ? totalAreas : inicio + areasPorTurno;

            // 4. Tomar solo el subconjunto de áreas que corresponden a este turno
            List<int> areasFiltradas = todasLasAreas.GetRange(inicio, fin - inicio);

            // 5. Generar instrumentos solo para esas áreas
            foreach (int idArea in areasFiltradas)
            {
                var alta = ObtenerInstrumentosPorAreaPrioridad(idArea, "Alta", 1, tagsExcluidos);
                rutina.AddRange(alta);
                tagsExcluidos.AddRange(alta.Select(i => i.TAG).ToList());

                var media = ObtenerInstrumentosPorAreaPrioridad(idArea, "Media", 1, tagsExcluidos);
                rutina.AddRange(media);
                tagsExcluidos.AddRange(media.Select(i => i.TAG).ToList());

                var baja = ObtenerInstrumentosPorAreaPrioridad(idArea, "Baja", 1, tagsExcluidos);
                rutina.AddRange(baja);
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

        // --- MÉTODO MODIFICADO: Selecciona un equipo por Área y Prioridad ---
        private List<ItemRutina> ObtenerInstrumentosPorAreaPrioridad(int idArea, string prioridadNombre, int cantidad, List<string> tagsExcluidos)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            DataTable dt = new DataTable();

            string tagsYaSeleccionados = tagsExcluidos.Any() ?
                "AND I.TAG NOT IN ('" + string.Join("','", tagsExcluidos) + "')" :
                string.Empty;

            // Consulta SQL: El filtro se aplica directamente al IDarea
            string sql = $@"
            SELECT TOP {cantidad} 
                I.TAG, 
                I.Nombre AS NombreInstrumento, 
                I.Actividad, 
                A.Nombre AS NombreArea
            FROM Instrumentos I
            INNER JOIN Area A ON I.IDarea = A.IDarea
            WHERE 
                I.IDarea = @IDArea -- << FILTRO POR EL ÁREA ESPECÍFICA
                AND I.IDprioridad = (SELECT IDprioridad FROM Prioridad WHERE Nombre = @PrioridadNombre)
                {tagsYaSeleccionados}
                AND I.TAG NOT IN (
                    SELECT RI.TAG
                    FROM Rutinas R
                    INNER JOIN Rutina_instrumento RI ON R.Correlativo = RI.Correlativo
                    WHERE R.Fecha >= DATEADD(hour, -24, GETDATE()) 
                )
            ORDER BY NEWID()"; // La aleatoriedad se aplica al equipo dentro de esta Area/Prioridad

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDArea", idArea); // NUEVO PARÁMETRO
                cmd.Parameters.AddWithValue("@PrioridadNombre", prioridadNombre);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ItemRutina
                {
                    NombreArea = row["NombreArea"].ToString(),
                    TAG = row["TAG"].ToString(),
                    NombreInstrumento = row["NombreInstrumento"].ToString(),
                    Actividad = row["Actividad"].ToString()
                });
            }
            return lista;
        }
        #endregion
        #region fase3
        // ----------------------------------------------------
        // FASE 3: GUARDADO DE LA RUTINA (Transacción)
        // ----------------------------------------------------
        private void GuardarNuevaRutina(string codigoEmpleado, string turnoActual, int idGrupo, List<ItemRutina> instrumentos)
        {
            if (instrumentos == null || instrumentos.Count == 0) return;

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

                    // 2. Insertar Detalle de Instrumentos
                    string sqlInsertDetalle = @"
                    INSERT INTO Rutina_instrumento (Correlativo, TAG)
                    VALUES (@Correlativo, @TAG);";

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
                    // Aquí deberías añadir un sistema de logging para ver los errores de base de datos
                }
            }
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

            // --- A. BUSCAR EL CORRELATIVO MÁS RECIENTE ---
            string sqlBuscarCorrelativo = @"
        SELECT TOP 1 
            Correlativo, 
            CONVERT(VARCHAR, Fecha, 120) AS FechaRutina -- Formato ISO 8601 para fácil manejo
        FROM Rutinas
        WHERE 
            Codigo_empleado = @CodigoEmpleado 
            AND Turno = @TurnoActual
        ORDER BY Correlativo DESC";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlBuscarCorrelativo, conn);
                cmd.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                cmd.Parameters.AddWithValue("@TurnoActual", turnoActual);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    correlativoUltimaRutina = reader.GetInt32(0); // Columna Correlativo
                    fechaRutinaGuardada = reader.GetString(1); // Columna FechaRutina
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
            string sqlCargarDetalle = @"
        SELECT 
            A.Nombre AS NombreArea, 
            I.TAG, 
            I.Nombre AS NombreInstrumento, 
            I.Actividad
        FROM Rutina_instrumento RI
        INNER JOIN Instrumentos I ON RI.TAG = I.TAG
        INNER JOIN Area A ON I.IDarea = A.IDarea
        WHERE 
            RI.Correlativo = @Correlativo
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

            // 1. Hora Correcta: Usamos la hora guardada en la BD
            lblname.Text = nombreEmpleado;
            lblfecha.Text = fechaRutinaGuardada; // ¡CORRECCIÓN APLICADA!
            lblturno.Text = turnoActual;

            // 2. Repeater Completo: Llenamos con el DataTable completo de detalles
            rptRutina.DataSource = dt;
            rptRutina.DataBind();
            rptRutina.Visible = true;

            // --- 3. IMPRESIÓN AUTOMÁTICA ---
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
    }
}
