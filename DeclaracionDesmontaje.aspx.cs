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

            // 1. Validar que existen datos de búsqueda
            if (string.IsNullOrWhiteSpace(hfDatos.Value))
            {
                lblMsg.Text = "No hay equipos para guardar. Realice primero una busqueda.";
                return;
            }

            // 2. Deserializar JSON
            var ser = new JavaScriptSerializer();
            List<Dictionary<string, string>> equipos;
            try
            {
                equipos = ser.Deserialize<List<Dictionary<string, string>>>(hfDatos.Value);
            }
            catch
            {
                lblMsg.Text = "Error al leer los datos de equipos. Intente de nuevo.";
                return;
            }

            // 3. Leer el formulario y filtrar los marcados como desmontados
            string codigoEmpleado = Session["CodigoEmpleado"]?.ToString() ?? "";
            string nombreEmpleado = Session["NombreEmpleado"]?.ToString() ?? "";

            var desmontados = new List<Dictionary<string, string>>();

            foreach (var item in equipos)
            {
                string tag      = item.ContainsKey("tag")         ? item["tag"]         : "";
                string desc     = item.ContainsKey("descripcion")  ? item["descripcion"]  : "";
                string area     = item.ContainsKey("area")         ? item["area"]         : "";

                // Nombre del radio button: rb_ + tag con caracteres no-alfanuméricos reemplazados por _
                string fieldName = "rb_" + Regex.Replace(tag, @"[^a-zA-Z0-9]", "_");
                string valor     = Request.Form[fieldName] ?? "0";

                if (valor == "1")
                {
                    desmontados.Add(new Dictionary<string, string>
                    {
                        { "tag",         tag  },
                        { "descripcion", desc },
                        { "area",        area }
                    });
                }
            }

            // 4. Validar que al menos uno fue marcado
            if (desmontados.Count == 0)
            {
                lblMsg.Text = "Ningun equipo fue marcado como desmontado.";
                return;
            }

            // 5. MERGE en BD REPORTES
            const string sqlMerge = @"
                MERGE DesmontajeAvance AS t
                USING (SELECT @TAG AS TAG) AS s ON t.TAG = s.TAG
                WHEN MATCHED THEN
                    UPDATE SET
                        Desmontado       = 1,
                        Descripcion      = @Desc,
                        Area             = @Area,
                        FechaDeclaracion = GETDATE(),
                        CodigoEmpleado   = @Codigo,
                        NombreEmpleado   = @Nombre
                WHEN NOT MATCHED THEN
                    INSERT (TAG, Descripcion, Area, Desmontado, CodigoEmpleado, NombreEmpleado)
                    VALUES (@TAG, @Desc, @Area, 1, @Codigo, @Nombre);";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    foreach (var d in desmontados)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlMerge, conn))
                        {
                            cmd.Parameters.AddWithValue("@TAG",    d["tag"]);
                            cmd.Parameters.AddWithValue("@Desc",   d["descripcion"]);
                            cmd.Parameters.AddWithValue("@Area",   d["area"]);
                            cmd.Parameters.AddWithValue("@Codigo", codigoEmpleado);
                            cmd.Parameters.AddWithValue("@Nombre", nombreEmpleado);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // 6. Exito — limpiar hidden field y mostrar toast
                hfDatos.Value = string.Empty;
                lblMsg.Text   = string.Empty;

                string msgOk = desmontados.Count + " equipo(s) guardado(s) correctamente.";
                Page.ClientScript.RegisterStartupScript(
                    GetType(), "toastOk",
                    "mostrarToast('" + msgOk + "', 'ok');", true);
            }
            catch (Exception ex)
            {
                string msg = ex.Message.Replace("'", "\\'")
                                       .Replace("\r", "")
                                       .Replace("\n", " ");

                Page.ClientScript.RegisterStartupScript(
                    GetType(), "toastErr",
                    "mostrarToast('Error al guardar: " + msg + "', 'error');", true);
            }
        }
    }
}
