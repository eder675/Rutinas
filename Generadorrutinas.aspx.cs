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

                    // Registra un script para el cliente que llama a la función de impresión
                    string script = @"
            <script type='text/javascript'>
                window.onload = function() { 
                    // 1. Guardar el título original
                    var originalTitle = document.title;
                    
                    // 2. Establecer el título como vacío (el navegador lo usa para el encabezado)
                    document.title = '';
                    
                    // 3. Abrir la ventana de impresión
                    window.print(); 
                    
                    // 4. Restaurar el título original después de que se inicia la impresión
                    document.title = originalTitle;
                };
            </script>";
                    this.ClientScript.RegisterClientScriptBlock(this.GetType(), "ImprimirRutina", script);
                }
            }
        }

        // El resto de los métodos deben ir aquí...

        #region metodos auxiliares
        private string DeterminarTurnoActual()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;

            // Lógica de turnos: Mañana (06:00-14:00), Tarde (14:00-22:00), Noche (22:00-06:00)
            if (ahora >= new TimeSpan(6, 0, 0) && ahora < new TimeSpan(14, 0, 0))
                return "06:00 A 14:00";
            else if (ahora >= new TimeSpan(14, 0, 0) && ahora < new TimeSpan(22, 0, 0))
                return "14:00 A 22:00";
            else
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

        // --- NUEVO MÉTODO: Obtiene los IDarea del grupo asignado en orden geográfico ---
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
        // Importante: La función debe recibir el CodigoEmpleado aunque solo se use para el contexto.
        private int ObtenerSiguienteIDgrupo(string codigoEmpleado, string turnoActual)
        {
            // =========================================================================
            // PASO 1: CHEQUEO DE PAREJA (Rotación A vs B)
            // Busca si ya se generó una rutina en la última hora (es decir, si el compañero ya generó).
            // =========================================================================

            int grupoAsignadoReciente = 0;
            DateTime limiteTiempo = DateTime.Now.AddHours(-1);

            string sqlCheckRecent = @"
        SELECT TOP 1 IDgrupo 
        FROM Rutinas 
        WHERE Turno = @TurnoActual AND Fecha >= @LimiteTiempo
        ORDER BY Correlativo DESC";

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlCheckRecent, conn);
                cmd.Parameters.AddWithValue("@TurnoActual", turnoActual);
                cmd.Parameters.AddWithValue("@LimiteTiempo", limiteTiempo);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    grupoAsignadoReciente = Convert.ToInt32(result);
                }
            }

            // CASO B: Es el segundo instrumentista. Rotación de Pareja (A -> B).
            // Esta lógica no debe tocar el estado atómico.
            if (grupoAsignadoReciente != 0)
            {
                return (grupoAsignadoReciente == 1) ? 2 : 1;
            }

            // A partir de aquí, solo ejecuta el Instrumentista que genera PRIMERO en el turno (CASO A).

            // =========================================================================
            // PASO 2: ROTACIÓN DIARIA y CORRECCIÓN PERSONAL (Manejo del Estado Atómico)
            // =========================================================================

            int grupoAsignadoHoy;     // El grupo que se asignará al instrumentista actual
            int grupoQueToca;         // El grupo que el estado atómico dice que toca por turno (Memoria Global)
            int ultimoGrupoPersonal = 0; // El último grupo que tuvo este empleado (Memoria Personal)

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();

                // 1. OBTENER EL ÚLTIMO GRUPO PERSONAL DE AYER (Memoria de la Persona)
                // Filtro: Busca el último grupo que ESTE empleado tuvo ANTES de hoy.
                string sqlUltimoPersonal = @"
            SELECT TOP 1 IDgrupo 
            FROM Rutinas 
            WHERE 
                Codigo_empleado = @CodigoEmpleado 
                AND Turno = @TurnoActual -- Añadimos el turno para ser más específico
                AND CONVERT(DATE, Fecha) < CONVERT(DATE, GETDATE()) 
            ORDER BY Fecha DESC, Correlativo DESC";

                SqlCommand cmdPersonal = new SqlCommand(sqlUltimoPersonal, conn);
                cmdPersonal.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                cmdPersonal.Parameters.AddWithValue("@TurnoActual", turnoActual);
                object resultPersonal = cmdPersonal.ExecuteScalar();

                if (resultPersonal != null && resultPersonal != DBNull.Value)
                {
                    ultimoGrupoPersonal = Convert.ToInt32(resultPersonal);
                }

                // 2. LEER EL ESTADO ATÓMICO (Memoria del Turno)
                string sqlReadState = "SELECT Grupo_Siguiente FROM Configuracion_Rotacion WHERE Turno = @TurnoActual";
                SqlCommand cmdReadState = new SqlCommand(sqlReadState, conn);
                cmdReadState.Parameters.AddWithValue("@TurnoActual", turnoActual);

                object resultState = cmdReadState.ExecuteScalar();
                if (resultState != null && resultState != DBNull.Value)
                {
                    grupoQueToca = Convert.ToInt32(resultState);
                }
                else
                {
                    // Inicialización si es la primera vez que se usa el turno
                    grupoQueToca = 1;
                }

                grupoAsignadoHoy = grupoQueToca; // Asignación inicial basada en el estado

                // 3. APLICAR LA CORRECCIÓN PERSONAL (Evitar repetición G1->G1 o G2->G2)
                if (ultimoGrupoPersonal != 0 && ultimoGrupoPersonal == grupoAsignadoHoy)
                {
                    // ¡El grupo que toca hoy es el mismo que tuve ayer! Se fuerza la rotación.
                    grupoAsignadoHoy = (grupoAsignadoHoy == 1) ? 2 : 1;
                }

                // 4. ACTUALIZAR EL ESTADO para el siguiente ciclo de ROTACIÓN DIARIA
                // El nuevo estado debe ser el opuesto al grupo FINAL que se asignó hoy.
                int nuevoGrupoSiguiente = (grupoAsignadoHoy == 1) ? 2 : 1;

                string sqlUpdateState = $@"
            UPDATE Configuracion_Rotacion 
            SET Grupo_Siguiente = @NuevoGrupo 
            WHERE Turno = @TurnoActual;
            
            -- Si no existe el registro, lo insertamos (UPSERT)
            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO Configuracion_Rotacion (Turno, Grupo_Siguiente) 
                VALUES (@TurnoActual, @NuevoGrupo);
            END";

                SqlCommand cmdUpdate = new SqlCommand(sqlUpdateState, conn);
                cmdUpdate.Parameters.AddWithValue("@NuevoGrupo", nuevoGrupoSiguiente);
                cmdUpdate.Parameters.AddWithValue("@TurnoActual", turnoActual);
                cmdUpdate.ExecuteNonQuery();
            }

            return grupoAsignadoHoy;
        }
        // -------------------------------------------------------------------------------------
        // MÉTODO COORDINADOR PRINCIPAL
        // -------------------------------------------------------------------------------------
        /*private List<ItemRutina> EjecutarAlgoritmoComplejo(string codigoEmpleado, string turnoActual, out string nombreGrupoAsignado)
        {
            // 1. ROTACIÓN DE GRUPOS
            GrupoRotacionAsignado grupo = ObtenerGrupoRotacion();
            nombreGrupoAsignado = grupo.NombreGrupo;
            int idGrupoAsignado = ObtenerSiguienteIDgrupo(turnoActual);

            // 2. SELECCIÓN DE INSTRUMENTOS (5 Alta, 5 Media, 5 Baja)
            List<ItemRutina> instrumentosSeleccionados = SeleccionarInstrumentos(grupo.IdGrupo);

            // 3. GUARDADO DE LA RUTINA
            GuardarNuevaRutina(codigoEmpleado, turnoActual, grupo.IdGrupo, instrumentosSeleccionados);

            return instrumentosSeleccionados;
        }*/

        private List<ItemRutina> EjecutarAlgoritmoComplejo(string codigoEmpleado, string turnoActual, out string nombreGrupoAsignado)
        {
            // 1. ROTACIÓN DE GRUPOS CORREGIDA

            // Obtiene el ID del grupo que debe rotar (1 o 2), basado en la última rutina del turno.
            int idGrupoAsignado = ObtenerSiguienteIDgrupo(codigoEmpleado, turnoActual);

            // Asigna un nombre (solo para el encabezado del reporte si es necesario)
            nombreGrupoAsignado = (idGrupoAsignado == 1) ? "Grupo 1" : "Grupo 2";

            // 2. SELECCIÓN DE INSTRUMENTOS (5 Alta, 5 Media, 5 Baja)
            // Usamos el grupo que OBTUVIMOS DE LA ROTACIÓN (idGrupoAsignado)
            List<ItemRutina> instrumentosSeleccionados = SeleccionarInstrumentos(idGrupoAsignado);

            // 3. GUARDADO DE LA RUTINA
            // Usamos el grupo que OBTUVIMOS DE LA ROTACIÓN (idGrupoAsignado)
            GuardarNuevaRutina(codigoEmpleado, turnoActual, idGrupoAsignado, instrumentosSeleccionados);

            return instrumentosSeleccionados;
        }

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
        private List<ItemRutina> SeleccionarInstrumentos(int idGrupo)
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
        }

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
            string script = @"window.onload = function() { var originalTitle = document.title; document.title = ''; window.print(); document.title = originalTitle; };";
            ClientScript.RegisterStartupScript(this.GetType(), "ImprimirRutina", script, true);
        }
        #endregion
    }
}
