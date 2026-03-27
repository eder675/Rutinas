<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginDeveloper.aspx.cs" EnableEventValidation="false" Inherits="Rutinas.LoginDeveloper" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <link rel="stylesheet" href="styleslogin.css" />
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            </div>
    <header>
    <h1>Pagina de administracion.</h1>
        <h2>
            <asp:Label ID="lblnameadmin" runat="server" Text="Label"></asp:Label>
        </h2>
</header>
<nav>
    <ul class="menu">
        <li>
        <asp:LinkButton 
            ID="lnkRutinas" 
            runat="server" 
            Text="Historial de generacion." 
            OnClick="lnk_Click" 
            CommandArgument="0" />
    </li>
    <li>
        <asp:LinkButton 
            ID="lnkEmpleados" 
            runat="server" 
            Text="Tabla de empleados." 
            OnClick="lnk_Click" 
            CommandArgument="1" />
    </li>
    <li>
        <asp:LinkButton 
            ID="lnkAreas" 
            runat="server" 
            Text="Tabla de areas." 
            OnClick="lnk_Click" 
            CommandArgument="2" />
    </li>
    <li>
        <asp:LinkButton 
            ID="lnkInstrumentos" 
            runat="server" 
            Text="Tabla de instrumentos." 
            OnClick="lnk_Click" 
            CommandArgument="3" />
    </li>
        <li>
            <asp:LinkButton 
            ID="lnkLogout" 
            runat="server" 
            Text="Cerrar sesion." 
            OnClick="lnkLogout_Click" />
        </li>
    </ul>
</nav>
    <p>
        <asp:MultiView ID="mvadmin" runat="server"  ActiveViewIndex="0">
            <asp:View ID="vrutinas" runat="server">
                <asp:GridView ID="gvrutinas" runat="server" DataSourceID="runtines" AutoGenerateColumns="False" DataKeyNames="Correlativo">
                    <Columns>
                        <asp:BoundField DataField="Correlativo" HeaderText="Correlativo" ReadOnly="True" SortExpression="Correlativo" InsertVisible="False" />
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" SortExpression="Fecha" />
                        <asp:BoundField DataField="Turno" HeaderText="Turno" SortExpression="Turno" />
                        <asp:BoundField DataField="Codigo_empleado" HeaderText="Codigo_empleado" SortExpression="Codigo_empleado" />
                        <asp:BoundField DataField="IDgrupo" HeaderText="IDgrupo" SortExpression="IDgrupo" />
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="runtines" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT * FROM [Rutinas]">
                </asp:SqlDataSource>
                <br />
            </asp:View>
            <asp:View ID="vempleados" runat="server">
                <asp:GridView ID="gvempleados" runat="server" AutoGenerateColumns="False" DataKeyNames="Codigo_empleado" DataSourceID="SqlDataSource2" ShowFooter="True" OnRowCommand="gvempleados_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Codigo_empleado" SortExpression="Codigo_empleado">
                            <EditItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("Codigo_empleado") %>'></asp:Label>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="txtnewcode" runat="server"></asp:TextBox>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("Codigo_empleado") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nombre" SortExpression="Nombre">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Nombre") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="txtnewname" runat="server"></asp:TextBox>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Cargo" SortExpression="Cargo">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Cargo") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlinsertnew" runat="server" DataSourceID="SqlDataSourceCargos" DataTextField="Cargo" DataValueField="Cargo">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceCargos" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT DISTINCT [Cargo] FROM [Empleado]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Cargo") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <EditItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Actualizar" />
                                &nbsp;<asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancelar" />
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Button ID="btninsertnew" runat="server" CommandName="insertnewempleado" Text="INSERTAR" />
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="False" CommandName="Edit" Text="Editar" />
                                &nbsp;<asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Delete" Text="Eliminar" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" DeleteCommand="DELETE FROM [Empleado] WHERE [Codigo_empleado] = @Codigo_empleado" InsertCommand="INSERT INTO [Empleado] ([Codigo_empleado], [Nombre], [Cargo]) VALUES (@Codigo_empleado, @Nombre, @Cargo)" SelectCommand="SELECT * FROM [Empleado]" UpdateCommand="UPDATE [Empleado] SET [Nombre] = @Nombre, [Cargo] = @Cargo WHERE [Codigo_empleado] = @Codigo_empleado">
                    <DeleteParameters>
                        <asp:Parameter Name="Codigo_empleado" Type="String" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="Codigo_empleado" Type="String" />
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Cargo" Type="String" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Cargo" Type="String" />
                        <asp:Parameter Name="Codigo_empleado" Type="String" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </asp:View>
            <asp:View ID="vareas" runat="server">
                <asp:GridView ID="gvarea" runat="server" AutoGenerateColumns="False" DataKeyNames="IDarea" DataSourceID="SqlDataSource1" ShowFooter="True" OnRowCommand="gvarea_RowCommand"> 
                    <Columns>
                        <asp:TemplateField HeaderText="IDarea" InsertVisible="False" SortExpression="IDarea">
                            <EditItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("IDarea") %>'></asp:Label>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="Label6" runat="server" Text="Campo Automatico"></asp:Label>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("IDarea") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nombre" SortExpression="Nombre">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Nombre") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="txtnewarea" runat="server"></asp:TextBox>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="IDgrupo" SortExpression="IDgrupo">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("IDgrupo") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlnewgrupo" runat="server" DataSourceID="SqlDataSourceGrupo" DataTextField="Nombregrupo" DataValueField="IDgrupo">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceGrupo" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDgrupo], [Nombregrupo] FROM [Rotaciongrupos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Nombregrupo") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <EditItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Actualizar" />
                                &nbsp;<asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancelar" />
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Button ID="btnnewinsert" runat="server" Text="INSERTAR" CommandName="insertnewarea" />
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="False" CommandName="Edit" Text="Editar" />
                                &nbsp;<asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Delete" Text="Eliminar" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" DeleteCommand="DELETE FROM [Area] WHERE [IDarea] = @IDarea" InsertCommand="INSERT INTO [Area] ([Nombre], [IDgrupo]) VALUES (@Nombre, @IDgrupo)" SelectCommand="SELECT 
    A.IDarea, 
    A.Nombre, 
    G.Nombregrupo, 
    A.IDgrupo
