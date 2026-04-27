using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. VERIFICACIÓN CRÍTICA DE SESIÓN
                if (Session["CodigoEmpleado"] == null)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                string nombreEmpleado = Session["NombreEmpleado"].ToString();
                lblnombremenu.Text = "Bienvenido, " + nombreEmpleado;

                // 2. CONTROL DE BOTONES (Generar vs Reimprimir)
                bool rutinaExisteRecientemente = RutinaRecienteExiste();

                if (rutinaExisteRecientemente)
                {
                    btnGenerarRutina.Enabled = false;
                    btnGenerarRutina.CssClass = "panel deshabilitado";
                    btnImprimirRutina.Enabled = true;
                    btnImprimirRutina.CssClass = "panel";

                    // Si ya existe rutina, ocultamos el panel de selección de domingo 
                    // porque ya no necesita generar nada.
                    
                }
                else
                {
                    btnGenerarRutina.Enabled = true;
                    btnGenerarRutina.CssClass = "panel";
                    btnImprimirRutina.Enabled = false;
                    btnImprimirRutina.CssClass = "panel deshabilitado";

                    // 3. LÓGICA ESPECIAL DOMINGO (Solo si va a Generar)
                    DateTime ahora = DateTime.Now;
                    TimeSpan horaActual = ahora.TimeOfDay;
                    TimeSpan inicioVentana = new TimeSpan(17, 40, 0); // 5:40 PM

                    
                }
            }
        }
        string connectionString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        private dynamic ConsultarDatosEmpleado(string codigo)
        {
            // Asegúrate de que los nombres de las columnas coincidan con tu tabla Empleado
            string query = "SELECT PuestoFijo, EsCuartoTurno FROM Empleado WHERE Codigo_empleado = @codigo";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@codigo", codigo);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new
                    {
                        PuestoFijo = Convert.ToInt32(reader["PuestoFijo"]),
                        EsCuartoTurno = Convert.ToBoolean(reader["EsCuartoTurno"])
                    };
                }
            }
            return null;
        }
        protected void btnGenerarRutina_Click(object sender, EventArgs e)
        {
            DateTime ahora = DateTime.Now;
            DateTime fechaInicioZafra = new DateTime(2025, 11, 25); // Fecha real del Día 1

            int diaZafra = (ahora - fechaInicioZafra).Days + 1; // Hoy 22 de Feb = Día 90
            int semanaZafra = diaZafra / 7; // Semana 12 (Par)
            bool esSemanaPar = (semanaZafra % 2 == 0);

            if (ahora.DayOfWeek == DayOfWeek.Sunday && ahora.TimeOfDay >= new TimeSpan(17, 40, 0))
            {
                var perfil = ConsultarDatosEmpleado(Session["CodigoEmpleado"].ToString());

                // PuestoFijo=1 hace 12h en semana PAR  y 4h en semana IMPAR.
                // PuestoFijo=2 hace 12h en semana IMPAR y 4h en semana PAR.
                if (perfil.PuestoFijo == 1) // Pareja A
                {
                    Session["JornadaDomingo"] = esSemanaPar ? "12h" : "4h";
                }
                else if (perfil.PuestoFijo == 2) // Pareja B
                {
                    Session["JornadaDomingo"] = esSemanaPar ? "4h" : "12h";
                }
            }
            Response.Redirect("Generadorrutinas.aspx?Action=Imprimir");
        }

        protected void btnImprimirRutina_Click(object sender, EventArgs e)
        {

            //Response.Redirect("Generadorrutinas.aspx"); asi redirecciona a la misma pagina que el generador
            Response.Redirect("Generadorrutinas.aspx?Action=Reimprimir");//aqui tambien pero el link se modifica para contener
                                                                         //un parametro que identifique de donde viene la solicitud
        }

        protected void registros_Click(object sender, EventArgs e)
        {
            
            Response.Redirect("Registros.aspx");
        }

        protected void logout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut(); //eliminar la cookie de autenticacion
            Session.Clear();
            Session.Abandon(); //asi destruimos la sesion
            // 3. Redirigir al usuario a la página de inicio de sesión
            Response.Redirect("Login.aspx", true);
        }

        

        private bool RutinaRecienteExiste()
        {
            if (Session["CodigoEmpleado"] == null) return false;

            string codigoEmpleado = Session["CodigoEmpleado"].ToString();

            // Para el Auxiliar (PuestoFijo=3): verificar por inicio del turno actual,
            // no por ventana fija de 8h, para que el botón se desbloquee en cada cambio de turno.
            var perfil = ConsultarDatosEmpleado(codigoEmpleado);
            DateTime limiteTiempo = (perfil != null && perfil.PuestoFijo == 3)
                ? ObtenerInicioTurnoActual()
                : DateTime.Now.AddHours(-8);

            string sql = @"SELECT COUNT(Correlativo) FROM Rutinas
                           WHERE Codigo_empleado = @CodigoEmpleado AND Fecha >= @LimiteTiempo";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                cmd.Parameters.AddWithValue("@LimiteTiempo", limiteTiempo);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private DateTime ObtenerInicioTurnoActual()
        {
            DateTime ahora = DateTime.Now;
            TimeSpan hora  = ahora.TimeOfDay;

            if (hora >= new TimeSpan(6, 0, 0) && hora < new TimeSpan(14, 0, 0))
                return ahora.Date.AddHours(6);          // Mañana:  desde 06:00 hoy
            if (hora >= new TimeSpan(14, 0, 0) && hora < new TimeSpan(22, 0, 0))
                return ahora.Date.AddHours(14);         // Tarde:   desde 14:00 hoy
            if (hora >= new TimeSpan(22, 0, 0))
                return ahora.Date.AddHours(22);         // Noche:   desde 22:00 hoy
            return ahora.Date.AddDays(-1).AddHours(22); // Madrugada: desde 22:00 ayer
        }
    }
}