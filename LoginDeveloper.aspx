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
        <asp:MultiView ID="mvadmin" runat="server" OnActiveViewChanged="MultiView1_ActiveViewChanged" ActiveViewIndex="0">
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
                <asp:GridView ID="gvempleados" runat="server" AutoGenerateColumns="False" DataKeyNames="Codigo_empleado" DataSourceID="SqlDataSource2" ShowFooter="True">
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
                                <asp:DropDownList ID="DropDownList1" runat="server" DataSourceID="SqlDataSource1">
                                </asp:DropDownList>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Cargo") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:TemplateField></asp:TemplateField>
                        <asp:CommandField ButtonType="Button" ShowDeleteButton="True" ShowEditButton="True" />
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
                <asp:GridView ID="gvarea" runat="server" AutoGenerateColumns="False" DataKeyNames="IDarea" DataSourceID="SqlDataSource1" AutoGenerateDeleteButton="true" AutoGenerateEditButton="True" ShowFooter="True"> 
                    <Columns>
                        <asp:BoundField DataField="IDarea" HeaderText="IDarea" InsertVisible="False" ReadOnly="True" SortExpression="IDarea" />
                        <asp:BoundField DataField="Nombre" HeaderText="Nombre" SortExpression="Nombre" />
                        <asp:BoundField DataField="IDgrupo" HeaderText="IDgrupo" SortExpression="IDgrupo" />
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
                <asp:GridView ID="gvinstrumentos" runat="server" AutoGenerateColumns="False" DataKeyNames="TAG" DataSourceID="SqlDataSource3" AutoGenerateDeleteButton="true" AutoGenerateEditButton="True" ShowFooter="True">
                    <Columns>
                        <asp:BoundField DataField="TAG" HeaderText="TAG" ReadOnly="True" SortExpression="TAG" />
                        <asp:BoundField DataField="Nombre" HeaderText="Nombre" SortExpression="Nombre" />
                        <asp:BoundField DataField="Actividad" HeaderText="Actividad" SortExpression="Actividad" />
                        <asp:BoundField DataField="IDarea" HeaderText="IDarea" SortExpression="IDarea" />
                        <asp:BoundField DataField="IDprioridad" HeaderText="IDprioridad" SortExpression="IDprioridad" />
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
