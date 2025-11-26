using Rutinas.Models; //para usar los modelos de datos en la carpeta Models
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Generadorrutinas : System.Web.UI.Page
    {
        private readonly string ConnString =WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // --- 1. DATOS DE ENTRADA SIMULADOS (Ajustar según tu logueo) ---
                string codigoEmpleado = "I101";

                // --- 2. PREPARACIÓN DE DATOS ---
                string nombreEmpleado = ObtenerNombreEmpleado(codigoEmpleado);
                string turnoActual = DeterminarTurnoActual();

                // --- 3. EJECUCIÓN DEL ALGORITMO COMPLEJO ---
                string nombreGrupoAsignado;
                List<ItemRutina> rutinaGenerada = EjecutarAlgoritmoComplejo(codigoEmpleado, turnoActual, out nombreGrupoAsignado);

                // --- 4. LLENADO DE ENCABEZADO Y REPEATER ---

                // Llenado de Labels (Asume que tienes lblInstrumentista, lblFecha, lblTurno, etc.)
                
                lblname.Text = nombreEmpleado;
                lblfecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                lblturno.Text = turnoActual;
                //lblGrupoAsignado.Text = nombreGrupoAsignado; 
                

                // Llenar el Repeater (la tabla de la rutina)
                rptRutina.DataSource = rutinaGenerada;
                rptRutina.DataBind();
            }
        }

        // El resto de los métodos deben ir aquí...

        #region metodos auxiliares
        private string DeterminarTurnoActual()
        {
            TimeSpan ahora = DateTime.Now.TimeOfDay;

            // Lógica de turnos: Mañana (06:00-14:00), Tarde (14:00-22:00), Noche (22:00-06:00)
            if (ahora >= new TimeSpan(6, 0, 0) && ahora < new TimeSpan(14, 0, 0))
                return "Mañana (06:00 - 14:00)";
            else if (ahora >= new TimeSpan(14, 0, 0) && ahora < new TimeSpan(22, 0, 0))
                return "Tarde (14:00 - 22:00)";
            else
                return "Noche (22:00 - 06:00)";
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

        // -------------------------------------------------------------------------------------
        // MÉTODO COORDINADOR PRINCIPAL
        // -------------------------------------------------------------------------------------
        private List<ItemRutina> EjecutarAlgoritmoComplejo(string codigoEmpleado, string turnoActual, out string nombreGrupoAsignado)
        {
            // 1. ROTACIÓN DE GRUPOS
            GrupoRotacionAsignado grupo = ObtenerGrupoRotacion();
            nombreGrupoAsignado = grupo.NombreGrupo;

            // 2. SELECCIÓN DE INSTRUMENTOS (5 Alta, 5 Media, 5 Baja)
            List<ItemRutina> instrumentosSeleccionados = SeleccionarInstrumentos(grupo.IdGrupo);

            // 3. GUARDADO DE LA RUTINA
            GuardarNuevaRutina(codigoEmpleado, turnoActual, grupo.IdGrupo, instrumentosSeleccionados);

            return instrumentosSeleccionados;
        }
        #endregion
        #region fase1
        // ----------------------------------------------------
        // FASE 1: ROTACIÓN DE GRUPOS (Lógica de Ciclo)
        // ----------------------------------------------------
        private GrupoRotacionAsignado ObtenerGrupoRotacion()
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
        }
        #endregion
        #region fase2
        // ----------------------------------------------------
        // FASE 2: SELECCIÓN DE INSTRUMENTOS (5-5-5 CON EXCLUSIÓN DE 24H)
        // ----------------------------------------------------
        private List<ItemRutina> SeleccionarInstrumentos(int idGrupo)
        {
            List<ItemRutina> rutina = new List<ItemRutina>();
            List<string> tagsExcluidos = new List<string>();

            // Llama a la lógica de selección tres veces para forzar el 5-5-5

            var alta = ObtenerInstrumentosPorPrioridad(idGrupo, "Alta", 5, tagsExcluidos);
            rutina.AddRange(alta);
            tagsExcluidos.AddRange(alta.Select(i => i.TAG).ToList());

            var media = ObtenerInstrumentosPorPrioridad(idGrupo, "Media", 5, tagsExcluidos);
            rutina.AddRange(media);
            tagsExcluidos.AddRange(media.Select(i => i.TAG).ToList());

            var baja = ObtenerInstrumentosPorPrioridad(idGrupo, "Baja", 5, tagsExcluidos);
            rutina.AddRange(baja);

            // NOTA: Si una prioridad no tiene 5 instrumentos disponibles, devolverá menos.
            return rutina;
        }

        private List<ItemRutina> ObtenerInstrumentosPorPrioridad(int idGrupo, string prioridadNombre, int cantidad, List<string> tagsExcluidos)
        {
            List<ItemRutina> lista = new List<ItemRutina>();
            DataTable dt = new DataTable();

            string tagsYaSeleccionados = tagsExcluidos.Any() ?
                "AND I.TAG NOT IN ('" + string.Join("','", tagsExcluidos) + "')" :
                string.Empty;

            // Consulta SQL con la lógica de 24h
            string sql = $@"
            SELECT TOP {cantidad} 
                I.TAG, 
                I.Nombre AS NombreInstrumento, 
                I.Actividad, 
                A.Nombre AS NombreArea
            FROM Instrumentos I
            INNER JOIN Area A ON I.IDarea = A.IDarea
            WHERE 
                A.IDgrupo = @IDGrupo
                AND I.IDprioridad = (SELECT IDprioridad FROM Prioridad WHERE Nombre = @PrioridadNombre)
                {tagsYaSeleccionados}
                AND I.TAG NOT IN (
                    SELECT RI.TAG
                    FROM Rutinas R
                    INNER JOIN Rutina_instrumento RI ON R.Correlativo = RI.Correlativo
                    -- REGLA CRÍTICA: Excluir si ya se usó en las últimas 24 horas
                    WHERE R.Fecha >= DATEADD(hour, -24, GETDATE()) 
                )
            ORDER BY NEWID()"; // Selección aleatoria

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDGrupo", idGrupo);
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
    }
    #endregion
}
