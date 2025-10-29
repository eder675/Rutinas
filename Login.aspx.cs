using System;
using System.Web.UI;

namespace Rutinas
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
        }

        protected void btnentrar_Click(object sender, EventArgs e)
        {
            Page.Validate();

            string mensaje = "";
            string colorFondo = "";
            if (Page.IsValid)
            {
                mensaje = "✅ ¡Validaciones correctas! Datos listos para procesar.";
                colorFondo = "#4CAF50";

            }
            else
            {
                mensaje = "⚠️ No se cumplió la validación. Revisa los mensajes de error.";
                colorFondo = "#f44336"; 

            }
            string script = @"
        var div = document.getElementById('divNotificacion');
        var span = document.getElementById('spanMensaje');
        
        // 1. Configurar el mensaje y el color
        span.innerHTML = '" + mensaje + @"';
        div.style.backgroundColor = '" + colorFondo + @"';
        
        // 2. Mostrar la notificación
        div.style.display = 'block';
        
        // 3. Ocultar después de 3 segundos (3000 milisegundos)
        setTimeout(function(){
            div.style.display = 'none';
        }, 3000);";
            ScriptManager.RegisterStartupScript(this, GetType(), "MostrarMensajeValidacion", script, true);
        }

        protected void txtname_TextChanged(object sender, EventArgs e)
        {

        }
    }
}