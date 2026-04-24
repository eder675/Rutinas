using Rutinas.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class RegistroDesmontaje : System.Web.UI.Page
    {
        private readonly string ConnString        = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        private readonly string VinetasConnString = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["CodigoEmpleado"] == null || Session["NombreEmpleado"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblNombreEmpleado.Text = Session["NombreEmpleado"].ToString();
                lblFechaHoy.Text       = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                string codigoEmpleado = Session["CodigoEmpleado"].ToString();
                int rutinaId = 0;

                string qsRutina = Request.QueryString["RutinaId"];
                if (!string.IsNullOrEmpty(qsRutina))
                    int.TryParse(qsRutina, out rutinaId);

                if (rutinaId <= 0)
                    rutinaId = ObtenerUltimaRutinaConPendiente(codigoEmpleado);

                if (rutinaId > 0)
                {
                    ViewState["RutinaId"] = rutinaId;
                    List<ItemDesmontajePendiente> pendientes = CargarPendientes(rutinaId);
                    if (pendientes.Count > 0)
                    {
                        rptPendientes.DataSource = pendientes;
                        rptPendientes.DataBind();
                    }
                    else
                    {
                        rptPendientes.Visible     = false;
                        lblSinPendientes.Text     = "No hay equipos pendientes de reporte para esta rutina.";
                        lblSinPendientes.Visible  = true;
                    }
                }
                else
                {
                    rptPendientes.Visible    = false;
                    lblSinPendientes.Text    = "No se encontró rutina con desmontaje pendiente.";
                    lblSinPendientes.Visible = true;
                }
            }
        }

        private int ObtenerUltimaRutinaConPendiente(string codigoEmpleado)
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

        private List<ItemDesmontajePendiente> CargarPendientes(int rutinaId)
        {
            // 1. TAGs pendientes de reportar en esta rutina
            var tags = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT TAG FROM Rutina_desmontaje WHERE RutinaId = @RutinaId AND Reportado = 0", conn);
                cmd.Parameters.AddWithValue("@RutinaId", rutinaId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) tags.Add(dr["TAG"].ToString());
            }
            if (tags.Count == 0) return new List<ItemDesmontajePendiente>();

            // 2. Descripciones desde Vinetas
            var descripcionesPorTag = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection conn = new SqlConnection(VinetasConnString))
            {
                conn.Open();
                var parametros = tags.Select((t, i) => "@T" + i).ToList();
                SqlCommand cmd = new SqlCommand(
                    $"SELECT TAG, Descripcion FROM equipos WHERE TAG IN ({string.Join(",", parametros)})", conn);
                for (int i = 0; i < tags.Count; i++)
                    cmd.Parameters.AddWithValue("@T" + i, tags[i]);
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read())
                        descripcionesPorTag[dr["TAG"].ToString()] = dr["Descripcion"].ToString();
            }

            return tags.Select(tag => new ItemDesmontajePendiente
            {
                DesmontajeInstrumentoId = 0,
                RutinaId               = rutinaId,
                TAG                    = tag,
                NombreInstrumento      = descripcionesPorTag.TryGetValue(tag, out string desc) ? desc : tag,
                NombreArea             = string.Empty
            }).ToList();
        }

        protected void rptPendientes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Nada adicional en DataBind — los controles se manejan desde JS
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int rutinaId = ViewState["RutinaId"] != null ? Convert.ToInt32(ViewState["RutinaId"]) : 0;
            if (rutinaId <= 0) { Response.Redirect("Generadorrutinas.aspx"); return; }

            string codigoEmpleado = Session["CodigoEmpleado"].ToString();

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();

                foreach (RepeaterItem item in rptPendientes.Items)
                {
                    if (item.ItemType != ListItemType.Item &&
                        item.ItemType != ListItemType.AlternatingItem)
                        continue;

                    HiddenField   hfTag   = (HiddenField)item.FindControl("hfTag");
                    HiddenField   hfId    = (HiddenField)item.FindControl("hfDesmontajeId");
                    CheckBox      chk     = (CheckBox)item.FindControl("chkDesmontado");
                    DropDownList  ddl     = (DropDownList)item.FindControl("ddlRazon");
                    TextBox       txt     = (TextBox)item.FindControl("txtDetalle");

                    string tag                  = hfTag.Value;
                    int    desmontajeInstId      = Convert.ToInt32(hfId.Value);
                    bool   desmontado           = chk.Checked;
                    string razonSeleccionada    = ddl.SelectedValue;
                    string detalleAdicional     = txt.Text.Trim();

                    // Construir razón combinada
                    string razonFinal = null;
                    if (!desmontado)
                    {
                        if (!string.IsNullOrEmpty(razonSeleccionada))
                            razonFinal = string.IsNullOrEmpty(detalleAdicional)
                                ? razonSeleccionada
                                : razonSeleccionada + ": " + detalleAdicional;
                        else if (!string.IsNullOrEmpty(detalleAdicional))
                            razonFinal = detalleAdicional;
                    }

                    // Siempre marcar Reportado=1 para evitar redireccionamiento en loop
                    string sqlRd = @"UPDATE Rutina_desmontaje
                                     SET Reportado = 1, Desmontado = @Desmontado, RazonNoDesmontaje = @Razon
                                     WHERE RutinaId = @RutinaId AND TAG = @TAG";
                    SqlCommand cmdRd = new SqlCommand(sqlRd, conn);
                    cmdRd.Parameters.AddWithValue("@Desmontado", desmontado ? 1 : 0);
                    cmdRd.Parameters.AddWithValue("@Razon",      (object)razonFinal ?? DBNull.Value);
                    cmdRd.Parameters.AddWithValue("@RutinaId",   rutinaId);
                    cmdRd.Parameters.AddWithValue("@TAG",        tag);
                    cmdRd.ExecuteNonQuery();

                    // Si se desmontó: actualizar estado en el pool
                    if (desmontado)
                    {
                        string sqlDi = @"UPDATE DesmontajeInstrumento
                                         SET Estado = 1, FechaActualizacion = GETDATE(), EmpleadoId = @Empleado
                                         WHERE Id = @Id";
                        SqlCommand cmdDi = new SqlCommand(sqlDi, conn);
                        cmdDi.Parameters.AddWithValue("@Empleado", codigoEmpleado);
                        cmdDi.Parameters.AddWithValue("@Id",       desmontajeInstId);
                        cmdDi.ExecuteNonQuery();
                    }
                }
            }

            Response.Redirect("Generadorrutinas.aspx");
        }
    }
}
