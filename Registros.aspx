<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registros.aspx.cs" Inherits="Rutinas.Registros" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString2 %>" ProviderName="<%$ ConnectionStrings:REPORTESConnectionString2.ProviderName %>" SelectCommand="SELECT [Fecha_generacion], [Correlativo], [Codigo_empleado], [Codigo_turno], [Area_asignada] FROM [Rutina]"></asp:SqlDataSource>
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="#CCCCCC" BorderColor="#999999" BorderStyle="Solid" BorderWidth="3px" CellPadding="4" CellSpacing="2" DataKeyNames="Correlativo" DataSourceID="SqlDataSource1" ForeColor="Black">
                <Columns>
                    <asp:BoundField DataField="Fecha_generacion" HeaderText="Fecha_generacion" SortExpression="Fecha_generacion" />
                    <asp:BoundField DataField="Correlativo" HeaderText="Correlativo" InsertVisible="False" ReadOnly="True" SortExpression="Correlativo" />
                    <asp:BoundField DataField="Codigo_empleado" HeaderText="Codigo_empleado" SortExpression="Codigo_empleado" />
                    <asp:BoundField DataField="Codigo_turno" HeaderText="Codigo_turno" SortExpression="Codigo_turno" />
                    <asp:BoundField DataField="Area_asignada" HeaderText="Area_asignada" SortExpression="Area_asignada" />
                </Columns>
                <FooterStyle BackColor="#CCCCCC" />
                <HeaderStyle BackColor="Black" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#CCCCCC" ForeColor="Black" HorizontalAlign="Left" />
                <RowStyle BackColor="White" />
                <SelectedRowStyle BackColor="#000099" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                <SortedAscendingHeaderStyle BackColor="#808080" />
                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                <SortedDescendingHeaderStyle BackColor="#383838" />
            </asp:GridView>
        </div>
    </form>
</body>
</html>
