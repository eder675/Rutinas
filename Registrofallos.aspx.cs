using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Registrofallos : System.Web.UI.Page
    {
        private readonly string ConnString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDropdowns();
                lblname.Text = Session["NombreEmpleado"]?.ToString() ?? "";
            }
        }

        private void CargarDropdowns()
        {
            ddlniveldano.Items.Clear();
            ddlniveldano.Items.Add(new ListItem("-- Seleccione --", ""));
            ddlniveldano.Items.Add(new ListItem("Transmisor o posicionador", "Transmisor o posicionador"));
            ddlniveldano.Items.Add(new ListItem("Electrónica del transmisor", "Electrónica del transmisor"));
            ddlniveldano.Items.Add(new ListItem("Placa perteneciente al equipo", "Placa perteneciente al equipo"));
            ddlniveldano.Items.Add(new ListItem("Transmisor esclavo", "Transmisor esclavo"));
            ddlniveldano.Items.Add(new ListItem("Fuente de poder", "Fuente de poder"));
            ddlniveldano.Items.Add(new ListItem("Accesorio mecanico", "Accesorio mecanico"));
            ddlniveldano.Items.Add(new ListItem("Software o parametrización incorrecta", "Software o parametrización incorrecta"));
            ddlniveldano.Items.Add(new ListItem("Pieza no reemplazable", "Pieza no reemplazable"));

            ddltipodano.Items.Clear();
            ddltipodano.Items.Add(new ListItem("-- Seleccione --", ""));

            ddlcausadano.Items.Clear();
            ddlcausadano.Items.Add(new ListItem("-- Seleccione --", ""));

            ddlsolucion.Items.Clear();
            ddlsolucion.Items.Add(new ListItem("-- Seleccione --", ""));
        }

        protected void btninsert_Click(object sender, EventArgs e)
        {
            lblMsgInsert.ForeColor = Color.Red;

            if (string.IsNullOrWhiteSpace(hfTAG.Value))
            {
                lblMsgInsert.Text = "Seleccione un instrumento antes de guardar.";
                return;
            }

            if (!rbinducitrue.Checked && !rbinducifalse.Checked)
            {
                lblMsgInsert.Text = "Indique si el fallo fue inducido.";
                return;
            }

            if (!rbtremplazatrue.Checked && !rbtremplazafalse.Checked)
            {
                lblMsgInsert.Text = "Indique si el equipo debe reemplazarse.";
                return;
            }

            if (!rbtintervienetrue.Checked && !rbtintervienefalse.Checked)
            {
                lblMsgInsert.Text = "Indique si el equipo interviene en el proceso.";
                return;
            }

            if (string.IsNullOrEmpty(ddlniveldano.SelectedValue))
            {
                lblMsgInsert.Text = "Seleccione el nivel de daño.";
                return;
            }

            string tipoDano  = Request.Form[ddltipodano.UniqueID]  ?? "";
            string causaDano = Request.Form[ddlcausadano.UniqueID] ?? "";
            string solucion  = Request.Form[ddlsolucion.UniqueID]  ?? "";

            if (string.IsNullOrEmpty(tipoDano))
            {
                lblMsgInsert.Text = "Seleccione el tipo de daño.";
                return;
            }

            if (string.IsNullOrEmpty(causaDano))
            {
                lblMsgInsert.Text = "Seleccione la posible causa.";
                return;
            }

            if (string.IsNullOrEmpty(solucion))
            {
                lblMsgInsert.Text = "Seleccione la solución actual.";
                return;
            }

            try
            {
                string sql = @"INSERT INTO RegistroFallos
                    (Instrumentista, TAG, Instrumento, FalloInducido, NivelDano, TipoDano, PosibleCausa, Comentarios, Reemplaza, Interviene, Solucion)
                    VALUES
                    (@Instrumentista, @TAG, @Instrumento, @FalloInducido, @NivelDano, @TipoDano, @PosibleCausa, @Comentarios, @Reemplaza, @Interviene, @Solucion)";

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Instrumentista", lblname.Text.Trim());
                    cmd.Parameters.AddWithValue("@TAG",            hfTAG.Value.Trim());
                    cmd.Parameters.AddWithValue("@Instrumento",    hfDescripcion.Value.Trim());
                    cmd.Parameters.AddWithValue("@FalloInducido",  rbinducitrue.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@NivelDano",      ddlniveldano.SelectedValue);
                    cmd.Parameters.AddWithValue("@TipoDano",       tipoDano);
                    cmd.Parameters.AddWithValue("@PosibleCausa",   causaDano);
                    cmd.Parameters.AddWithValue("@Comentarios",    txtcomentario.Text.Trim());
                    cmd.Parameters.AddWithValue("@Reemplaza",      rbtremplazatrue.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Interviene",     rbtintervienetrue.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Solucion",       solucion);
                    cmd.ExecuteNonQuery();
                }

                lblMsgInsert.Text = string.Empty;
                LimpiarFormulario();
                ScriptManager.RegisterStartupScript(this, GetType(), "toast",
                    "mostrarToast('Registro guardado correctamente.', 'ok');", true);
            }
            catch (Exception ex)
            {
                lblMsgInsert.Text = string.Empty;
                string msg = ex.Message.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
                ScriptManager.RegisterStartupScript(this, GetType(), "toast",
                    $"mostrarToast('Error al guardar: {msg}', 'error');", true);
            }
        }

        private void LimpiarFormulario()
        {
            txtTAG.Text         = string.Empty;
            txtdescripcion.Text = string.Empty;
            hfTAG.Value         = string.Empty;
            hfDescripcion.Value = string.Empty;
            txtcomentario.Text  = string.Empty;
            rbinducitrue.Checked      = false;
            rbinducifalse.Checked     = false;
            rbtremplazatrue.Checked   = false;
            rbtremplazafalse.Checked  = false;
            rbtintervienetrue.Checked  = false;
            rbtintervienefalse.Checked = false;
            ddlniveldano.SelectedIndex  = 0;
            ddltipodano.SelectedIndex   = 0;
            ddlcausadano.SelectedIndex  = 0;
            ddlsolucion.SelectedIndex   = 0;
        }

        protected void btnSalir_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }

        protected void ddltipodano_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}