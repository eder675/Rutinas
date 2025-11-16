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
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString2 %>" SelectCommand="SELECT [Correlativo], [Codigo_empleado], [Turno], [Fecha] FROM [Rutinas] ORDER BY [Correlativo] DESC" ConflictDetection="CompareAllValues" DeleteCommand="DELETE FROM [Rutinas] WHERE [Correlativo] = @original_Correlativo AND (([Codigo_empleado] = @original_Codigo_empleado) OR ([Codigo_empleado] IS NULL AND @original_Codigo_empleado IS NULL)) AND [Turno] = @original_Turno AND [Fecha] = @original_Fecha" InsertCommand="INSERT INTO [Rutinas] ([Codigo_empleado], [Turno], [Fecha]) VALUES (@Codigo_empleado, @Turno, @Fecha)" OldValuesParameterFormatString="original_{0}" UpdateCommand="UPDATE [Rutinas] SET [Codigo_empleado] = @Codigo_empleado, [Turno] = @Turno, [Fecha] = @Fecha WHERE [Correlativo] = @original_Correlativo AND (([Codigo_empleado] = @original_Codigo_empleado) OR ([Codigo_empleado] IS NULL AND @original_Codigo_empleado IS NULL)) AND [Turno] = @original_Turno AND [Fecha] = @original_Fecha">
                <DeleteParameters>
                    <asp:Parameter Name="original_Correlativo" Type="Int32" />
                    <asp:Parameter Name="original_Codigo_empleado" Type="String" />
                    <asp:Parameter Name="original_Turno" Type="String" />
                    <asp:Parameter Name="original_Fecha" Type="DateTime" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:Parameter Name="Codigo_empleado" Type="String" />
                    <asp:Parameter Name="Turno" Type="String" />
                    <asp:Parameter Name="Fecha" Type="DateTime" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Codigo_empleado" Type="String" />
                    <asp:Parameter Name="Turno" Type="String" />
                    <asp:Parameter Name="Fecha" Type="DateTime" />
                    <asp:Parameter Name="original_Correlativo" Type="Int32" />
                    <asp:Parameter Name="original_Codigo_empleado" Type="String" />
                    <asp:Parameter Name="original_Turno" Type="String" />
                    <asp:Parameter Name="original_Fecha" Type="DateTime" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" BackColor="#CCCCCC" BorderColor="#999999" BorderStyle="Solid" BorderWidth="3px" CellPadding="4" CellSpacing="2" DataKeyNames="Correlativo" DataSourceID="SqlDataSource1" ForeColor="Black">
                <Columns>
                    <asp:BoundField DataField="Correlativo" HeaderText="Correlativo" SortExpression="Correlativo" InsertVisible="False" ReadOnly="True" />
                    <asp:BoundField DataField="Codigo_empleado" HeaderText="Codigo_empleado" SortExpression="Codigo_empleado" />
                    <asp:BoundField DataField="Turno" HeaderText="Turno" SortExpression="Turno" />
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" SortExpression="Fecha" />
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
