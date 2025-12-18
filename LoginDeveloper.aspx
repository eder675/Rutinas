<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginDeveloper.aspx.cs" Inherits="Rutinas.LoginDeveloper" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <link rel="stylesheet" href="styleslogin.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            </div>
    <header>
    <h1>Pagina de administracion.</h1>
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
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <EditItemTemplate>
                                <asp:Button ID="Button1" runat="server" CausesValidation="True" CommandName="Update" Text="Actualizar" />
                                &nbsp;<asp:Button ID="Button2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancelar" />
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Button ID="btninsertnew" runat="server" CommandName="insertnew" Text="INSERTAR" />
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
                <asp:GridView ID="gvarea" runat="server" AutoGenerateColumns="False" DataKeyNames="IDarea" DataSourceID="SqlDataSource1" ShowFooter="True"> 
                    <Columns>
                        <asp:TemplateField HeaderText="IDarea" InsertVisible="False" SortExpression="IDarea">
                            <EditItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("IDarea") %>'></asp:Label>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="txtnewarea" runat="server"></asp:TextBox>
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
                                <asp:TextBox ID="txtnewnamearea" runat="server"></asp:TextBox>
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
                                <asp:DropDownList ID="DropDownList1" runat="server" DataSourceID="SqlDataSourceGrupo" DataTextField="Nombregrupo" DataValueField="IDgrupo">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSourceGrupo" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" SelectCommand="SELECT [IDgrupo], [Nombregrupo] FROM [Rotaciongrupos]"></asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("IDgrupo") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False"></asp:TemplateField>
                        <asp:TemplateField>
                            <FooterTemplate>
                                <asp:Button ID="Button3" runat="server" Text="Button" />
                            </FooterTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" DeleteCommand="DELETE FROM [Area] WHERE [IDarea] = @IDarea" InsertCommand="INSERT INTO [Area] ([Nombre], [IDgrupo]) VALUES (@Nombre, @IDgrupo)" SelectCommand="SELECT * FROM [Area]" UpdateCommand="UPDATE [Area] SET [Nombre] = @Nombre, [IDgrupo] = @IDgrupo WHERE [IDarea] = @IDarea">
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
                <asp:GridView ID="gvinstrumentos" runat="server" AutoGenerateColumns="False" DataKeyNames="TAG" DataSourceID="SqlDataSource3" ShowFooter="True">
                    <Columns>
                        <asp:TemplateField HeaderText="TAG" SortExpression="TAG">
                            <EditItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("TAG") %>'></asp:Label>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="txtnewtag" runat="server"></asp:TextBox>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("TAG") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nombre" SortExpression="Nombre">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Nombre") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("Nombre") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actividad" SortExpression="Actividad">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("Actividad") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Actividad") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="IDarea" SortExpression="IDarea">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("IDarea") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("IDarea") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="IDprioridad" SortExpression="IDprioridad">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("IDprioridad") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label5" runat="server" Text='<%# Bind("IDprioridad") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <FooterTemplate>
                                <asp:Button ID="btninsertinstrumento" runat="server" Text="INSERTAR" />
                            </FooterTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString3 %>" DeleteCommand="DELETE FROM [Instrumentos] WHERE [TAG] = @TAG" InsertCommand="INSERT INTO [Instrumentos] ([TAG], [Nombre], [Actividad], [IDarea], [IDprioridad]) VALUES (@TAG, @Nombre, @Actividad, @IDarea, @IDprioridad)" SelectCommand="SELECT * FROM [Instrumentos]" UpdateCommand="UPDATE [Instrumentos] SET [Nombre] = @Nombre, [Actividad] = @Actividad, [IDarea] = @IDarea, [IDprioridad] = @IDprioridad WHERE [TAG] = @TAG">
                    <DeleteParameters>
                        <asp:Parameter Name="TAG" Type="String" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="TAG" Type="String" />
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Actividad" Type="String" />
                        <asp:Parameter Name="IDarea" Type="Int32" />
                        <asp:Parameter Name="IDprioridad" Type="Int32" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Nombre" Type="String" />
                        <asp:Parameter Name="Actividad" Type="String" />
                        <asp:Parameter Name="IDarea" Type="Int32" />
                        <asp:Parameter Name="IDprioridad" Type="Int32" />
                        <asp:Parameter Name="TAG" Type="String" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </asp:View>
        </asp:MultiView>
        </p>


    </form>


    <footer class="footer-acerca">
    <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
</footer>
</body>
</html>
