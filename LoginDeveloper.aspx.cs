using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class LoginDeveloper : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void lnk_Click(object sender, EventArgs e)
        {
            LinkButton lnb = (LinkButton)sender;

            // El CommandArgument es el índice de la vista a mostrar (0, 1, 2, 3)
            int viewIndex = Convert.ToInt32(lnb.CommandArgument);

            // Cambia la vista activa del MultiView
            
            mvadmin.ActiveViewIndex = viewIndex;

            // Opcional: Recargar o enlazar los datos si es necesario al cambiar de vista
            if (viewIndex == 0) gvrutinas.DataBind();
            if (viewIndex == 1) gvempleados.DataBind();
            //if (viewIndex == 2) gvareas.DataBind();
            //if (viewIndex == 3) gvinstrumentos.DataBind();
            // ... y así sucesivamente para las otras vistas
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {

        }
    }
}