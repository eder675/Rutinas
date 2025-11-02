<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Generadorrutinas.aspx.cs" Inherits="Rutinas.Generadorrutinas" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Creando Formulario</title>
    <link rel="stylesheet" href="estilosgenerador.css"/>
</head>
<body>
    <form id="form1" runat="server">
        
        <asp:Label ID="Label1" runat="server" Text="INGENIO LA CABAÑA"></asp:Label>
        <br />
        <asp:Label ID="Label2" runat="server" Text="DEPARTAMENTO DE TECNOLOGIA INDUSTRIAL"></asp:Label>
        <br />
        <asp:Label ID="Label3" runat="server" Text="LISTA DE VERIFICACION DE RUTINA DE INSTRUMENTISTA"></asp:Label>
        <br />
        Instrumentista:
        <asp:Label ID="lblname" runat="server" Text="Nombre"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Firma: _______________<br />
        <asp:Label ID="Label4" runat="server" Text="Fecha:"></asp:Label>
&nbsp;<asp:Label ID="lblfecha" runat="server" Text="fecha actual"></asp:Label>
&nbsp;<br />
        <asp:Label ID="Label5" runat="server" Text="Turno"></asp:Label>
        :
        <asp:Label ID="lblturno" runat="server" Text="Turno Actual"></asp:Label>
        <br />
        <br />
        
    </form>
</body>
</html>
