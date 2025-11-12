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
        <asp:Label ID="Label6" runat="server" Text="Instrumentista:"></asp:Label>
&nbsp;<asp:Label ID="lblname" runat="server" Text="Nombre"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Firma: _______________<br />
        <asp:Label ID="Label4" runat="server" Text="Fecha:"></asp:Label>
&nbsp;<asp:Label ID="lblfecha" runat="server" Text="fecha actual"></asp:Label>
&nbsp;<br />
        <asp:Label ID="Label5" runat="server" Text="Turno"></asp:Label>
        :
        <asp:Label ID="lblturno" runat="server" Text="Turno Actual"></asp:Label>
        <br />
        <asp:Repeater ID="rptrutina" runat="server">
            <HeaderTemplate>
        <table class="tabla-instrumentos" cellspacing="0" cellpadding="0">
            <thead>
                <tr>
                    <th class="col-area">AREA</th>
                    <th>EQUIPO</th>
                    <th>DETALLE A VERIFICAR</th>
                    <th class="col-lectura">LECTURA ACTUAL</th>
                    <th class="col-hora">HORA LECTURA</th>
                    <th class="col-observaciones">OBSERVACIONES</th>
                </tr>
            </thead>
            <tbody>
    </HeaderTemplate>

    <ItemTemplate>
        <tr>
            <td>
                <asp:Label ID="lblInstrumento" runat="server" 
                           Text='<%# Eval("NombreInstrumento") %>' />
            </td>
            
            <td class="col-lectura">
                <div class="casilla-verif-small"></div>
            </td>
            
            <td class="col-hora">
                <div class="linea-manual"></div>
            </td>
            
            <td class="col-observaciones">
                <div class="linea-manual-larga"></div>
            </td>
        </tr>
    </ItemTemplate>

    <FooterTemplate>
            </tbody>
        </table>
    </FooterTemplate>
        </asp:Repeater>
        <br />
        
    </form>
</body>
</html>
