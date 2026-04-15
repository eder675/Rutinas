<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registrofallos.aspx.cs" Inherits="Rutinas.Registrofallos" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="Label1" runat="server" Text="PAGINA DE REGISTROS DE FALLOS DE INSTRUMENTOS"></asp:Label>
            <br />
        </div>
        <asp:Label ID="Label2" runat="server" Text="INSTRUMENTO:"></asp:Label>
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        <p>
            <asp:Label ID="Label3" runat="server" Text="¿FALLO INDUCIDO? (determina si el fallo fue por causas naturales o  si tiene que ver con acciones humanas)"></asp:Label>
        </p>
        <p>
            <asp:RadioButton ID="RadioButton1" runat="server" Text="SI" />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:RadioButton ID="RadioButton2" runat="server" Text="NO" />
        </p>
        <asp:Label ID="Label4" runat="server" Text="NIVEL DE DAÑO: (determina si fue a nivel de su electronica, transmisor esclavo, accesorio, software o pieza no reemplazable)"></asp:Label>
        <br />
        <asp:DropDownList ID="DropDownList1" runat="server">
        </asp:DropDownList>
        <br />
        <br />
        <asp:Label ID="Label8" runat="server" Text="TIPO DE DAÑO: (indica el tipo de daño que recibio el equipo)"></asp:Label>
        <br />
        <asp:DropDownList ID="DropDownList2" runat="server">
        </asp:DropDownList>
        <br />
        <p>
            <asp:Label ID="Label5" runat="server" Text="¿REEMPLAZAR POR NUEVO EL PROXIMO MANTENIMIENTO? (indica si el equipo es necesario cambiarlo por uno nuevo) "></asp:Label>
        </p>
        <p>
            <asp:RadioButton ID="RadioButton3" runat="server" Text="SI" />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:RadioButton ID="RadioButton4" runat="server" Text="NO (se puede reparar)" />
        </p>
        <asp:Label ID="Label6" runat="server" Text="INTERVIENE EN EL PROCESO: "></asp:Label>
        <br />
        <br />
        <p>
            <asp:Label ID="Label7" runat="server" Text="SOLUCION ACTUAL: (determina que tipo de solucion se le ha dado actualmente)"></asp:Label>
        </p>
        <p>
            <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        </p>
    </form>
</body>
</html>
