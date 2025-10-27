using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UnobtrusiveValidationMode= UnobtrusiveValidationMode.None;
        }

        protected void btnentrar_Click(object sender, EventArgs e)
        {
            
            string script = "";
            if (Page.IsValid)
           {
                 script = "alert('¡Bienvenido a Rutinas!');";


           }
           else {
                 script = "alert('¡Error en la validación!');";

           }
           ScriptManager.RegisterStartupScript(this, GetType(),"MostrarMensajeValidacion", script, true);
        }

        protected void txtname_TextChanged(object sender, EventArgs e)
        {

        }
    }
}