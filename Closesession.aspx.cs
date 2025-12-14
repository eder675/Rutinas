using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace Rutinas
{
    public partial class Closesession : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Cierra la Sesión de Autenticación de Formularios
            FormsAuthentication.SignOut();

            // 2. Cierra la Sesión de Variables de Estado (Destruye Session["NombreUsuario"])
            Session.Clear();
            Session.Abandon();

            // 3. Redirigir al usuario al formulario de login
            // 'false' permite que el código anterior se complete.
            Response.Redirect("Login.aspx", false);

            // Termina la solicitud actual para asegurar la redirección
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}