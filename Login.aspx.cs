using System;
using System.Data; 
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;

namespace Rutinas
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
            rfvName.Enabled = false;
            txtcodigo.Focus();  
        }

        protected void btnentrar_Click(object sender, EventArgs e)
        {
            // NO es necesario llamar a Page.Validate() si ya usas los RequiredFieldValidator, 
            // pero lo mantendremos para consistencia.
            Page.Validate();

            // Solo necesitamos la cadena de conexión si las validaciones son correctas
            // string connectionString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
            
            if (Page.IsValid)
            {
                // 1. Obtener datos de los TextBox (Asegúrate que los IDs sean correctos: txtname y txtcodigo)
                string nombreIngresado = txtname.Text.Trim();
                string codigoIngresado = txtcodigo.Text.Trim();

                // 2. Obtener la Cadena de Conexión
                string connectionString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

                // 3. Consulta SQL para verificar la credencial
                // Buscamos un registro que COINCIDA en Nombre Y Codigo_empleado
                string consulta;
                bool esInstrumentista = rbtInstrumentista.Checked;

                if (esInstrumentista)
                {
                    // A) Lógica para INSTRUMENTISTA: Solo validar por Codigo_empleado
                    consulta = "SELECT Nombre, Cargo, Codigo_empleado FROM Empleado WHERE Codigo_empleado = @Codigo";
                }
                else // Se asume que es Administrador (o la única otra opción)
                {
                    // B) Lógica para ADMINISTRADOR: Validar por Nombre Y Codigo_empleado
                    consulta = "SELECT Nombre, Cargo, Codigo_empleado FROM Empleado WHERE Nombre = @Nombre AND Codigo_empleado = @Codigo";
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(consulta, conn))
                    {
                        // Parámetros SQL: ¡CLAVE para la seguridad (prevención de Inyección SQL)!
                        cmd.Parameters.AddWithValue("@Nombre", nombreIngresado);
                        cmd.Parameters.AddWithValue("@Codigo", codigoIngresado);

                        try
                        {
                            conn.Open();
                            SqlDataReader reader = cmd.ExecuteReader();

                            if (reader.Read()) // Si 'reader.Read()' devuelve true, el usuario existe y las credenciales son correctas.
                            {
                                // 4. ¡Inicio de Sesión Exitoso!
                                string nombreUsuario = reader["Nombre"].ToString();
                                string cargoUsuario = reader["Cargo"].ToString();
                                string codigoUsuario = reader["Codigo_empleado"].ToString();

                                // Crear Variables de Sesión para mantener el estado del usuario
                                Session["NombreEmpleado"] = nombreUsuario;
                                Session["Cargo"] = cargoUsuario; 
                                //Session["Codigo,"] = codigoUsuario;
                                Session["CodigoEmpleado"] = codigoUsuario;



                                // Redirigir al usuario a la página principal
                                // Reemplaza "~/Default.aspx" por la ruta correcta de tu página de inicio.
                                // --------------------------------------------------------------------------
                                // LÓGICA DE REDIRECCIÓN CONDICIONAL
                                // --------------------------------------------------------------------------

                                string paginaDestino;

                                // Evaluar si el cargo es Administrador (lo llevamos al panel de Desarrolladores)
                                if (cargoUsuario.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                                {
                                    // El Administrador irá a la página de desarrollador
                                    paginaDestino = "~/LoginDeveloper.aspx";
                                }
                                else
                                {
                                    // Cualquier otro (Instrumentista, etc.) va a la página principal
                                    paginaDestino = "~/Default.aspx";
                                }

                                // --------------------------------------------------------------------------
                                // EMISIÓN DEL TICKET Y REDIRECCIÓN FINAL
                                // --------------------------------------------------------------------------

                                // 1. Crea el ticket de autenticación (la cookie)
                                FormsAuthentication.SetAuthCookie(nombreUsuario, false);

                                // 2. Redirige a la página elegida
                                Response.Redirect(paginaDestino);

                                return; // Terminamos la ejecución
                            }
                            else
                            {
                                // 5. Credenciales Incorrectas
                                MostrarNotificacion("❌ Nombre o código de empleado incorrectos. Intente de nuevo.", "#f44336");
                            }
                            reader.Close();
                            conn.Close();
                        }
                        catch (Exception ex)
                        {
                            // 6. Error de Conexión o SQL
                            // Mostrar un mensaje genérico al usuario y registrar el error para ti (el desarrollador)
                            MostrarNotificacion("⚠️ Error al conectar con la base de datos. Contacte a soporte.", "#ff9800");
                            // Aquí podrías agregar un logger (ej: Console.WriteLine(ex.Message);)
                        }
                    }
                }
            }
            else
            {
                // 7. Fallo en la Validación (campos vacíos)
                // Usamos tu función de notificaciones para los errores de validación (RequiredFieldValidator)
                MostrarNotificacion("⚠️ No se cumplió la validación. Revisa los mensajes de error.", "#f44336");
            }

            // **NOTA**: Ya no necesitamos el código del script aquí porque lo encapsularemos en la función 'MostrarNotificacion'
        }


        // --- Función Auxiliar ---
        // (Debes copiar esta función, ya que la has usado para las notificaciones)
        private void MostrarNotificacion(string mensaje, string colorFondo)
        {
            string script = @"
        var div = document.getElementById('divNotificacion');
        var span = document.getElementById('spanMensaje');
        
        span.innerHTML = '" + mensaje.Replace("'", "\\'") + @"'; 
        div.style.backgroundColor = '" + colorFondo + @"';
        
        div.style.display = 'block';
        
        setTimeout(function(){
            div.style.display = 'none';
        }, 3000);";

            ScriptManager.RegisterStartupScript(this, GetType(), "MostrarMensajeValidacion", script, true);
        }

        protected void txtname_TextChanged(object sender, EventArgs e)
        {

        }

        protected void rbtInstrumentista_CheckedChanged(object sender, EventArgs e)
        {
                txtname.Enabled = false;
                rfvName.Enabled = false;
                txtname.Text = "";

        }

        protected void rbtAdministrador_CheckedChanged(object sender, EventArgs e)
        {
            txtname.Enabled = true;
            rfvName.Enabled = true;
            txtname.Text = "";
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("AcercaDe.aspx");
        }
    }
}