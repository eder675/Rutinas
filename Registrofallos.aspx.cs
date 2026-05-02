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
            ddlniveldano.Items.Add(new ListItem("Electrónica", "Electrónica"));
            ddlniveldano.Items.Add(new ListItem("Transmisor esclavo", "Transmisor esclavo"));
            ddlniveldano.Items.Add(new ListItem("Accesorio", "Accesorio"));
            ddlniveldano.Items.Add(new ListItem("Software", "Software"));
            ddlniveldano.Items.Add(new ListItem("Pieza no reemplazable", "Pieza no reemplazable"));

            ddltipodano.Items.Clear();
            ddltipodano.Items.Add(new ListItem("-- Seleccione --", ""));
            ddltipodano.Items.Add(new ListItem("Fallo eléctrico", "Fallo eléctrico"));
            ddltipodano.Items.Add(new ListItem("Fallo mecánico", "Fallo mecánico"));
            ddltipodano.Items.Add(new ListItem("Fallo de señal", "Fallo de señal"));
            ddltipodano.Items.Add(new ListItem("Daño físico", "Daño físico"));
            ddltipodano.Items.Add(new ListItem("Corrosión", "Corrosión"));
            ddltipodano.Items.Add(new ListItem("Sobrecalentamiento", "Sobrecalentamiento"));

            ddlcausadano.Items.Clear();
            ddlcausadano.Items.Add(new ListItem("-- Seleccione --", ""));
            ddlcausadano.Items.Add(new ListItem("Desgaste natural", "Desgaste natural"));
            ddlcausadano.Items.Add(new ListItem("Mal manejo del equipo", "Mal manejo del equipo"));
            ddlcausadano.Items.Add(new ListItem("Falta de mantenimiento", "Falta de mantenimiento"));
            ddlcausadano.Items.Add(new ListItem("Condiciones ambientales", "Condiciones ambientales"));
            ddlcausadano.Items.Add(new ListItem("Sobretensión eléctrica", "Sobretensión eléctrica"));

            ddlsolucion.Items.Clear();
            ddlsolucion.Items.Add(new ListItem("-- Seleccione --", ""));
            ddlsolucion.Items.Add(new ListItem("Reemplazo temporal", "Reemplazo temporal"));
            ddlsolucion.Items.Add(new ListItem("Reparación en sitio", "Reparación en sitio"));
            ddlsolucion.Items.Add(new ListItem("Equipo fuera de servicio", "Equipo fuera de servicio"));
            ddlsolucion.Items.Add(new ListItem("Pendiente de revisión", "Pendiente de revisión"));
            ddlsolucion.Items.Add(new ListItem("Sin solución aplicada", "Sin solución aplicada"));
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
                    cmd.Parameters.AddWithValue("@TipoDano",       ddltipodano.SelectedValue);
                    cmd.Parameters.AddWithValue("@PosibleCausa",   ddlcausadano.SelectedValue);
                    cmd.Parameters.AddWithValue("@Comentarios",    txtcomentario.Text.Trim());
                    cmd.Parameters.AddWithValue("@Reemplaza",      rbtremplazatrue.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Interviene",     rbtintervienetrue.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Solucion",       ddlsolucion.SelectedValue);
                    cmd.ExecuteNonQuery();
                }

                lblMsgInsert.ForeColor = Color.Green;
                lblMsgInsert.Text = "Registro guardado correctamente.";
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                lblMsgInsert.Text = "Error al guardar el registro: " + ex.Message;
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

        protected void ddltipodano_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}