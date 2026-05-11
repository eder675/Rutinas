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

                int total     = todos.Count;
                int desmCount = todos.Count(e => desmontados.ContainsKey(e.TAG.ToUpper()));
                int pendCount = total - desmCount;
                double pct    = total > 0 ? Math.Round((double)desmCount / total * 100.0, 1) : 0.0;

                lblTotal.Text       = total.ToString();
                lblDesmontados.Text = desmCount.ToString();
                lblPendientes.Text  = pendCount.ToString();
                lblPorcentaje.Text  = pct.ToString("0.0") + "%";

                // Delta: nuevos declarados hoy (desde medianoche)
                int deltaHoy = 0;
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(DISTINCT TAG) FROM DesmontajeAvance
                          WHERE Desmontado = 1
                            AND FechaDeclaracion >= CAST(GETDATE() AS DATE)", conn);
                    deltaHoy = (int)cmd.ExecuteScalar();
                }

                if (deltaHoy > 0)
                {
                    double deltaPct = total > 0 ? Math.Round((double)deltaHoy / total * 100.0, 1) : 0.0;

                    lblDeltaDia.Text    = "+" + deltaHoy + " hoy";
                    lblDeltaDia.Visible = true;

                    lblDeltaPct.Text    = "+" + deltaPct.ToString("0.0") + "% hoy";
                    lblDeltaPct.Visible = true;
                }
                else
                {
                    lblDeltaDia.Visible = false;
                    lblDeltaPct.Visible = false;
                }
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

                // Calcular avance por área (todos los equipos de Vinetas, sin filtro)
                List<EquipoAvance> todosEquipos = string.IsNullOrEmpty(areaFiltro)
                    ? equipos
                    : ObtenerEquiposVinetas("");

                var avancePorArea = todosEquipos
                    .GroupBy(eq => eq.Area ?? "Sin área")
                    .Select(g =>
                    {
                        int total = g.Count();
                        int desm  = g.Count(eq => desmontados.ContainsKey(eq.TAG.ToUpper()));
                        double pct = total > 0 ? Math.Round(desm * 100.0 / total, 1) : 0;
                        return new { Area = g.Key, Total = total, Desmontados = desm,
                                     Pendientes = total - desm, Avance = pct };
                    })
                    .OrderByDescending(x => x.Avance)
                    .ThenBy(x => x.Area)
                    .ToList();

                using (XLWorkbook wb = new XLWorkbook())
                {
                    // ── HOJA 1: Equipos desmontados ──────────────────────────
                    IXLWorksheet ws = wb.Worksheets.Add("Desmontados");
                    ws.TabColor = XLColor.FromHtml("#2E7D32");

                    // Título
                    ws.Cell(1, 1).Value = "REGISTRO DE EQUIPOS DESMONTADOS";
                    ws.Range(1, 1, 1, 5).Merge();
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;
                    ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1B5E20");
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(1).Height = 28;

                    // Subtítulo con fecha
                    ws.Cell(2, 1).Value = "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    ws.Range(2, 1, 2, 5).Merge();
                    ws.Cell(2, 1).Style.Font.Italic = true;
                    ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#555555");
                    ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Encabezados columnas (fila 3)
                    string[] h1 = { "TAG", "Descripción", "Área", "Fecha Declaración", "Declarado Por" };
                    for (int col = 0; col < h1.Length; col++)
                    {
                        IXLCell cell = ws.Cell(3, col + 1);
                        cell.Value = h1[col];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontSize = 11;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E7D32");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.White;
                    }
                    ws.Row(3).Height = 20;

                    // Datos desde fila 4
                    for (int row = 0; row < exportar.Count; row++)
                    {
                        var item = exportar[row];
                        int xlRow = row + 4;
                        ws.Cell(xlRow, 1).Value = item.TAG;
                        ws.Cell(xlRow, 2).Value = item.Descripcion;
                        ws.Cell(xlRow, 3).Value = item.Area;
                        ws.Cell(xlRow, 4).Value = item.FechaDeclaracion.HasValue
                            ? item.FechaDeclaracion.Value.ToString("dd/MM/yyyy HH:mm") : "";
                        ws.Cell(xlRow, 5).Value = item.NombreEmpleado;

                        // Alineación
                        ws.Cell(xlRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(xlRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Alternado
                        var rowStyle = ws.Row(xlRow).Style;
                        rowStyle.Fill.BackgroundColor = row % 2 == 0
                            ? XLColor.White : XLColor.FromHtml("#F1F8F1");

                        // Bordes
                        ws.Range(xlRow, 1, xlRow, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        ws.Range(xlRow, 1, xlRow, 5).Style.Border.OutsideBorderColor = XLColor.FromHtml("#C8E6C9");
                    }

                    // Total al pie
                    int totalRow1 = exportar.Count + 4;
                    ws.Cell(totalRow1, 1).Value = "TOTAL DESMONTADOS:";
                    ws.Cell(totalRow1, 1).Style.Font.Bold = true;
                    ws.Cell(totalRow1, 2).Value = exportar.Count;
                    ws.Cell(totalRow1, 2).Style.Font.Bold = true;
                    ws.Range(totalRow1, 1, totalRow1, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");

                    ws.Columns().AdjustToContents();
                    ws.Column(2).Width = Math.Min(ws.Column(2).Width, 50);
                    ws.SheetView.FreezeRows(3);
                    ws.RangeUsed().SetAutoFilter();

                    // ── HOJA 2: Avance por área ───────────────────────────────
                    IXLWorksheet ws2 = wb.Worksheets.Add("Avance por Area");
                    ws2.TabColor = XLColor.FromHtml("#1565C0");

                    // Título
                    ws2.Cell(1, 1).Value = "AVANCE DE DESMONTAJE POR ÁREA";
                    ws2.Range(1, 1, 1, 5).Merge();
                    ws2.Cell(1, 1).Style.Font.Bold = true;
                    ws2.Cell(1, 1).Style.Font.FontSize = 14;
                    ws2.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                    ws2.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#0D47A1");
                    ws2.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws2.Row(1).Height = 28;

                    ws2.Cell(2, 1).Value = "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    ws2.Range(2, 1, 2, 5).Merge();
                    ws2.Cell(2, 1).Style.Font.Italic = true;
                    ws2.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#555555");
                    ws2.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Encabezados columnas (fila 3)
                    string[] h2 = { "Área", "Total Equipos", "Desmontados", "Pendientes", "% Avance" };
                    for (int col = 0; col < h2.Length; col++)
                    {
                        IXLCell cell = ws2.Cell(3, col + 1);
                        cell.Value = h2[col];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.FontSize = 11;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.OutsideBorderColor = XLColor.White;
                    }
                    ws2.Row(3).Height = 20;

                    // Datos
                    for (int row = 0; row < avancePorArea.Count; row++)
                    {
                        var a = avancePorArea[row];
                        int xlRow = row + 4;

                        ws2.Cell(xlRow, 1).Value = a.Area;
                        ws2.Cell(xlRow, 2).Value = a.Total;
                        ws2.Cell(xlRow, 3).Value = a.Desmontados;
                        ws2.Cell(xlRow, 4).Value = a.Pendientes;
                        ws2.Cell(xlRow, 5).Value = a.Avance / 100.0;
                        ws2.Cell(xlRow, 5).Style.NumberFormat.Format = "0.0%";

                        // Alineación numérica
                        for (int c = 2; c <= 5; c++)
                            ws2.Cell(xlRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Paleta por nivel de avance — 0% sin color
                        XLColor rowColor;
                        if (a.Avance == 0)         rowColor = XLColor.White;               // sin avance
                        else if (a.Avance < 35)    rowColor = XLColor.FromHtml("#FFEBEE"); // rojo claro
                        else if (a.Avance < 70)    rowColor = XLColor.FromHtml("#FFF8E1"); // ámbar claro
                        else if (a.Avance < 100)   rowColor = XLColor.FromHtml("#E3F2FD"); // azul claro
                        else                        rowColor = XLColor.FromHtml("#E8F5E9"); // verde claro (100%)

                        ws2.Range(xlRow, 1, xlRow, 5).Style.Fill.BackgroundColor = rowColor;
                        ws2.Range(xlRow, 1, xlRow, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        ws2.Range(xlRow, 1, xlRow, 5).Style.Border.BottomBorderColor = XLColor.FromHtml("#DDDDDD");
                    }

                    // Fila de totales
                    int totalRow = avancePorArea.Count + 4;
                    int gtTotal  = avancePorArea.Sum(x => x.Total);
                    int gtDesm   = avancePorArea.Sum(x => x.Desmontados);
                    int gtPend   = avancePorArea.Sum(x => x.Pendientes);
                    double gtPct = gtTotal > 0 ? Math.Round(gtDesm * 100.0 / gtTotal, 1) : 0;

                    ws2.Cell(totalRow, 1).Value = "TOTAL GENERAL";
                    ws2.Cell(totalRow, 2).Value = gtTotal;
                    ws2.Cell(totalRow, 3).Value = gtDesm;
                    ws2.Cell(totalRow, 4).Value = gtPend;
                    ws2.Cell(totalRow, 5).Value = gtPct / 100.0;
                    ws2.Cell(totalRow, 5).Style.NumberFormat.Format = "0.0%";
                    ws2.Range(totalRow, 1, totalRow, 5).Style.Font.Bold = true;
                    ws2.Range(totalRow, 1, totalRow, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#0D47A1");
                    ws2.Range(totalRow, 1, totalRow, 5).Style.Font.FontColor = XLColor.White;
                    for (int c = 2; c <= 5; c++)
                        ws2.Cell(totalRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Barra de datos en columna % Avance (mini-barras visuales)
                    if (avancePorArea.Count > 0)
                    {
                        var rangoBarras = ws2.Range(ws2.Cell(4, 5), ws2.Cell(avancePorArea.Count + 3, 5));
                        rangoBarras.AddConditionalFormat().DataBar(XLColor.FromHtml("#1565C0"), true);
                    }

                    ws2.Columns().AdjustToContents();
                    ws2.Column(1).Width = Math.Min(ws2.Column(1).Width, 45);
                    ws2.SheetView.FreezeRows(3);
                    ws2.SheetView.FreezeColumns(1);

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
            string cargo = Session["Cargo"]?.ToString() ?? "";
            if (cargo.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                Response.Redirect("LoginDeveloper.aspx");
            else
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
