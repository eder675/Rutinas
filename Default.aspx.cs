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
           if (!IsPostBack) {
                // 1. VERIFICACIÓN CRÍTICA DE SESIÓN
                if (Session["CodigoEmpleado"] == null)
                {
                    // Si la sesión no existe, redirigir al Login.
                    Response.Redirect("Login.aspx");
                    return; // Termina la ejecución de la página
                }

                // 2. Si la sesión es válida, procedemos con el control de botones.

                bool rutinaExisteRecientemente = RutinaRecienteExiste();

                if (rutinaExisteRecientemente)
                {
                    // Rutina generada: Bloquear Generar
                    btnGenerarRutina.Enabled = false;
                    btnGenerarRutina.CssClass = "panel deshabilitado";

                    // Habilitar Reimprimir
                    btnImprimirRutina.Enabled = true;
                    btnImprimirRutina.CssClass = "panel";
                }
                else
                {
                    // Rutina no generada: Habilitar Generar
                    btnGenerarRutina.Enabled = true;
                    btnGenerarRutina.CssClass = "panel";

                    // Bloquear Reimprimir
                    btnImprimirRutina.Enabled = false;
                    btnImprimirRutina.CssClass = "panel deshabilitado";
                }
            }
            string nombreEmpleado = Session["NombreEmpleado"].ToString();
            lblnombremenu.Text = "Bienvenido, " + nombreEmpleado;
        }
        string connectionString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        protected void btnGenerarRutina_Click(object sender, EventArgs e)
        {
            // Aquí puedes incluir la lógica para determinar el área (Fabricación/Extracción)
            // y generar los 30 instrumentos al azar antes de redirigir.

            // Redirección a la página de generación de rutina
            Response.Redirect("Generadorrutinas.aspx");
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