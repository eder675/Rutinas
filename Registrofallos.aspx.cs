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
            ddlniveldano.Items.Add(new ListItem("Fuente de poder", "Fuente de poder"));
            ddlniveldano.Items.Add(new ListItem("Placa perteneciente al equipo", "Placa perteneciente al equipo"));
            ddlniveldano.Items.Add(new ListItem("Transmisor esclavo", "Transmisor esclavo"));
            ddlniveldano.Items.Add(new ListItem("Accesorio mecanico", "Accesorio mecanico"));
            ddlniveldano.Items.Add(new ListItem("Software o parametrizacion incorrecta", "Software o parametrizacion incorrecta"));
            ddlniveldano.Items.Add(new ListItem("Pieza no reemplazable", "Pieza no reemplazable"));

            ddltipodano.Items.Clear();
            ddltipodano.Items.Add(new ListItem("-- Seleccione --", ""));
            ddltipodano.Items.Add(new ListItem("Fallo eléctrico por humedad", "Fallo eléctrico por humedad"));
            ddltipodano.Items.Add(new ListItem("Fallo por sensor golpeado", "Fallo por sensor golpeado"));
            ddltipodano.Items.Add(new ListItem("Fallo eléctrico por mala conexion", "Fallo eléctrico por mala conexion"));
            ddltipodano.Items.Add(new ListItem("Fallo eléctrico por inundacion", "Fallo eléctrico por inundacion"));
            ddltipodano.Items.Add(new ListItem("Fallo eléctrico por sobretension", "Fallo eléctrico por sobretension"));
            ddltipodano.Items.Add(new ListItem("Fallo mecánico por desacople", "Fallo mecánico por desacople"));
            ddltipodano.Items.Add(new ListItem("Fallo mecanico por mal montaje", "Fallo mecanico por mal montaje"));
            ddltipodano.Items.Add(new ListItem("Fallo mecanico por golpe", "Fallo mecanico por golpe"));
            ddltipodano.Items.Add(new ListItem("Fallo de señal (4-20mA)", "Fallo de señal (4-20mA)"));
            ddltipodano.Items.Add(new ListItem("Fallo electronico por condiciones inapropiadas", "Fallo electronico por condiciones inapropiadas"));
            ddltipodano.Items.Add(new ListItem("Daño físico por caida", "Daño físico por caida"));
            ddltipodano.Items.Add(new ListItem("Corrosión de bornes", "Corrosión de bornes"));
            ddltipodano.Items.Add(new ListItem("Fuga de aire", "Fuga de aire"));
            ddltipodano.Items.Add(new ListItem("Sobrecalentamiento por fallo electronico", "Sobrecalentamiento por fallo electronico"));
            ddltipodano.Items.Add(new ListItem("Sobrecalentamiento por mala ubicacion", "Sobrecalentamiento por mala ubicacion"));

            ddlcausadano.Items.Clear();
            ddlcausadano.Items.Add(new ListItem("-- Seleccione --", ""));
            ddlcausadano.Items.Add(new ListItem("No fue protegido contra el clima adverso", "No fue protegido contra el clima adverso"));
            ddlcausadano.Items.Add(new ListItem("Desgaste natural", "Desgaste natural"));
            ddlcausadano.Items.Add(new ListItem("Vibraciones excesivas", "Vibraciones excesivas"));
            ddlcausadano.Items.Add(new ListItem("Mal manejo del equipo", "Mal manejo del equipo"));
            ddlcausadano.Items.Add(new ListItem("Falta de mantenimiento", "Falta de mantenimiento"));
            ddlcausadano.Items.Add(new ListItem("Condiciones ambientales (lluvia)", "Condiciones ambientales (lluvia)"));
            ddlcausadano.Items.Add(new ListItem("Condiciones ambientales (calor excesivo)", "Condiciones ambientales (calor excesivo)"));
            ddlcausadano.Items.Add(new ListItem("Sobretensión eléctrica", "Sobretensión eléctrica"));
            ddlcausadano.Items.Add(new ListItem("Empaque o sello no reemplazado en mantenimiento", "Empaque o sello no reemplazado en mantenimiento"));
            ddlcausadano.Items.Add(new ListItem("Tuberias y coraza en malas condiciones", "Tuberias y coraza en malas condiciones"));
            ddlcausadano.Items.Add(new ListItem("Los cables que llegan al equipo no estan protegidos", "Los cables que llegan al equipo no estan protegidos"));
            ddlcausadano.Items.Add(new ListItem("Agua que entro por la coraza", "Agua que entro por la coraza"));
            ddlcausadano.Items.Add(new ListItem("Agua que entro por una abertura", "Agua que entro por una abertura"));
            ddlcausadano.Items.Add(new ListItem("Caja de campo en mal estado", "Caja de campo en mal estado"));
            ddlcausadano.Items.Add(new ListItem("Fue golpeado por una persona", "Fue golpeado por una persona"));
            ddlcausadano.Items.Add(new ListItem("Le cayo algo encima", "Le cayo algo encima"));
            ddlcausadano.Items.Add(new ListItem("Impacto por rayo", "Impacto por rayo"));
            ddlcausadano.Items.Add(new ListItem("Ubicacion donde se puede inundar o caer agua", "Ubicacion donde se puede inundar o caer agua"));

            ddlsolucion.Items.Clear();
            ddlsolucion.Items.Add(new ListItem("-- Seleccione --", ""));
            ddlsolucion.Items.Add(new ListItem("Reemplazo temporal", "Reemplazo temporal"));
            ddlsolucion.Items.Add(new ListItem("Reemplazo de pieza dañada", "Reemplazo de pieza dañada"));
            ddlsolucion.Items.Add(new ListItem("Reparación en sitio", "Reparación en sitio"));
            ddlsolucion.Items.Add(new ListItem("Se agregó o adaptó pieza que ayuda a estabilizarse", "Se agrego o adapto pieza que ayuda a estabilizarse"));
            ddlsolucion.Items.Add(new ListItem("Se instalo un soporte extra", "Se instalo un soporte extra"));
            ddlsolucion.Items.Add(new ListItem("Reparacion en el taller", "Reparacion en el taller"));
            ddlsolucion.Items.Add(new ListItem("Equipo fuera de servicio", "Equipo fuera de servicio"));
            ddlsolucion.Items.Add(new ListItem("Pendiente de revisión", "Pendiente de revisión"));
            ddlsolucion.Items.Add(new ListItem("Sin solución aplicada", "Sin solución aplicada"));
            ddlsolucion.Items.Add(new ListItem("Reemplazo de la electronica", "Reemplazo de la electronica"));
            ddlsolucion.Items.Add(new ListItem("Reemplazo de su sensor", "Reemplazo de su sensor"));
            ddlsolucion.Items.Add(new ListItem("Fue movido de donde estaba", "Fue movido de donde estaba"));
            ddlsolucion.Items.Add(new ListItem("Se le puso techo provisional", "Se le puso techo provisional"));
            ddlsolucion.Items.Add(new ListItem("Se tiro cable desde otro lado o solo se cambio el cable", "Se tiro cable desde otro lado o solo se cambio el cable"));
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