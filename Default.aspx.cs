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
            DateTime fechaInicioZafra = new DateTime(2024, 11, 25); // Fecha real del Día 1

            int diaZafra = (ahora - fechaInicioZafra).Days + 1; // Hoy 22 de Feb = Día 90
            int semanaZafra = diaZafra / 7; // Semana 12 (Par)
            bool esSemanaPar = (semanaZafra % 2 == 0);

            if (ahora.DayOfWeek == DayOfWeek.Sunday && ahora.TimeOfDay >= new TimeSpan(17, 40, 0))
            {
                var perfil = ConsultarDatosEmpleado(Session["CodigoEmpleado"].ToString());

                // Hoy es Domingo, Semana 12 (Par). 
                // Como el Puesto A hizo 12h hoy, la lógica par coincide:
                if (perfil.PuestoFijo == 1) // Pareja A
                {
                    Session["JornadaDomingo"] = esSemanaPar ? "12h" : "4h";
                }
                else if (perfil.PuestoFijo == 2) // Pareja B
                {
                    Session["JornadaDomingo"] = !esSemanaPar ? "12h" : "4h";
                }
            }
            // ... resto del código (Redirect)
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
            if (Session["CodigoEmpleado"] == null)
            {
                return false;
            }

            string codigoEmpleado = Session["CodigoEmpleado"].ToString();

            // 1. Definir el límite de tiempo (8 horas atrás)
            DateTime limiteTiempo = DateTime.Now.AddHours(-8);

            // 2. Consulta SQL: Verificar existencia en las últimas 8 horas
            // Nota: Reemplaza [TuTablaRutinas] por Rutinas.
            string sql = @"
        SELECT COUNT(Correlativo) 
        FROM Rutinas 
        WHERE Codigo_empleado = @CodigoEmpleado AND Fecha >= @LimiteTiempo";

            int count = 0;

            using (SqlConnection conn = new SqlConnection(connectionString)) // Asegúrate de tener ConnString definido
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CodigoEmpleado", codigoEmpleado);
                cmd.Parameters.AddWithValue("@LimiteTiempo", limiteTiempo);

                count = (int)cmd.ExecuteScalar();
            }

            // Si la cuenta es mayor que cero, la rutina ya existe.
            return count > 0;
        }
    }
}