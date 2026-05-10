using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClosedXML.Excel;

namespace Rutinas
{
    public partial class DashboardDesmontaje : System.Web.UI.Page
    {
        // ── Cadenas de conexión ─────────────────────────────────────────────
        private readonly string ConnReportes =
            WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        private readonly string ConnVinetas =
            WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;

        // ── Clase interna para datos cruzados ───────────────────────────────
        private class EquipoAvance
        {
            public string TAG            { get; set; }
            public string Descripcion    { get; set; }
            public string Area           { get; set; }
            public string NombreEmpleado { get; set; }
            public bool   Desmontado     { get; set; }
            public DateTime? FechaDeclaracion { get; set; }
        }

        // ════════════════════════════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarAreasFiltro();
            }

            CargarMetricasGlobales();
            CargarTablaEquipos();
            CargarAvancePorArea();
        }

        // ════════════════════════════════════════════════════════════════════
        // CARGAR ÁREAS EN EL DROPDOWNLIST
        // ════════════════════════════════════════════════════════════════════
        private void CargarAreasFiltro()
        {
            ddlAreaFiltro.Items.Clear();
            ddlAreaFiltro.Items.Add(new ListItem("-- Todas las áreas --", ""));

            try
            {
                const string sql = @"SELECT DISTINCT RTRIM(Area) AS Area
                                     FROM equipos
                                     WHERE Area IS NOT NULL AND RTRIM(Area) <> ''
                                     ORDER BY Area";

                using (SqlConnection conn = new SqlConnection(ConnVinetas))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string area = dr.GetString(0);
                            ddlAreaFiltro.Items.Add(new ListItem(area, area));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar areas: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // OBTENER DESMONTADOS (BD REPORTES) → Dictionary por TAG
        // ════════════════════════════════════════════════════════════════════
        private Dictionary<string, EquipoAvance> ObtenerDesmontados()
        {
            var dict = new Dictionary<string, EquipoAvance>(StringComparer.OrdinalIgnoreCase);

            try
            {
                const string sql = @"SELECT RTRIM(TAG) AS TAG,
                                            RTRIM(ISNULL(Descripcion,''))  AS Descripcion,
                                            RTRIM(ISNULL(Area,''))         AS Area,
                                            FechaDeclaracion,
                                            ISNULL(NombreEmpleado,'')      AS NombreEmpleado
                                     FROM DesmontajeAvance
                                     WHERE Desmontado = 1";

                using (SqlConnection conn = new SqlConnection(ConnReportes))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string tag = dr.GetString(0).Trim().ToUpper();
                            if (string.IsNullOrEmpty(tag)) continue;

                            DateTime? fecha = null;
                            if (!dr.IsDBNull(3))
                                fecha = dr.GetDateTime(3);

                            var ea = new EquipoAvance
                            {
                                TAG             = tag,
                                Descripcion     = dr.GetString(1),
                                Area            = dr.GetString(2),
                                FechaDeclaracion = fecha,
                                NombreEmpleado  = dr.GetString(4),
                                Desmontado      = true
                            };

                            // Si hay duplicados conserva el más reciente
                            if (!dict.ContainsKey(tag))
                                dict[tag] = ea;
                            else if (ea.FechaDeclaracion.HasValue &&
                                     (!dict[tag].FechaDeclaracion.HasValue ||
                                      ea.FechaDeclaracion > dict[tag].FechaDeclaracion))
                                dict[tag] = ea;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al obtener desmontados: " + ex.Message);
            }

            return dict;
        }

        // ════════════════════════════════════════════════════════════════════
        // OBTENER EQUIPOS DE VINETAS (con filtro opcional de área)
        // ════════════════════════════════════════════════════════════════════
        private List<EquipoAvance> ObtenerEquiposVinetas(string area)
        {
            var lista = new List<EquipoAvance>();

            try
            {
                string sql = @"SELECT RTRIM(TAG)         AS TAG,
                                      RTRIM(Descripcion) AS Descripcion,
                                      RTRIM(Area)        AS Area
                               FROM equipos
                               WHERE TAG IS NOT NULL AND RTRIM(TAG) <> ''";

                if (!string.IsNullOrEmpty(area))
                    sql += " AND RTRIM(Area) = @Area";

                sql += " ORDER BY TAG";

                using (SqlConnection conn = new SqlConnection(ConnVinetas))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(area))
                        cmd.Parameters.AddWithValue("@Area", area);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new EquipoAvance
                            {
                                TAG         = dr.IsDBNull(0) ? "" : dr.GetString(0).Trim(),
                                Descripcion = dr.IsDBNull(1) ? "" : dr.GetString(1).Trim(),
                                Area        = dr.IsDBNull(2) ? "" : dr.GetString(2).Trim()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al obtener equipos de Vinetas: " + ex.Message);
            }

            return lista;
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTRICAS GLOBALES (siempre sin filtro de área)
        // ════════════════════════════════════════════════════════════════════
        private void CargarMetricasGlobales()
        {
            try
            {
                List<EquipoAvance> todos      = ObtenerEquiposVinetas("");
                Dictionary<string, EquipoAvance> desmontados = ObtenerDesmontados();

                int total       = todos.Count;
                int desmCount   = todos.Count(e => desmontados.ContainsKey(e.TAG.ToUpper()));
                int pendCount   = total - desmCount;
                double pct      = total > 0 ? Math.Round((double)desmCount / total * 100.0, 1) : 0.0;

                lblTotal.Text       = total.ToString();
                lblDesmontados.Text = desmCount.ToString();
                lblPendientes.Text  = pendCount.ToString();
                lblPorcentaje.Text  = pct.ToString("0.0") + "%";
            }
            catch (Exception ex)
            {
                MostrarError("Error en metricas globales: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // TABLA DE EQUIPOS (con filtro de área si se seleccionó)
        // ════════════════════════════════════════════════════════════════════
        private void CargarTablaEquipos()
        {
            try
            {
                string areaFiltro = ddlAreaFiltro.SelectedValue;
                List<EquipoAvance> equipos    = ObtenerEquiposVinetas(areaFiltro);
                Dictionary<string, EquipoAvance> desmontados = ObtenerDesmontados();

                var pendientes   = new List<EquipoAvance>();
                var desmontadosList = new List<EquipoAvance>();

                foreach (var eq in equipos.OrderBy(e => e.TAG))
                {
                    string key = eq.TAG.ToUpper();
                    if (desmontados.TryGetValue(key, out EquipoAvance da))
                        desmontadosList.Add(new EquipoAvance {
                            TAG = eq.TAG, Descripcion = eq.Descripcion, Area = eq.Area,
                            Desmontado = true, FechaDeclaracion = da.FechaDeclaracion,
                            NombreEmpleado = da.NombreEmpleado });
                    else
                        pendientes.Add(new EquipoAvance {
                            TAG = eq.TAG, Descripcion = eq.Descripcion, Area = eq.Area,
                            Desmontado = false });
                }

                gvPendientes.DataSource = pendientes;
                gvPendientes.DataBind();
                lblContPendientes.Text = $"({pendientes.Count})";

                gvDesmontados.DataSource = desmontadosList;
                gvDesmontados.DataBind();
                lblContDesmontados.Text = $"({desmontadosList.Count})";
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar tabla de equipos: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // AVANCE POR ÁREA (siempre todos los equipos, independiente del filtro)
        // ════════════════════════════════════════════════════════════════════
        private void CargarAvancePorArea()
        {
            try
            {
                List<EquipoAvance> todos      = ObtenerEquiposVinetas("");
                Dictionary<string, EquipoAvance> desmontados = ObtenerDesmontados();

                // Agrupar por área
                var grupos = todos
                    .GroupBy(e => string.IsNullOrEmpty(e.Area) ? "(Sin área)" : e.Area)
                    .Select(g =>
                    {
                        int total    = g.Count();
                        int desm     = g.Count(e => desmontados.ContainsKey(e.TAG.ToUpper()));
                        int pend     = total - desm;
                        double avpct = total > 0 ? Math.Round((double)desm / total * 100.0, 1) : 0.0;
                        return new
                        {
                            Area        = g.Key,
                            Total       = total,
                            Desmontados = desm,
                            Pendientes  = pend,
                            AvancePct   = avpct
                        };
                    })
                    .OrderByDescending(x => x.AvancePct)
                    .ThenBy(x => x.Area)
                    .ToList();

                // Convertir a DataTable para DataBind (permite BoundField + TemplateField)
                DataTable dt = new DataTable();
                dt.Columns.Add("Area",        typeof(string));
                dt.Columns.Add("Total",       typeof(int));
                dt.Columns.Add("Desmontados", typeof(int));
                dt.Columns.Add("Pendientes",  typeof(int));
                dt.Columns.Add("AvancePct",   typeof(double));

                foreach (var g in grupos)
                    dt.Rows.Add(g.Area, g.Total, g.Desmontados, g.Pendientes, g.AvancePct);

                gvAvanceAreas.DataSource = dt;
                gvAvanceAreas.DataBind();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar avance por area: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ROW DATA BOUND — gvEquipos
        // ════════════════════════════════════════════════════════════════════
        protected void gvDesmontados_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            e.Row.Style["background-color"] = "rgba(46,125,50,0.08)";
        }

        // ════════════════════════════════════════════════════════════════════
        // ROW DATA BOUND — gvAvanceAreas (colorear fila según % avance)
        // ════════════════════════════════════════════════════════════════════
        protected void gvAvanceAreas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            double pct = Convert.ToDouble(drv["AvancePct"]);
            if (pct >= 100.0)
                e.Row.Style["background-color"] = "rgba(46,125,50,0.08)";
        }

        // ════════════════════════════════════════════════════════════════════
        // EVENTO: Cambio de área en filtro
        // ════════════════════════════════════════════════════════════════════
        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTablaEquipos();
            CargarAvancePorArea();
        }

        // ════════════════════════════════════════════════════════════════════
        // EVENTO: Exportar Excel (desmontados del área seleccionada o todos)
        // ════════════════════════════════════════════════════════════════════
        protected void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                string areaFiltro = ddlAreaFiltro.SelectedValue;

                List<EquipoAvance> equipos    = ObtenerEquiposVinetas(areaFiltro);
                Dictionary<string, EquipoAvance> desmontados = ObtenerDesmontados();

                // Solo los equipos de Vinetas que estén desmontados
                var exportar = equipos
                    .Where(eq => desmontados.ContainsKey(eq.TAG.ToUpper()))
                    .Select(eq =>
                    {
                        EquipoAvance da = desmontados[eq.TAG.ToUpper()];
                        return new
                        {
                            TAG              = eq.TAG,
                            Descripcion      = eq.Descripcion,
                            Area             = eq.Area,
                            FechaDeclaracion = da.FechaDeclaracion,
                            NombreEmpleado   = da.NombreEmpleado
                        };
                    })
                    .OrderBy(x => x.Area)
                    .ThenBy(x => x.TAG)
                    .ToList();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    IXLWorksheet ws = wb.Worksheets.Add("Desmontados");

                    // Encabezados
                    string[] headers = new string[]
                    {
                        "TAG", "Descripcion", "Area", "Fecha Declaracion", "Declarado Por"
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
                    for (int row = 0; row < exportar.Count; row++)
                    {
                        var item = exportar[row];
                        ws.Cell(row + 2, 1).Value = item.TAG;
                        ws.Cell(row + 2, 2).Value = item.Descripcion;
                        ws.Cell(row + 2, 3).Value = item.Area;
                        ws.Cell(row + 2, 4).Value = item.FechaDeclaracion.HasValue
                            ? item.FechaDeclaracion.Value.ToString("dd/MM/yyyy HH:mm")
                            : "";
                        ws.Cell(row + 2, 5).Value = item.NombreEmpleado;

                        // Filas alternas verde muy claro
                        if (row % 2 == 1)
                            ws.Row(row + 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
                    }

                    // Ajustar columnas
                    ws.Columns().AdjustToContents();
                    ws.SheetView.FreezeRows(1);

                    // Nombre de archivo
                    string sufijo = string.IsNullOrEmpty(areaFiltro)
                        ? "todas_areas"
                        : areaFiltro.Replace(" ", "_").Replace("/", "-");

                    string fileName = string.Format(
                        "Desmontados_{0}_{1:yyyyMMdd_HHmm}.xlsx",
                        sufijo, DateTime.Now);

                    Response.Clear();
                    Response.ContentType =
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
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
                if (ex is ThreadAbortException) return;
                MostrarError("Error al exportar: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // EVENTO: Salir
        // ════════════════════════════════════════════════════════════════════
        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }

        // ════════════════════════════════════════════════════════════════════
        // HELPER: mostrar error en consola del navegador
        // ════════════════════════════════════════════════════════════════════
        private void MostrarError(string mensaje)
        {
            string safe = mensaje
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "")
                .Replace("\n", " ");
            ClientScript.RegisterStartupScript(
                GetType(),
                "ddbError_" + Guid.NewGuid().ToString("N"),
                string.Format("console.error('DashboardDesmontaje: {0}');", safe),
                true);
        }
    }
}
