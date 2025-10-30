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
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:REPORTESConnectionString %>" DeleteCommand="DELETE FROM [Rutina_Instrumento] WHERE [Correlativo_Rutina] = @Correlativo_Rutina AND [Correlativo_Instrumento] = @Correlativo_Instrumento" InsertCommand="INSERT INTO [Rutina_Instrumento] ([Correlativo_Rutina], [Correlativo_Instrumento], [Estado]) VALUES (@Correlativo_Rutina, @Correlativo_Instrumento, @Estado)" ProviderName="<%$ ConnectionStrings:REPORTESConnectionString.ProviderName %>" SelectCommand="SELECT [Correlativo_Rutina], [Correlativo_Instrumento], [Estado] FROM [Rutina_Instrumento] ORDER BY [Correlativo_Instrumento]" UpdateCommand="UPDATE [Rutina_Instrumento] SET [Estado] = @Estado WHERE [Correlativo_Rutina] = @Correlativo_Rutina AND [Correlativo_Instrumento] = @Correlativo_Instrumento">
                <DeleteParameters>
                    <asp:Parameter Name="Correlativo_Rutina" Type="Int32" />
                    <asp:Parameter Name="Correlativo_Instrumento" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:Parameter Name="Correlativo_Rutina" Type="Int32" />
                    <asp:Parameter Name="Correlativo_Instrumento" Type="Int32" />
                    <asp:Parameter Name="Estado" Type="String" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Estado" Type="String" />
                    <asp:Parameter Name="Correlativo_Rutina" Type="Int32" />
                    <asp:Parameter Name="Correlativo_Instrumento" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False" BackColor="White" BorderColor="#336666" BorderStyle="Double" BorderWidth="3px" CellPadding="4" DataKeyNames="Correlativo_Rutina,Correlativo_Instrumento" DataSourceID="SqlDataSource1" GridLines="Horizontal">
                <Columns>
                    <asp:BoundField DataField="Correlativo_Rutina" HeaderText="Correlativo_Rutina" ReadOnly="True" SortExpression="Correlativo_Rutina" />
                    <asp:BoundField DataField="Correlativo_Instrumento" HeaderText="Correlativo_Instrumento" ReadOnly="True" SortExpression="Correlativo_Instrumento" />
                    <asp:BoundField DataField="Estado" HeaderText="Estado" SortExpression="Estado" />
                </Columns>
                <FooterStyle BackColor="White" ForeColor="#333333" />
                <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
                <RowStyle BackColor="White" ForeColor="#333333" />
                <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                <SortedAscendingHeaderStyle BackColor="#487575" />
                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                <SortedDescendingHeaderStyle BackColor="#275353" />
            </asp:GridView>
        </div>
    </form>
</body>
</html>
