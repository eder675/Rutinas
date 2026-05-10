using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class DeclaracionDesmontaje : System.Web.UI.Page
    {
        private readonly string ConnString =
            WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        // ── PAGE LOAD ────────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblNombre.Text  = Session["NombreEmpleado"]?.ToString() ?? "";
                lblMsg.Text     = string.Empty;
            }
        }

        // ── SALIR ────────────────────────────────────────────────────────────────
        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }

        // ── GUARDAR DECLARACION ──────────────────────────────────────────────────
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            lblMsg.Text = string.Empty;

            // Leer del carrito (hfSeleccionados)
            string json = hfSeleccionados.Value;
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
            {
                lblMsg.Text = "No hay equipos seleccionados para guardar.";
                return;
            }

            var ser = new JavaScriptSerializer();
            List<Dictionary<string, string>> seleccionados;
            try
            {
                seleccionados = ser.Deserialize<List<Dictionary<string, string>>>(json);
            }
            catch
            {
                lblMsg.Text = "Error al leer los datos. Intente de nuevo.";
                return;
            }

            if (seleccionados.Count == 0)
            {
                lblMsg.Text = "No hay equipos seleccionados para guardar.";
                return;
            }

            string codigoEmpleado = Session["CodigoEmpleado"]?.ToString() ?? "";
            string nombreEmpleado = Session["NombreEmpleado"]?.ToString() ?? "";

            const string sqlMerge = @"
                MERGE DesmontajeAvance AS t
                USING (SELECT @TAG AS TAG) AS s ON t.TAG = s.TAG
                WHEN MATCHED THEN
                    UPDATE SET Desmontado=1, Descripcion=@Desc, Area=@Area,
                               FechaDeclaracion=GETDATE(), CodigoEmpleado=@Codigo, NombreEmpleado=@Nombre
                WHEN NOT MATCHED THEN
                    INSERT (TAG, Descripcion, Area, Desmontado, CodigoEmpleado, NombreEmpleado)
                    VALUES (@TAG, @Desc, @Area, 1, @Codigo, @Nombre);";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    foreach (var item in seleccionados)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlMerge, conn))
                        {
                            cmd.Parameters.AddWithValue("@TAG",    item.ContainsKey("tag")         ? item["tag"]         : "");
                            cmd.Parameters.AddWithValue("@Desc",   item.ContainsKey("descripcion")  ? item["descripcion"]  : "");
                            cmd.Parameters.AddWithValue("@Area",   item.ContainsKey("area")         ? item["area"]         : "");
                            cmd.Parameters.AddWithValue("@Codigo", codigoEmpleado);
                            cmd.Parameters.AddWithValue("@Nombre", nombreEmpleado);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                hfSeleccionados.Value = "[]";
                lblMsg.Text = string.Empty;
                string msgOk = seleccionados.Count + " equipo(s) guardado(s) correctamente.";
                Page.ClientScript.RegisterStartupScript(GetType(), "toastOk",
                    "mostrarToast('" + msgOk + "', 'ok'); limpiarCarrito();", true);
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
                Page.ClientScript.RegisterStartupScript(GetType(), "toastErr",
                    "mostrarToast('Error al guardar: " + msg + "', 'error');", true);
            }
        }
    }
}
