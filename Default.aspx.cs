using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rutinas
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           if (!IsPostBack) {

           }
        }

        protected void btnGenerarRutina_Click(object sender, EventArgs e)
        {
            // Aquí puedes incluir la lógica para determinar el área (Fabricación/Extracción)
            // y generar los 30 instrumentos al azar antes de redirigir.

            // Redirección a la página de generación de rutina
            Response.Redirect("PaginaGeneracionRutina.aspx");
        }

        protected void btnImprimirRutina_Click(object sender, EventArgs e)
        {
            // Lógica para obtener el correlativo de la última rutina generada en el turno
            // y prepararla para la impresión.

            // Redirección a la página de impresión/reporte
            Response.Redirect("PaginaReporteImpresion.aspx");
        }

    }
}