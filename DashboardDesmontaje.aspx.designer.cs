//------------------------------------------------------------------------------
// <generado automáticamente>
//     Este código fue generado por una herramienta.
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto
//     y se perderán si se vuelve a generar el código.
// </generado automáticamente>
//------------------------------------------------------------------------------

namespace Rutinas
{
    public partial class DashboardDesmontaje
    {
        /// <summary>
        /// Control form1.
        /// </summary>
        protected global::System.Web.UI.HtmlControls.HtmlForm form1;

        /// <summary>
        /// Botón para volver a la página principal.
        /// </summary>
        protected global::System.Web.UI.WebControls.Button btnSalir;

        /// <summary>
        /// Etiqueta con el total de equipos registrados en Vinetas.
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblTotal;

        /// <summary>
        /// Etiqueta con el total de equipos desmontados (Desmontado = 1 en DesmontajeAvance).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblDesmontados;

        /// <summary>
        /// Etiqueta con el total de equipos pendientes (Total - Desmontados).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblPendientes;

        /// <summary>
        /// Etiqueta con el porcentaje de avance global (Desmontados / Total * 100).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblPorcentaje;

        protected global::System.Web.UI.WebControls.Label lblDeltaDia;

        protected global::System.Web.UI.WebControls.Label lblDeltaPct;

        /// <summary>
        /// DropDownList para filtrar equipos por área. AutoPostBack = true.
        /// </summary>
        protected global::System.Web.UI.WebControls.DropDownList ddlAreaFiltro;

        /// <summary>
        /// Botón que descarga un archivo Excel con los equipos desmontados del área seleccionada.
        /// </summary>
        protected global::System.Web.UI.WebControls.Button btnExportar;

        /// <summary>
        /// GridView que muestra el avance de desmontaje agrupado por área (siempre sin filtro).
        /// </summary>
        protected global::System.Web.UI.WebControls.GridView gvAvanceAreas;

        protected global::System.Web.UI.WebControls.Label lblContPendientes;

        protected global::System.Web.UI.WebControls.GridView gvPendientes;

        protected global::System.Web.UI.WebControls.Label lblContDesmontados;

        protected global::System.Web.UI.WebControls.GridView gvDesmontados;
    }
}