FROM Area A
INNER JOIN Rotaciongrupos G ON A.IDgrupo = G.IDgrupo" UpdateCommand="UPDATE [Area] SET [Nombre] = @Nombre, [IDgrupo] = @IDgrupo WHERE [IDarea] = @IDarea">
                    <DeleteParameters>
                        <asp:Parameter Name="IDarea" Type="Int32" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="IDgrupo" Type="Int32" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="IDgrupo" Type="Int32" />
                        <asp:Parameter Name="IDarea" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </asp:View>
            <asp:View ID="vinstrumentos" runat="server">
                <asp:Label ID="Label7" runat="server" Text="LISTA DE INSTRUMENTOS"></asp:Label>
                <br />
                <asp:GridView ID="gvinstrumentos" runat="server" AutoGenerateColumns="False" DataKeyNames="TAG" DataSourceID="SqlDataSource3" ShowFooter="True" OnRowCommand="gvinstrumentos_RowCommand" OnSelectedIndexChanged="gvinstrumentos_SelectedIndexChanged" OnRowDataBound="gvinstrumentos_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="TAG" SortExpression="TAG">
                            <EditItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("TAG") %>'></asp:Label>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:ListBox ID="lbxtag" runat="server" CssClass="select2-busqueda" DataSourceID="SqlDataSourceDonador" DataTextField="TAG" DataValueField="TAG"></asp:ListBox>
                                <asp:SqlDataSource ID="SqlDataSourceDonador" runat="server" ConnectionString="<%$ ConnectionStrings:VinetasConnectionString %>" ProviderName="<%$ ConnectionStrings:VinetasConnectionString.ProviderName %>" SelectCommand="SELECT [TAG] FROM [equipos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("TAG") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nombre" SortExpression="Nombre">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Nombre") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:ListBox ID="lbxnombre" runat="server" CssClass="select2-busqueda-desc" DataSourceID="SqlDataSourcedescripciondonador" DataTextField="Descripcion" DataValueField="Descripcion"></asp:ListBox>
                                <asp:SqlDataSource ID="SqlDataSourcedescripciondonador" runat="server" ConnectionString="<%$ ConnectionStrings:VinetasConnectionString %>" SelectCommand="SELECT [Descripcion] FROM [equipos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actividad" SortExpression="Actividad">
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList2" runat="server" DataSourceID="SqlDataSource4" DataTextField="Actividad" DataValueField="Actividad" SelectedValue='<%# Bind("Actividad") %>'>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSource4" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT DISTINCT [Actividad] FROM [Instrumentos]"></asp:SqlDataSource>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlactividad" runat="server" DataSourceID="SqlDataSourceactividad" DataTextField="Actividad" DataValueField="Actividad">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceactividad" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT DISTINCT [Actividad] FROM [Instrumentos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Actividad") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="IDarea" SortExpression="IDarea">
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList1" runat="server" DataSourceID="SqlDataSourceEditAreas" DataTextField="Nombre" DataValueField="IDarea" SelectedValue='<%# Bind("IDarea") %>'>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceEditAreas" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDarea], [Nombre] FROM [Area]"></asp:SqlDataSource>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlarea" runat="server" DataSourceID="SqlDataSourceIDAreas" DataTextField="Nombre" DataValueField="IDarea">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceIDAreas" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDarea], [Nombre] FROM [Area]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("Expr1") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="IDprioridad" SortExpression="IDprioridad">
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList5" runat="server" DataSourceID="SqlDataSourceEditPrioridad" DataTextField="Nombre" DataValueField="IDprioridad" SelectedValue='<%# Bind("IDprioridad") %>'>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceEditPrioridad" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDprioridad], [Nombre] FROM [Prioridad]"></asp:SqlDataSource>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlprioridad" runat="server" DataSourceID="SqlDataSourcePrioridad" DataTextField="Nombre" DataValueField="IDprioridad">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourcePrioridad" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDprioridad], [Nombre] FROM [Prioridad]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label5" runat="server" Text='<%# Bind("Expr2") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="EsObligatorio" SortExpression="Obligatorio">
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList3" runat="server" DataSourceID="SqlDataSourceobliga" DataTextField="EsObligatorio" DataValueField="EsObligatorio" SelectedValue='<%# Bind("EsObligatorio") %>'>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceobliga" runat="server" ConnectionString="<%$ ConnectionStrings:ConexionRutinasMTI %>" SelectCommand="SELECT DISTINCT [EsObligatorio] FROM [Instrumentos]"></asp:SqlDataSource>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlobliga" runat="server" DataSourceID="SqlDataSourceobliatorio" DataTextField="EsObligatorio" DataValueField="EsObligatorio">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceobliatorio" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString %>" SelectCommand="SELECT DISTINCT [EsObligatorio] FROM [Instrumentos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label8" runat="server" Text='<%# Bind("EsObligatorio") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="TipoAnalisis" SortExpression="Tipoanalisis">
                            <EditItemTemplate>
                                <asp:DropDownList ID="DropDownList4" runat="server" DataSourceID="SqlDataSourceTipoanalisisddl" DataTextField="TipoAnalisis" DataValueField="TipoAnalisis" SelectedValue='<%# Bind("TipoAnalisis") %>'>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceTipoanalisisddl" runat="server" ConnectionString="<%$ ConnectionStrings:ConexionRutinasMTI %>" SelectCommand="SELECT DISTINCT [TipoAnalisis] FROM [Instrumentos]"></asp:SqlDataSource>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlanalisis" runat="server" DataSourceID="SqlDataSourcetipoanalisis" DataTextField="TipoAnalisis" DataValueField="TipoAnalisis">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourcetipoanalisis" runat="server" ConnectionString="<%$ ConnectionStrings:ConexionRutinasMTI %>" SelectCommand="SELECT DISTINCT [TipoAnalisis] FROM [Instrumentos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label9" runat="server" Text='<%# Bind("TipoAnalisis") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <EditItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Actualizar" />
                                <asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancelar" />
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Button ID="btninsertinstrumento" runat="server" CommandName="insertnewinstrumentos" OnClick="btninsertinstrumento_Click" Text="INSERTAR" />
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="False" CommandName="Edit" Text="Editar" />
                                <asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Delete" Text="Eliminar" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>"
                    DeleteCommand="DELETE FROM [Instrumentos] WHERE [TAG] = @TAG"
                    InsertCommand="INSERT INTO [Instrumentos] ([TAG], [Nombre], [Actividad], [IDarea], [IDprioridad], [EsObligatorio], [TipoAnalisis]) VALUES (@TAG, @Nombre, @Actividad, @IDarea, @IDprioridad, @EsObligatorio, @TipoAnalisis)"
                    SelectCommand="SELECT I.TAG, I.Nombre, I.Actividad, I.IDarea, I.IDprioridad, A.Nombre AS Expr1, P.Nombre AS Expr2, I.EsObligatorio, I.TipoAnalisis
                        FROM Instrumentos I
                        INNER JOIN Prioridad P ON I.IDprioridad = P.IDprioridad
                        INNER JOIN Area A ON I.IDarea = A.IDarea
                        ORDER BY I.IDarea"
                    UpdateCommand="UPDATE [Instrumentos] SET [Nombre] = @Nombre, [Actividad] = @Actividad, [IDarea] = @IDarea, [IDprioridad] = @IDprioridad, [EsObligatorio] = @EsObligatorio, [TipoAnalisis] = @TipoAnalisis WHERE [TAG] = @TAG">
                    <DeleteParameters>
                        <asp:Parameter Name="TAG" Type="String" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="TAG" Type="String" />
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Actividad" Type="String" />
                        <asp:Parameter Name="IDarea" Type="Int32" />
                        <asp:Parameter Name="IDprioridad" Type="Int32" />
                        <asp:Parameter Name="EsObligatorio" Type="String" />
                        <asp:Parameter Name="TipoAnalisis" Type="String" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Actividad" Type="String" />
                        <asp:Parameter Name="IDarea" Type="Int32" />
                        <asp:Parameter Name="IDprioridad" Type="Int32" />
                        <asp:Parameter Name="TAG" Type="String" />
                        <asp:Parameter Name="EsObligatorio" Type="String" />
                        <asp:Parameter Name="TipoAnalisis" Type="String" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </asp:View>
        </asp:MultiView>
        </p>


        <script type="text/javascript">
        // Deshabilitar la restauracion automatica de scroll del navegador.
        // Sin esto, la restauracion nativa (history.scrollRestoration='auto')
        // puede ejecutarse despues del timeout de 500ms y cancelar el scrollTo,
        // impidiendo que la pagina baje al footer del grid tras un postback completo.
        if ('scrollRestoration' in history) {
            history.scrollRestoration = 'manual';
        }

        function setupSincronizacion() {
            var $selectTag = $('.select2-busqueda');
            var $selectDesc = $('.select2-busqueda-desc');

            if ($selectTag.length === 0) return;

            $selectTag.select2({ tags: true, placeholder: "TAG...", width: '100%' });
            $selectDesc.select2({ tags: true, placeholder: "Descripcion...", width: '100%' });

            // Al hacer clic en cualquiera de los dos campos, abrir el dropdown
            // y enfocar automaticamente el campo de busqueda interno de Select2
            $(document).off('select2:open').on('select2:open', function () {
                setTimeout(function () {
                    var campo = document.querySelector('.select2-container--open .select2-search__field');
                    if (campo) campo.focus();
                }, 50);
            });

            // Sincronizar por indice: seleccionar TAG mueve Descripcion y viceversa
            $selectTag.off('select2:select').on('select2:select', function (e) {
                var index = e.params.data.element.index;
                $selectDesc.prop('selectedIndex', index).trigger('change.select2');
            });

            $selectDesc.off('select2:select').on('select2:select', function (e) {
                var index = e.params.data.element.index;
                $selectTag.prop('selectedIndex', index).trigger('change.select2');
            });
        }

        function scrollAlBuscador() {
            // Se usa window.scrollTo con posicion absoluta en lugar de scrollIntoView
            // porque en postbacks completos de Web Forms el navegador restaura el
            // scroll despues de que scrollIntoView ya ejecuto, sobreescribiendo el
            // resultado. Calcular la coordenada Y y llamar window.scrollTo con un
            // timeout de 500ms garantiza que corre despues de la restauracion nativa.
            setTimeout(function () {
                var footer = document.querySelector('#gvinstrumentos tfoot');
                if (!footer) return;
                var y = footer.getBoundingClientRect().top + window.pageYOffset;
                window.scrollTo({ top: y, behavior: 'smooth' });
            }, 500);
        }

        // window.onload (no document.ready) para garantizar que el layout
        // ya esta calculado y el footer tiene su posicion final en el DOM.
        $(window).on('load', function () {
            setupSincronizacion();
            scrollAlBuscador();
        });

        if (typeof (Sys) !== 'undefined') {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_endRequest(function () {
                setupSincronizacion();
                scrollAlBuscador();
            });
        }
    </script>
</form>


    <footer class="footer-acerca">
    <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
</footer>
</body>
</html>
