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
    public partial class Dashboard
    {
        /// <summary>
        /// Control form1.
        /// </summary>
        protected global::System.Web.UI.HtmlControls.HtmlForm form1;

        /// <summary>
        /// Control ScriptManager1.
        /// </summary>
        protected global::System.Web.UI.ScriptManager ScriptManager1;

        /// <summary>
        /// Botón para volver a la página principal.
        /// </summary>
        protected global::System.Web.UI.WebControls.Button btnSalir;

        /// <summary>
        /// Etiqueta con el total de fallos registrados.
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblTotalFallos;

        /// <summary>
        /// Etiqueta con el total de fallos inducidos (FalloInducido = 1).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblInducidos;

        /// <summary>
        /// Etiqueta con el total de fallos naturales (FalloInducido = 0).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblNaturales;

        /// <summary>
        /// Etiqueta con el total de equipos pendientes de reemplazo (Reemplaza = 1).
        /// </summary>
        protected global::System.Web.UI.WebControls.Label lblReemplazar;

        /// <summary>
        /// DropDownList para filtrar registros por NivelDano.
        /// AutoPostBack = true.
        /// </summary>
        protected global::System.Web.UI.WebControls.DropDownList ddlFiltroNivel;

        /// <summary>
        /// GridView que muestra los registros de RegistroFallos
        /// (todas las columnas excepto Comentarios y Solucion).
        /// </summary>
        protected global::System.Web.UI.WebControls.GridView gvFallos;

        /// <summary>
        /// Botón que descarga un archivo Excel con ClosedXML.
        /// </summary>
        protected global::System.Web.UI.WebControls.Button btnExportar;
    }
}
