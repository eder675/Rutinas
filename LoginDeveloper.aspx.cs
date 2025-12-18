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
            if (viewIndex == 2) gvarea.DataBind();
            if (viewIndex == 3) gvinstrumentos.DataBind();
            // ... y así sucesivamente para las otras vistas
        }

        protected void gvempleados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Solo actuamos si el comando es el que definimos en el botón del Footer
            if (e.CommandName == "insertnew")
            {
                // 1. Localizar los controles dentro del FooterRow del GridView
                // Usamos los IDs que les pusiste a tus controles en el modo "Editar Plantillas"
                TextBox txtCod = (TextBox)gvempleados.FooterRow.FindControl("txtnewcode");
                TextBox txtNom = (TextBox)gvempleados.FooterRow.FindControl("txtnewname");
                DropDownList ddlCar = (DropDownList)gvempleados.FooterRow.FindControl("ddlinsertnew");

                // 2. Validar que los campos no estén vacíos (opcional pero recomendado)
                if (string.IsNullOrEmpty(txtCod.Text) || string.IsNullOrEmpty(txtNom.Text))
                {
                    Response.Write("<script>alert('Por favor, complete el código y el nombre.');</script>");
                    return;
                }

                // 3. Asignar los valores a los parámetros del SqlDataSource
                // Estos nombres ["NombreParametro"] deben ser IGUALES a los que pusiste 
                // en el InsertQuery del SqlDataSource2 (@Codigo_empleado, @Nombre, @Cargo)
                SqlDataSource2.InsertParameters["Codigo_empleado"].DefaultValue = txtCod.Text;
                SqlDataSource2.InsertParameters["Nombre"].DefaultValue = txtNom.Text;
                SqlDataSource2.InsertParameters["Cargo"].DefaultValue = ddlCar.SelectedValue;

                try
                {
                    // 4. Ejecutar la inserción en la base de datos
                    SqlDataSource2.Insert();

                    // 5. Limpiar los campos para una nueva entrada
                    txtCod.Text = "";
                    txtNom.Text = "";
                    // El dropdown regresa a su primer valor automáticamente al recargar

                    // El GridView se refresca solo gracias al SqlDataSource
                }
                catch (Exception ex)
                {
                    // Manejo de errores (por ejemplo, si el código de empleado ya existe)
                    Response.Write("<script>alert('Error al insertar: " + ex.Message.Replace("'", "") + "');</script>");
                }
            }
        }
    }
}