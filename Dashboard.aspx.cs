using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace Rutinas
{
    public partial class Dashboard : System.Web.UI.Page
    {
        // ── Cadena de conexión ──────────────────────────────────────────────
        private readonly string ConnString =
            WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        // ── Clave de ViewState para mantener el filtro activo ───────────────
        private const string VS_FILTRO = "FiltroNivel";

        // ────────────────────────────────────────────────────────────────────
        // PAGE LOAD
        // ────────────────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFiltroNivel();
                CargarMetricas();
                CargarTabla();
                RegistrarDatosGraficas();
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // MÉTRICAS
        // ────────────────────────────────────────────────────────────────────
        private void CargarMetricas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    // Total de fallos
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM RegistroFallos", conn))
                    {
                        lblTotalFallos.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Fallos inducidos
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM RegistroFallos WHERE FalloInducido = 1", conn))
                    {
                        lblInducidos.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }

                    // Fallos naturales
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM RegistroFallos WHERE FalloInducido = 0", conn))
                    {
                        lblNaturales.Text = cmd.ExecuteScalar()?.ToString() ?? "0";
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarErrorScript("Error al cargar metricas: " + ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // FILTRO DROPDOWNLIST — NivelDano
        // ────────────────────────────────────────────────────────────────────
        private void CargarFiltroNivel()
        {
            ddlFiltroNivel.Items.Clear();
            ddlFiltroNivel.Items.Add(new ListItem("-- Todos los niveles --", ""));

            try
            {
                string sql = @"SELECT DISTINCT NivelDano
                               FROM RegistroFallos
                               WHERE NivelDano IS NOT NULL AND NivelDano <> ''
                               ORDER BY NivelDano";

                using (SqlConnection conn = new SqlConnection(ConnString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string nivel = dr.GetString(0);
                            ddlFiltroNivel.Items.Add(new ListItem(nivel, nivel));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarErrorScript("Error al cargar filtro: " + ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // TABLA (con filtro opcional)
        // ────────────────────────────────────────────────────────────────────
        private void CargarTabla()
        {
            string filtroNivel = ddlFiltroNivel.SelectedValue;
            ViewState[VS_FILTRO] = filtroNivel;

            try
            {
                StringBuilder sql = new StringBuilder(
                    @"SELECT ID, Instrumentista, TAG, Instrumento,
                             FalloInducido, NivelDano, TipoDano,
                             PosibleCausa, Reemplaza, Interviene
                      FROM RegistroFallos");

                if (!string.IsNullOrEmpty(filtroNivel))
                    sql.Append(" WHERE NivelDano = @NivelDano");

                sql.Append(" ORDER BY ID DESC");

                using (SqlConnection conn = new SqlConnection(ConnString))
                using (SqlCommand cmd = new SqlCommand(sql.ToString(), conn))
                {
                    if (!string.IsNullOrEmpty(filtroNivel))
                        cmd.Parameters.AddWithValue("@NivelDano", filtroNivel);

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);

                    gvFallos.DataSource = dt;
                    gvFallos.DataBind();
                }
            }
            catch (Exception ex)
            {
                MostrarErrorScript("Error al cargar tabla: " + ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // DATOS PARA GRÁFICAS — JSON via ScriptManager
        // ────────────────────────────────────────────────────────────────────
        private void RegistrarDatosGraficas()
        {
            var jss = new JavaScriptSerializer();

            List<string> tipoLabels = new List<string>();
            List<int>    tipoValues = new List<int>();
            List<string> causaLabels = new List<string>();
            List<int>    causaValues = new List<int>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    // Gráfica 1: Dona — fallos por TipoDano
                    string sqlTipo = @"SELECT TipoDano, COUNT(*) AS Total
                                       FROM RegistroFallos
                                       WHERE TipoDano IS NOT NULL AND TipoDano <> ''
                                       GROUP BY TipoDano
                                       ORDER BY Total DESC";

                    using (SqlCommand cmd = new SqlCommand(sqlTipo, conn))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            tipoLabels.Add(dr.GetString(0));
                            tipoValues.Add(dr.GetInt32(1));
                        }
                    }

                    // Gráfica 2: Barras — fallos por PosibleCausa
                    string sqlCausa = @"SELECT PosibleCausa, COUNT(*) AS Total
                                        FROM RegistroFallos
                                        WHERE PosibleCausa IS NOT NULL AND PosibleCausa <> ''
                                        GROUP BY PosibleCausa
                                        ORDER BY Total DESC";

                    using (SqlCommand cmd = new SqlCommand(sqlCausa, conn))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            causaLabels.Add(dr.GetString(0));
                            causaValues.Add(dr.GetInt32(1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarErrorScript("Error al cargar datos de graficas: " + ex.Message);
            }

            // Serializar y registrar como variables JS globales
            string script = string.Format(
                "var _tipoDanoLabels = {0}; var _tipoDanoValues = {1}; var _causaLabels = {2}; var _causaValues = {3};",
                jss.Serialize(tipoLabels),
                jss.Serialize(tipoValues),
                jss.Serialize(causaLabels),
                jss.Serialize(causaValues)
            );

            ScriptManager.RegisterStartupScript(this, GetType(), "graficasData", script, true);
        }

        // ────────────────────────────────────────────────────────────────────
        // EVENTO: Cambio de filtro DropDownList
        // ────────────────────────────────────────────────────────────────────
        protected void ddlFiltroNivel_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTabla();
        }

        // ────────────────────────────────────────────────────────────────────
        // EVENTO: GridView RowDataBound (no se usa para transformaciones extra;
        //         los badges se manejan directamente en el ASPX con TemplateField)
        // ────────────────────────────────────────────────────────────────────
        protected void gvFallos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Reservado para extensiones futuras.
        }

        // ────────────────────────────────────────────────────────────────────
        // EVENTO: Exportar Excel con ClosedXML
        // ────────────────────────────────────────────────────────────────────
        protected void btnExportar_Click(object sender, EventArgs e)
        {
            string filtroNivel = ddlFiltroNivel.SelectedValue;

            try
            {
                // Obtener datos (todas las columnas incluyendo Comentarios y Solucion)
                StringBuilder sql = new StringBuilder(
                    @"SELECT ID, Instrumentista, TAG, Instrumento,
                             FalloInducido, NivelDano, TipoDano,
                             PosibleCausa, Comentarios,
                             Reemplaza, Interviene, Solucion
                      FROM RegistroFallos");

                if (!string.IsNullOrEmpty(filtroNivel))
                    sql.Append(" WHERE NivelDano = @NivelDano");

                sql.Append(" ORDER BY ID DESC");

                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(ConnString))
                using (SqlCommand cmd = new SqlCommand(sql.ToString(), conn))
                {
                    if (!string.IsNullOrEmpty(filtroNivel))
                        cmd.Parameters.AddWithValue("@NivelDano", filtroNivel);

                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);
                }

                // Construir libro Excel
                using (XLWorkbook wb = new XLWorkbook())
                {
                    IXLWorksheet ws = wb.Worksheets.Add("Registro de Fallos");

                    // Encabezados
                    string[] headers = new string[]
                    {
                        "ID", "Instrumentista", "TAG", "Instrumento",
                        "Fallo Inducido", "Nivel Daño", "Tipo Daño",
                        "Posible Causa", "Comentarios",
                        "Reemplaza", "Interviene", "Solución"
                    };

                    for (int col = 0; col < headers.Length; col++)
                    {
                        IXLCell cell = ws.Cell(1, col + 1);
                        cell.Value = headers[col];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E7D32");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // Filas de datos
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        DataRow dr = dt.Rows[row];
                        ws.Cell(row + 2, 1).Value  = (int)dr["ID"];
                        ws.Cell(row + 2, 2).Value  = dr["Instrumentista"]?.ToString() ?? "";
                        ws.Cell(row + 2, 3).Value  = dr["TAG"]?.ToString() ?? "";
                        ws.Cell(row + 2, 4).Value  = dr["Instrumento"]?.ToString() ?? "";
                        ws.Cell(row + 2, 5).Value  = Convert.ToBoolean(dr["FalloInducido"]) ? "SI" : "NO";
                        ws.Cell(row + 2, 6).Value  = dr["NivelDano"]?.ToString() ?? "";
                        ws.Cell(row + 2, 7).Value  = dr["TipoDano"]?.ToString() ?? "";
                        ws.Cell(row + 2, 8).Value  = dr["PosibleCausa"]?.ToString() ?? "";
                        ws.Cell(row + 2, 9).Value  = dr["Comentarios"]?.ToString() ?? "";
                        ws.Cell(row + 2, 10).Value = Convert.ToBoolean(dr["Reemplaza"]) ? "SI" : "NO";
                        ws.Cell(row + 2, 11).Value = Convert.ToBoolean(dr["Interviene"]) ? "SI" : "NO";
                        ws.Cell(row + 2, 12).Value = dr["Solucion"]?.ToString() ?? "";

                        // Alternar colores de fila
                        if (row % 2 == 1)
                        {
                            ws.Row(row + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F5F2");
                        }
                    }

                    // Autoajustar columnas
                    ws.Columns().AdjustToContents();

                    // Fijar encabezado
                    ws.SheetView.FreezeRows(1);

                    // Nombre del archivo
                    string sufijo = string.IsNullOrEmpty(filtroNivel)
                        ? "todos"
                        : filtroNivel.Replace(" ", "_").Replace("/", "-");
                    string fileName = string.Format("RegistroFallos_{0}_{1:yyyyMMdd}.xlsx", sufijo, DateTime.Now);

                    // Enviar al cliente
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition",
                        "attachment; filename=\"" + fileName + "\"");

                    using (MemoryStream ms = new MemoryStream())
                    {
                        wb.SaveAs(ms);
                        Response.BinaryWrite(ms.ToArray());
                    }

                    Response.End();
                }
            }
            catch (Exception ex)
            {
                // Si Response.End lanza ThreadAbortException es normal; relanzar solo si es otra excepcion
                if (ex is System.Threading.ThreadAbortException)
                    return;

                MostrarErrorScript("Error al exportar: " + ex.Message);
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // EVENTO: Salir
        // ────────────────────────────────────────────────────────────────────
        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }

        // ────────────────────────────────────────────────────────────────────
        // HELPER: mostrar error vía alert JS
        // ────────────────────────────────────────────────────────────────────
        private void MostrarErrorScript(string mensaje)
        {
            string safe = mensaje
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", " ");
            ScriptManager.RegisterStartupScript(this, GetType(), "dbError_" + Guid.NewGuid().ToString("N"),
                string.Format("console.error('Dashboard: {0}');", safe), true);
        }
    }
}
