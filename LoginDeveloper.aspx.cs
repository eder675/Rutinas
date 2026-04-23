using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;


namespace Rutinas
{
    public partial class LoginDeveloper : System.Web.UI.Page
    {
        private readonly string ConnString = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            string Adminname = Session["NombreEmpleado"].ToString();
            lblnameadmin.Text = "Administrador en sesion: " + Adminname;
        }

        protected void lnk_Click(object sender, EventArgs e)
        {
            LinkButton lnb = (LinkButton)sender;
            int viewIndex = Convert.ToInt32(lnb.CommandArgument);
            mvadmin.ActiveViewIndex = viewIndex;

            if (viewIndex == 0) gvrutinas.DataBind();
            if (viewIndex == 1) gvempleados.DataBind();
            if (viewIndex == 2) gvarea.DataBind();
            if (viewIndex == 3) gvinstrumentos.DataBind();
            if (viewIndex == 4) CargarConfigDesmontaje();
        }

        #region config desmontaje

        private void CargarConfigDesmontaje()
        {
            // Cargar valores actuales de DesmontajeConfig
            string sql = "SELECT MostrarTablaInstrumentos, MostrarTablaObligatorios, MostrarTablaDesmontaje, CantManana, CantTarde, CantNoche FROM DesmontajeConfig WHERE Id = 1";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        chkMostrarInstrumentos.Checked = Convert.ToBoolean(dr["MostrarTablaInstrumentos"]);
                        chkMostrarObligatorios.Checked = Convert.ToBoolean(dr["MostrarTablaObligatorios"]);
                        chkMostrarDesmontaje.Checked   = Convert.ToBoolean(dr["MostrarTablaDesmontaje"]);
                        txtCantManana.Text             = dr["CantManana"].ToString();
                        txtCantTarde.Text              = dr["CantTarde"].ToString();
                        txtCantNoche.Text              = dr["CantNoche"].ToString();
                    }
                }
            }

            // Cargar áreas disponibles y marcar las habilitadas
            DataTable dtAreas = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                new SqlDataAdapter("SELECT IDarea, Nombre FROM Area ORDER BY IDarea", conn).Fill(dtAreas);
            }
            cblAreas.DataSource     = dtAreas;
            cblAreas.DataTextField  = "Nombre";
            cblAreas.DataValueField = "IDarea";
            cblAreas.DataBind();

            // Marcar las áreas que ya están habilitadas
            DataTable dtHabilitadas = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                new SqlDataAdapter("SELECT AreaId FROM DesmontajeAreaHabilitada", conn).Fill(dtHabilitadas);
            }
            HashSet<string> idsHabilitados = new HashSet<string>();
            foreach (DataRow row in dtHabilitadas.Rows)
                idsHabilitados.Add(row["AreaId"].ToString());

            foreach (ListItem item in cblAreas.Items)
                item.Selected = idsHabilitados.Contains(item.Value);

            // Cargar pool de instrumentos
            BindPoolDesmontaje();
        }

        private void BindPoolDesmontaje()
        {
            string sql = @"
                SELECT DI.Id, DI.TAG, I.Nombre AS NombreInstrumento, A.Nombre AS NombreArea, DI.Estado
                FROM DesmontajeInstrumento DI
                INNER JOIN Instrumentos I ON DI.TAG = I.TAG
                INNER JOIN Area A ON I.IDarea = A.IDarea
                ORDER BY A.IDarea, I.Nombre";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                new SqlDataAdapter(new SqlCommand(sql, conn)).Fill(dt);
            }
            gvPoolDesmontaje.DataSource = dt;
            gvPoolDesmontaje.DataBind();

            // Poblar dropdown del footer con instrumentos que aún no están en el pool
            DropDownList ddl = (DropDownList)gvPoolDesmontaje.FooterRow?.FindControl("ddlNuevoInstrumento");
            if (ddl != null)
            {
                string sqlDisponibles = @"
                    SELECT TAG, Nombre FROM Instrumentos
                    WHERE TAG NOT IN (SELECT TAG FROM DesmontajeInstrumento)
                    ORDER BY Nombre";
                DataTable dtDisp = new DataTable();
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    new SqlDataAdapter(new SqlCommand(sqlDisponibles, conn)).Fill(dtDisp);
                }
                ddl.DataSource     = dtDisp;
                ddl.DataTextField  = "Nombre";
                ddl.DataValueField = "TAG";
                ddl.DataBind();
                ddl.Items.Insert(0, new ListItem("-- Seleccione instrumento --", ""));
            }
        }

        protected void btnGuardarConfig_Click(object sender, EventArgs e)
        {
            int cantManana = 0, cantTarde = 0, cantNoche = 0;
            int.TryParse(txtCantManana.Text.Trim(), out cantManana);
            int.TryParse(txtCantTarde.Text.Trim(),  out cantTarde);
            int.TryParse(txtCantNoche.Text.Trim(),  out cantNoche);

            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    // Actualizar configuración global
                    string sqlCfg = @"UPDATE DesmontajeConfig SET
                        MostrarTablaInstrumentos = @Inst,
                        MostrarTablaObligatorios = @Obl,
                        MostrarTablaDesmontaje   = @Des,
                        CantManana = @M, CantTarde = @T, CantNoche = @N
                        WHERE Id = 1";
                    SqlCommand cmdCfg = new SqlCommand(sqlCfg, conn, tx);
                    cmdCfg.Parameters.AddWithValue("@Inst", chkMostrarInstrumentos.Checked ? 1 : 0);
                    cmdCfg.Parameters.AddWithValue("@Obl",  chkMostrarObligatorios.Checked ? 1 : 0);
                    cmdCfg.Parameters.AddWithValue("@Des",  chkMostrarDesmontaje.Checked   ? 1 : 0);
                    cmdCfg.Parameters.AddWithValue("@M",    cantManana);
                    cmdCfg.Parameters.AddWithValue("@T",    cantTarde);
                    cmdCfg.Parameters.AddWithValue("@N",    cantNoche);
                    cmdCfg.ExecuteNonQuery();

                    // Reconstruir áreas habilitadas (borrar todo y re-insertar marcadas)
                    new SqlCommand("DELETE FROM DesmontajeAreaHabilitada", conn, tx).ExecuteNonQuery();
                    foreach (ListItem item in cblAreas.Items)
                    {
                        if (item.Selected)
                        {
                            SqlCommand cmdArea = new SqlCommand(
                                "INSERT INTO DesmontajeAreaHabilitada (AreaId) VALUES (@AreaId)", conn, tx);
                            cmdArea.Parameters.AddWithValue("@AreaId", Convert.ToInt32(item.Value));
                            cmdArea.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    lblConfigMsg.Text      = "Configuración guardada correctamente.";
                    lblConfigMsg.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    lblConfigMsg.Text      = "Error al guardar: " + ex.Message;
                    lblConfigMsg.ForeColor = System.Drawing.Color.Red;
                }
            }

            BindPoolDesmontaje();
        }

        protected void gvPoolDesmontaje_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "resetPool")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                string sql = "UPDATE DesmontajeInstrumento SET Estado = 0, RazonExclusion = NULL, FechaActualizacion = NULL, EmpleadoId = NULL WHERE Id = @Id";
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
                BindPoolDesmontaje();
            }
            else if (e.CommandName == "eliminarPool")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    // Eliminar registros de Rutina_desmontaje primero para evitar FK
                    string tagSql = "SELECT TAG FROM DesmontajeInstrumento WHERE Id = @Id";
                    SqlCommand cmdTag = new SqlCommand(tagSql, conn);
                    cmdTag.Parameters.AddWithValue("@Id", id);
                    object tagObj = cmdTag.ExecuteScalar();
                    if (tagObj != null)
                    {
                        string tag = tagObj.ToString();
                        SqlCommand cmdDelRd = new SqlCommand(
                            "DELETE FROM Rutina_desmontaje WHERE TAG = @TAG", conn);
                        cmdDelRd.Parameters.AddWithValue("@TAG", tag);
                        cmdDelRd.ExecuteNonQuery();
                    }
                    new SqlCommand("DELETE FROM DesmontajeInstrumento WHERE Id = " + id, conn).ExecuteNonQuery();
                }
                BindPoolDesmontaje();
            }
            else if (e.CommandName == "agregarPool")
            {
                DropDownList ddl = (DropDownList)gvPoolDesmontaje.FooterRow.FindControl("ddlNuevoInstrumento");
                if (ddl == null || string.IsNullOrEmpty(ddl.SelectedValue)) return;

                string tag = ddl.SelectedValue;
                string sqlCheck = "SELECT COUNT(1) FROM DesmontajeInstrumento WHERE TAG = @TAG";
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlCommand cmdChk = new SqlCommand(sqlCheck, conn);
                    cmdChk.Parameters.AddWithValue("@TAG", tag);
                    int exists = Convert.ToInt32(cmdChk.ExecuteScalar());
                    if (exists == 0)
                    {
                        SqlCommand cmdIns = new SqlCommand(
                            "INSERT INTO DesmontajeInstrumento (TAG, Estado) VALUES (@TAG, 0)", conn);
                        cmdIns.Parameters.AddWithValue("@TAG", tag);
                        cmdIns.ExecuteNonQuery();
                    }
                }
                BindPoolDesmontaje();
            }
        }

        #endregion

        protected void gvempleados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Solo actuamos si el comando es el que definimos en el botón del Footer
            if (e.CommandName == "insertnewempleado")
            {
                // 1. Localizar los controles dentro del FooterRow del GridView
                // Usamos los IDs que les pusiste a tus controles en el modo "Editar Plantillas"
                TextBox txtCod = (TextBox)gvempleados.FooterRow.FindControl("txtnewcode");
                TextBox txtNom = (TextBox)gvempleados.FooterRow.FindControl("txtnewname");
                DropDownList ddlCar = (DropDownList)gvempleados.FooterRow.FindControl("ddlinsertnew");

                // 2. Validar que los campos no estén vacíos (opcional pero recomendado)
                if (string.IsNullOrEmpty(txtCod.Text) || string.IsNullOrEmpty(txtNom.Text))
                {
                    
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

        protected void gvarea_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Solo actuamos si el comando es el que definimos en el botón del Footer
            if (e.CommandName == "insertnewarea")
            {
                // 1. Localizar los controles dentro del FooterRow del GridView
                // Usamos los IDs que les pusiste a tus controles en el modo "Editar Plantillas"
                TextBox txtName = (TextBox)gvarea.FooterRow.FindControl("txtnewarea");
                DropDownList ddlGroup = (DropDownList)gvarea.FooterRow.FindControl("ddlnewgrupo");

                // 2. Validar que los campos no estén vacíos (opcional pero recomendado)
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(ddlGroup.Text))
                {
                    Response.Write("<script>alert('Por favor, los campos requeridos.');</script>");
                    return;
                }

                // 3. Asignar los valores a los parámetros del SqlDataSource
                // Estos nombres ["NombreParametro"] deben ser IGUALES a los que pusiste 
                // en el InsertQuery del SqlDataSource2 (@Codigo_empleado, @Nombre, @Cargo)
                SqlDataSource1.InsertParameters["Nombre"].DefaultValue = txtName.Text;
                SqlDataSource1.InsertParameters["IDgrupo"].DefaultValue = ddlGroup.SelectedValue;

                try
                {
                    // 4. Ejecutar la inserción en la base de datos
                    SqlDataSource1.Insert();

                    // 5. Limpiar los campos para una nueva entrada
                    txtName.Text = "";
                   
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

        protected void gvinstrumentos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "insertnewinstrumentos")
            {
                // 1. CAPTURAR CONTROLES (Cambiamos TextBox por ListBox en TAG y Nombre)
                ListBox lbxtag = (ListBox)gvinstrumentos.FooterRow.FindControl("lbxtag");
                ListBox lbxnombre = (ListBox)gvinstrumentos.FooterRow.FindControl("lbxnombre");

                DropDownList ddlactividad = (DropDownList)gvinstrumentos.FooterRow.FindControl("ddlactividad");
                DropDownList ddlGrupo = (DropDownList)gvinstrumentos.FooterRow.FindControl("ddlarea");
                DropDownList ddlPrioridad = (DropDownList)gvinstrumentos.FooterRow.FindControl("ddlprioridad");
                DropDownList ddlObliga = (DropDownList)gvinstrumentos.FooterRow.FindControl("ddlobliga");
                DropDownList ddlAnalisis = (DropDownList)gvinstrumentos.FooterRow.FindControl("ddlanalisis");
                // 2. OBTENER VALORES (Lógica para Select2 con tags:true)
                // Si el usuario escribió algo nuevo, SelectedValue nos dará ese texto.
                //string valorTAG = lbxtag.SelectedValue;
                //string valorNombre = lbxnombre.SelectedValue;
                string valorTAG = Request.Form[lbxtag.UniqueID];
                string valorNombre = Request.Form[lbxnombre.UniqueID];
                //string obligatorio = "1";
                //string tipoanalisis = "N/A";

                // PRUEBA TEMPORAL: Reemplaza tu alerta por esta para ver qué llega
                if (string.IsNullOrEmpty(valorTAG) || string.IsNullOrEmpty(valorNombre))
                {
                    string debugMsg = "TAG encontrado: " + (valorTAG ?? "NULL") + " | Nombre encontrado: " + (valorNombre ?? "NULL");
                    Response.Write("<script>alert('" + debugMsg + "');</script>");
                    return;
                }
                // Validación de campos vacíos
                //if (string.IsNullOrEmpty(valorTAG) || string.IsNullOrEmpty(valorNombre) ||
                //    string.IsNullOrEmpty(txtactividad.Text) || ddlGrupo.SelectedIndex == 0 || ddlPrioridad.SelectedIndex == 0)
                //{
               //     Response.Write("<script>alert('Por favor, complete todos los campos requeridos.');</script>");
                //    return;
                //}

                // 3. ASIGNAR PARÁMETROS AL SQLDATASOURCE
                SqlDataSource3.InsertParameters["TAG"].DefaultValue = valorTAG;
                SqlDataSource3.InsertParameters["Nombre"].DefaultValue = valorNombre;
                SqlDataSource3.InsertParameters["Actividad"].DefaultValue = ddlactividad.SelectedValue;
                SqlDataSource3.InsertParameters["IDarea"].DefaultValue = ddlGrupo.SelectedValue;
                SqlDataSource3.InsertParameters["IDprioridad"].DefaultValue = ddlPrioridad.SelectedValue;
                SqlDataSource3.InsertParameters["EsObligatorio"].DefaultValue = ddlObliga.SelectedValue;
                SqlDataSource3.InsertParameters["TipoAnalisis"].DefaultValue = ddlAnalisis.SelectedValue;
                try
                {
                    // 4. Ejecutar la inserción
                    SqlDataSource3.Insert();

                    // 5. Limpieza (Opcional, ya que el GridView suele recargarse)
                    //ddlactividad.Text = "";
                    // Nota: Los ListBox/Select2 se limpian solos al recargar el GridView
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error al insertar: " + ex.Message.Replace("'", "") + "');</script>");
                }
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Response.Redirect("Closesession.aspx");
        }

        protected void txtactividad_TextChanged(object sender, EventArgs e)
        {

        }

        protected void gvinstrumentos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btninsertinstrumento_Click(object sender, EventArgs e)
        {

        }

        protected void gvinstrumentos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                ListBox lbxTag = (ListBox)e.Row.FindControl("lbxtag");
                ListBox lbxNombre = (ListBox)e.Row.FindControl("lbxnombre");

                if (lbxTag != null && lbxNombre != null)
                {
                    DataView dvLocal = (DataView)SqlDataSource3.Select(DataSourceSelectArguments.Empty);

                    if (dvLocal != null)
                    {
                        HashSet<string> tagsExistentes = new HashSet<string>();
                        foreach (DataRow row in dvLocal.Table.Rows)
                        {
                            tagsExistentes.Add(row["TAG"].ToString().Trim().ToUpper());
                        }

                        for (int i = lbxTag.Items.Count - 1; i >= 0; i--)
                        {
                            string tagDonador = lbxTag.Items[i].Value.Trim().ToUpper();
                            if (tagsExistentes.Contains(tagDonador))
                            {
                                lbxTag.Items.RemoveAt(i);
                                lbxNombre.Items.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }
    }
}