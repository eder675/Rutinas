using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class DashboardMantenimiento : System.Web.UI.Page
    {
        private readonly string ConnRutinas =
            WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        private readonly string ConnVinetas =
            WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;

        private class EmpleadoMmto
        {
            public string CodEmp       { get; set; }
            public string Nombre       { get; set; }
            public int    Hoy          { get; set; }
            public int    Semana       { get; set; }
            public int    Mes          { get; set; }
            public int    Anio         { get; set; }
            public int    Pendientes   { get; set; }  // -1 = sin área asignada
            public int    TotalArea    { get; set; }  // total equipos del área (-1 = sin área)
        }

        // ════════════════════════════════════════════════════════════════════
        // PAGE LOAD
        // ════════════════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Cargo"] == null || Session["Cargo"].ToString() != "Administrador")
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ViewState["SortField"] = "Semana";
                ViewState["SortDir"]   = "DESC";
                CargarFiltroEmpleados();
            }

            CargarMetricasGlobales();
            CargarRanking();

            if (IsPostBack && ViewState["DetailEmpleado"] != null)
            {
                CargarDetalleEmpleado(ViewState["DetailEmpleado"].ToString());
                pnlDetalle.Visible = true;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTRICAS GLOBALES
        // ════════════════════════════════════════════════════════════════════
        private void CargarMetricasGlobales()
        {
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();

                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM equipos WHERE (EnUSO=1 OR EnUSO IS NULL) AND (Hibernacion=0 OR Hibernacion IS NULL)", conn))
                        lblTotalEquipos.Text = cmd.ExecuteScalar().ToString();

                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(DISTINCT RTRIM(TAG)) FROM vinetas WHERE YEAR(Fecha)=2026", conn))
                        lblVinetas2026.Text = cmd.ExecuteScalar().ToString();

                    int total  = int.Parse(lblTotalEquipos.Text);
                    int v2026  = int.Parse(lblVinetas2026.Text);
                    double pct = total > 0 ? (double)v2026 / total * 100.0 : 0;
                    lblPorcentajeGlobal.Text = $"{pct:F1}%";
                    int barW = Math.Min((int)Math.Round(pct), 100);
                    litBarraGlobal.Text = $"<div class='ddm-progress-bar'><div class='ddm-progress-fill' style='width:{barW}%'></div></div>";

                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM vinetas WHERE Fecha = CAST(GETDATE() AS DATE)", conn))
                        lblHoy.Text = cmd.ExecuteScalar().ToString();

                    using (var cmd = new SqlCommand(
                        @"SELECT COUNT(*) FROM vinetas
                          WHERE Fecha >= DATEADD(day, 2-DATEPART(WEEKDAY,GETDATE()), CAST(GETDATE() AS DATE))
                            AND Fecha <= CAST(GETDATE() AS DATE)", conn))
                        lblSemana.Text = cmd.ExecuteScalar().ToString();
                }
            }
            catch { /* si Vinetas no responde, las tarjetas quedan en 0 */ }
        }

        // ════════════════════════════════════════════════════════════════════
        // FILTRO EMPLEADOS
        // ════════════════════════════════════════════════════════════════════
        private void CargarFiltroEmpleados()
        {
            ddlFiltroEmpleado.Items.Clear();
            ddlFiltroEmpleado.Items.Add(new ListItem("— Todos los empleados —", ""));

            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT ID, RTRIM(Instrumentistas) as Nombre FROM Instrumentistas ORDER BY Instrumentistas", conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            ddlFiltroEmpleado.Items.Add(
                                new ListItem(dr["Nombre"].ToString(), dr["ID"].ToString()));
                    }
                }
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════════════
        // RANKING DE EMPLEADOS
        // ════════════════════════════════════════════════════════════════════
        private void CargarRanking()
        {
            // 1. Instrumentistas
            var dtInst = new DataTable();
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    new SqlDataAdapter(
                        "SELECT ID, RTRIM(Instrumentistas) as Nombre, CodEmp FROM Instrumentistas", conn)
                        .Fill(dtInst);
                }
            }
            catch { }

            // 2. Conteos de viñetas por nombre (Realizo)
            var conteos = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT RTRIM(Realizo) as Nombre,
                            SUM(CASE WHEN Fecha = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) as Hoy,
                            SUM(CASE WHEN Fecha >= DATEADD(day, 2-DATEPART(WEEKDAY,GETDATE()),
                                                            CAST(GETDATE() AS DATE))
                                         AND Fecha <= CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) as Semana,
                            SUM(CASE WHEN YEAR(Fecha)=YEAR(GETDATE())
                                         AND MONTH(Fecha)=MONTH(GETDATE()) THEN 1 ELSE 0 END) as Mes,
                            SUM(CASE WHEN YEAR(Fecha)=2026 THEN 1 ELSE 0 END) as Anio
                        FROM vinetas
                        GROUP BY RTRIM(Realizo)";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string nombre = dr["Nombre"].ToString().Trim();
                            conteos[nombre] = new[]
                            {
                                Convert.ToInt32(dr["Hoy"]),
                                Convert.ToInt32(dr["Semana"]),
                                Convert.ToInt32(dr["Mes"]),
                                Convert.ToInt32(dr["Anio"])
                            };
                        }
                    }
                }
            }
            catch { }

            // 3. Keywords por CodEmp
            var keywords = new Dictionary<string, (List<string> inc, List<string> exc)>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnRutinas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT Codigo_empleado,
                               Keyword1,Keyword2,Keyword3,Keyword4,Keyword5,Keyword6,
                               Keyword7,Keyword8,Keyword9,Keyword10,Keyword11,
                               ExcludeKeyword1,ExcludeKeyword2,ExcludeKeyword3,
                               ExcludeKeyword4,ExcludeKeyword5,ExcludeKeyword6,
                               ExcludeKeyword7,ExcludeKeyword8,ExcludeKeyword9,
                               ExcludeKeyword10,ExcludeKeyword11
                        FROM MantenimientoEmpleadoArea";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string cod = dr["Codigo_empleado"].ToString();
                            var inc = new List<string>();
                            var exc = new List<string>();
                            for (int i = 1; i <= 11; i++)
                            {
                                string ki = dr[$"Keyword{i}"] == DBNull.Value ? "" : dr[$"Keyword{i}"].ToString().Trim();
                                string ke = dr[$"ExcludeKeyword{i}"] == DBNull.Value ? "" : dr[$"ExcludeKeyword{i}"].ToString().Trim();
                                if (ki.Length > 0) inc.Add(ki.ToUpper());
                                if (ke.Length > 0) exc.Add(ke.ToUpper());
                            }
                            keywords[cod] = (inc, exc);
                        }
                    }
                }
            }
            catch { }

            // 4. Áreas asignadas por CodEmp
            var areasEmp = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnRutinas))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Codigo_empleado, Area FROM MantenimientoEmpleadoAreaDetalle", conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string cod  = dr.GetString(0);
                            string area = dr.GetString(1);
                            if (!areasEmp.ContainsKey(cod))
                                areasEmp[cod] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            areasEmp[cod].Add(area);
                        }
                    }
                }
            }
            catch { }

            // 5. TAGs con viñeta en 2026
            var tagsConVineta = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT DISTINCT RTRIM(TAG) FROM vinetas WHERE YEAR(Fecha)=2026", conn))
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            tagsConVineta.Add(dr.GetString(0).Trim());
                }
            }
            catch { }

            // 6. Todos los equipos activos no hibernados (con área)
            var todosEquipos = new List<(string tag, string descUpper, string area)>();
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT RTRIM(TAG), UPPER(RTRIM(Descripcion)), ISNULL(RTRIM(Area),'')
                        FROM equipos
                        WHERE (EnUSO=1 OR EnUSO IS NULL) AND (Hibernacion=0 OR Hibernacion IS NULL)";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            todosEquipos.Add((dr.GetString(0).Trim(), dr.GetString(1).Trim(), dr.GetString(2).Trim()));
                }
            }
            catch { }

            // 7. Construir lista
            string filtroId = ddlFiltroEmpleado.SelectedValue;
            var lista = new List<EmpleadoMmto>();

            foreach (DataRow row in dtInst.Rows)
            {
                string instId = row["ID"].ToString();
                if (!string.IsNullOrEmpty(filtroId) && instId != filtroId)
                    continue;

                string nombre    = row["Nombre"].ToString().Trim();
                string codEmpStr = row["CodEmp"] == DBNull.Value ? "0" : row["CodEmp"].ToString().Trim();

                int[] c = conteos.ContainsKey(nombre) ? conteos[nombre] : new[] { 0, 0, 0, 0 };

                int pendientes = -1;
                int totalArea  = -1;

                if (codEmpStr != "0")
                {
                    bool tieneAreas    = areasEmp.ContainsKey(codEmpStr) && areasEmp[codEmpStr].Count > 0;
                    bool tieneKeywords = keywords.ContainsKey(codEmpStr)  && keywords[codEmpStr].inc.Count > 0;

                    if (tieneAreas || tieneKeywords)
                    {
                        var areas = tieneAreas ? areasEmp[codEmpStr] : new HashSet<string>();
                        var inc   = tieneKeywords ? keywords[codEmpStr].inc : new List<string>();
                        var exc   = keywords.ContainsKey(codEmpStr) ? keywords[codEmpStr].exc : new List<string>();

                        var enArea = todosEquipos
                            .Where(eq =>
                                (tieneAreas    && areas.Contains(eq.area)) ||
                                (tieneKeywords && inc.Any(k => eq.descUpper.Contains(k))))
                            .Where(eq => !exc.Any(k => eq.descUpper.Contains(k)))
                            .ToList();

                        totalArea  = enArea.Count;
                        pendientes = enArea.Count(eq => !tagsConVineta.Contains(eq.tag));
                    }
                }

                lista.Add(new EmpleadoMmto
                {
                    CodEmp     = codEmpStr,
                    Nombre     = nombre,
                    Hoy        = c[0],
                    Semana     = c[1],
                    Mes        = c[2],
                    Anio       = c[3],
                    Pendientes = pendientes,
                    TotalArea  = totalArea
                });
            }

            // 7. Ordenar
            string sf  = ViewState["SortField"]?.ToString() ?? "Semana";
            string sd  = ViewState["SortDir"]?.ToString()   ?? "DESC";
            bool   asc = sd == "ASC";

            if      (sf == "Hoy"    && !asc) lista = lista.OrderByDescending(x => x.Hoy).ToList();
            else if (sf == "Hoy"    &&  asc) lista = lista.OrderBy(x => x.Hoy).ToList();
            else if (sf == "Semana" && !asc) lista = lista.OrderByDescending(x => x.Semana).ToList();
            else if (sf == "Semana" &&  asc) lista = lista.OrderBy(x => x.Semana).ToList();
            else                             lista = lista.OrderByDescending(x => x.Semana).ToList();

            gvRanking.DataSource = lista;
            gvRanking.DataBind();

            // Marcar botón de ordenamiento activo
            btnSortMasSemana.CssClass   = (sf == "Semana" && !asc) ? "ddm-sort-btn ddm-sort-active" : "ddm-sort-btn";
            btnSortMenosSemana.CssClass = (sf == "Semana" &&  asc) ? "ddm-sort-btn ddm-sort-active" : "ddm-sort-btn";
            btnSortMasHoy.CssClass      = (sf == "Hoy"    && !asc) ? "ddm-sort-btn ddm-sort-active" : "ddm-sort-btn";
            btnSortMenosHoy.CssClass    = (sf == "Hoy"    &&  asc) ? "ddm-sort-btn ddm-sort-active" : "ddm-sort-btn";
        }

        // ════════════════════════════════════════════════════════════════════
        // ROW DATA BOUND — colorear celdas
        // ════════════════════════════════════════════════════════════════════
        protected void gvRanking_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var emp = (EmpleadoMmto)e.Row.DataItem;

            // Colorear Hoy
            var lblH = (Label)e.Row.FindControl("lblHoyCell");
            if (lblH != null) lblH.CssClass = emp.Hoy > 5 ? "ddm-num-alto" : emp.Hoy > 0 ? "ddm-num-medio" : "ddm-num-zero";

            // Colorear Semana
            var lblS = (Label)e.Row.FindControl("lblSemanaCell");
            if (lblS != null) lblS.CssClass = emp.Semana > 20 ? "ddm-num-alto" : emp.Semana > 5 ? "ddm-num-medio" : emp.Semana > 0 ? "ddm-num-bajo" : "ddm-num-zero";

            // Colorear Mes
            var lblM = (Label)e.Row.FindControl("lblMesCell");
            if (lblM != null) lblM.CssClass = emp.Mes > 50 ? "ddm-num-alto" : emp.Mes > 10 ? "ddm-num-medio" : emp.Mes > 0 ? "ddm-num-bajo" : "ddm-num-zero";

            // Colorear Año
            var lblA = (Label)e.Row.FindControl("lblAnioCell");
            if (lblA != null) lblA.CssClass = emp.Anio > 100 ? "ddm-num-alto" : emp.Anio > 30 ? "ddm-num-medio" : emp.Anio > 0 ? "ddm-num-bajo" : "ddm-num-zero";

            // Pendientes
            var lblP = (Label)e.Row.FindControl("lblPendientesCell");
            if (lblP != null)
            {
                if (emp.Pendientes < 0)
                {
                    lblP.Text     = "—";
                    lblP.CssClass = "ddm-sin-asignar";
                }
                else if (emp.TotalArea > 0)
                {
                    double pct = (double)(emp.TotalArea - emp.Pendientes) / emp.TotalArea * 100;
                    lblP.Text     = $"{emp.Pendientes} ({pct:F0}% completado)";
                    lblP.CssClass = emp.Pendientes > 100 ? "ddm-pendiente-alto"
                                  : emp.Pendientes > 20  ? "ddm-pendiente-medio"
                                  :                        "ddm-pendiente-bajo";
                }
                else
                {
                    lblP.Text     = emp.Pendientes.ToString();
                    lblP.CssClass = emp.Pendientes > 50 ? "ddm-pendiente-alto"
                                  : emp.Pendientes > 0  ? "ddm-pendiente-medio"
                                  :                       "ddm-pendiente-bajo";
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ROW COMMAND — abrir detalle
        // ════════════════════════════════════════════════════════════════════
        protected void gvRanking_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "VerDetalle") return;
            string nombre = e.CommandArgument.ToString();
            ViewState["DetailEmpleado"] = nombre;
            CargarDetalleEmpleado(nombre);
            pnlDetalle.Visible = true;
        }

        // ════════════════════════════════════════════════════════════════════
        // DETALLE DE EMPLEADO
        // ════════════════════════════════════════════════════════════════════
        private void CargarDetalleEmpleado(string nombreEmpleado)
        {
            lblDetalleNombre.Text = System.Web.HttpUtility.HtmlEncode(nombreEmpleado);

            // Viñetas recientes
            var dtVin = new DataTable();
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT TOP 50
                            RTRIM(TAG)          AS TAG,
                            RTRIM(Descripcion)  AS Descripcion,
                            Fecha,
                            RTRIM(Mantenimiento) AS Mantenimiento
                        FROM vinetas
                        WHERE RTRIM(Realizo) = @Nombre
                        ORDER BY Fecha DESC, Nvineta DESC";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Nombre", nombreEmpleado);
                    new SqlDataAdapter(cmd).Fill(dtVin);
                }
            }
            catch { }
            gvVinetas.DataSource = dtVin;
            gvVinetas.DataBind();

            // Obtener CodEmp via Instrumentistas
            string codEmp = "";
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT CodEmp FROM Instrumentistas WHERE RTRIM(Instrumentistas) = @Nombre", conn);
                    cmd.Parameters.AddWithValue("@Nombre", nombreEmpleado);
                    object val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value)
                        codEmp = val.ToString().Trim();
                }
            }
            catch { }

            if (string.IsNullOrEmpty(codEmp) || codEmp == "0")
            {
                litPendientes.Text = "<p class='ddm-no-data'>No hay código de empleado vinculado en Instrumentistas.</p>";
                return;
            }

            // Keywords del empleado
            var inc = new List<string>();
            var exc = new List<string>();
            try
            {
                using (var conn = new SqlConnection(ConnRutinas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT Keyword1,Keyword2,Keyword3,Keyword4,Keyword5,Keyword6,
                               Keyword7,Keyword8,Keyword9,Keyword10,Keyword11,
                               ExcludeKeyword1,ExcludeKeyword2,ExcludeKeyword3,
                               ExcludeKeyword4,ExcludeKeyword5,ExcludeKeyword6,
                               ExcludeKeyword7,ExcludeKeyword8,ExcludeKeyword9,
                               ExcludeKeyword10,ExcludeKeyword11
                        FROM MantenimientoEmpleadoArea WHERE Codigo_empleado = @Cod";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Cod", codEmp);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            for (int i = 1; i <= 11; i++)
                            {
                                string ki = dr[$"Keyword{i}"] == DBNull.Value ? "" : dr[$"Keyword{i}"].ToString().Trim();
                                string ke = dr[$"ExcludeKeyword{i}"] == DBNull.Value ? "" : dr[$"ExcludeKeyword{i}"].ToString().Trim();
                                if (ki.Length > 0) inc.Add(ki.ToUpper());
                                if (ke.Length > 0) exc.Add(ke.ToUpper());
                            }
                        }
                    }
                }
            }
            catch { }

            // Áreas asignadas al empleado
            var areasDetalle = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnRutinas))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT Area FROM MantenimientoEmpleadoAreaDetalle WHERE Codigo_empleado = @Cod", conn);
                    cmd.Parameters.AddWithValue("@Cod", codEmp);
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            areasDetalle.Add(dr.GetString(0));
                }
            }
            catch { }

            if (inc.Count == 0 && areasDetalle.Count == 0)
            {
                litPendientes.Text = "<p class='ddm-no-data'>No hay áreas ni palabras clave asignadas. Configúralas en la sección de administración.</p>";
                return;
            }

            // TAGs con viñeta en 2026
            var tagsConVin = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT DISTINCT RTRIM(TAG) FROM vinetas WHERE YEAR(Fecha)=2026", conn))
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            tagsConVin.Add(dr.GetString(0).Trim());
                }
            }
            catch { }

            // Equipos pendientes (área asignada OR keyword match, menos exclusiones)
            var pendientes = new List<(string tag, string desc, string area)>();
            try
            {
                using (var conn = new SqlConnection(ConnVinetas))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT RTRIM(TAG), RTRIM(Descripcion), ISNULL(RTRIM(Area),'') AS Area
                        FROM equipos
                        WHERE (EnUSO=1 OR EnUSO IS NULL) AND (Hibernacion=0 OR Hibernacion IS NULL)";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string tag    = dr.GetString(0).Trim();
                            string desc   = dr.GetString(1).Trim();
                            string area   = dr.GetString(2).Trim();
                            string descUp = desc.ToUpper();

                            bool porArea     = areasDetalle.Count > 0 && areasDetalle.Contains(area);
                            bool porKeyword  = inc.Count > 0 && inc.Any(k => descUp.Contains(k));
                            bool excluir     = exc.Any(k => descUp.Contains(k));

                            if ((porArea || porKeyword) && !excluir && !tagsConVin.Contains(tag))
                                pendientes.Add((tag, desc, area));
                        }
                    }
                }
            }
            catch { }

            if (pendientes.Count == 0)
            {
                litPendientes.Text = "<p class='ddm-ok'>&#10003; Sin pendientes — todos los equipos del área tienen viñeta en 2026.</p>";
                return;
            }

            var sb = new StringBuilder();
            sb.Append($"<p class='ddm-pending-count'>{pendientes.Count} equipos sin viñeta en 2026</p>");
            sb.Append("<div class='ddm-table-wrap'>");
            sb.Append("<table class='ddm-grid'><thead><tr><th>TAG</th><th>Descripción</th><th>Área</th></tr></thead><tbody>");
            foreach (var (tag, desc, area) in pendientes.OrderBy(x => x.area).ThenBy(x => x.desc))
            {
                sb.Append("<tr>");
                sb.Append($"<td>{System.Web.HttpUtility.HtmlEncode(tag)}</td>");
                sb.Append($"<td>{System.Web.HttpUtility.HtmlEncode(desc)}</td>");
                sb.Append($"<td>{System.Web.HttpUtility.HtmlEncode(area)}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div>");
            litPendientes.Text = sb.ToString();
        }

        // ════════════════════════════════════════════════════════════════════
        // EVENTOS DE BOTONES
        // ════════════════════════════════════════════════════════════════════
        protected void btnSortMasSemana_Click(object sender, EventArgs e)
        {
            ViewState["SortField"] = "Semana";
            ViewState["SortDir"]   = "DESC";
        }
        protected void btnSortMenosSemana_Click(object sender, EventArgs e)
        {
            ViewState["SortField"] = "Semana";
            ViewState["SortDir"]   = "ASC";
        }
        protected void btnSortMasHoy_Click(object sender, EventArgs e)
        {
            ViewState["SortField"] = "Hoy";
            ViewState["SortDir"]   = "DESC";
        }
        protected void btnSortMenosHoy_Click(object sender, EventArgs e)
        {
            ViewState["SortField"] = "Hoy";
            ViewState["SortDir"]   = "ASC";
        }
        protected void ddlFiltroEmpleado_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewState["DetailEmpleado"] = null;
            pnlDetalle.Visible = false;
        }
        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            ViewState["DetailEmpleado"] = null;
            pnlDetalle.Visible = false;
        }
        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Response.Redirect("LoginDeveloper.aspx");
        }
    }
}
