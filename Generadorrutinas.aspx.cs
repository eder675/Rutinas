using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Configuration;
using Rutinas.Models; //para usar los modelos de datos en la carpeta Models

namespace Rutinas
{
    public partial class Generadorrutinas : System.Web.UI.Page
    {
        private readonly string ConnString =WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}