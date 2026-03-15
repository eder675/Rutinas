<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Generadorrutinas.aspx.cs" Inherits="Rutinas.Generadorrutinas" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Creando Formulario</title>
    <link rel="stylesheet" href="estilosgenerador.css"/>
    <style type="text/css">
        .auto-style1 {
            width: 10%;
            height: 46px;
        }
        .auto-style2 {
            width: 20%;
            height: 46px;
        }
        .auto-style3 {
            width: 29%;
            height: 46px;
        }
        .auto-style4 {
            width: 8%;
            text-align: center;
            height: 46px;
        }
        .auto-style5 {
            width: 25%;
            height: 46px;
        }
        .auto-style6 {
            width: 10%;
            height: 54px;
        }
        .auto-style7 {
            width: 20%;
            height: 54px;
        }
        .auto-style8 {
            width: 29%;
            height: 54px;
        }
        .auto-style9 {
            width: 8%;
            text-align: center;
            height: 54px;
        }
        .auto-style10 {
            width: 25%;
            height: 54px;
        }
    </style>
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
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Firma: _______________<br />
        <asp:Label ID="Label4" runat="server" Text="Fecha:"></asp:Label>
&nbsp;<asp:Label ID="lblfecha" runat="server" Text="fecha actual"></asp:Label>
&nbsp;<br />
        <asp:Label ID="Label5" runat="server" Text="Turno"></asp:Label>
        :
        <asp:Label ID="lblturno" runat="server" Text="Turno Actual"></asp:Label>
        <br />
        
    <h3>DETALLE DE INSTRUMENTOS</h3>
    
    <table class="tabla-instrumentos" cellspacing="0" cellpadding="0">
        <asp:Repeater ID="rptRutina" runat="server">
            
            <HeaderTemplate>
                <thead>
                    <tr>
                        <th class="col-area">ÁREA</th> 
                        <th class="col-equipo">EQUIPO</th>
                        <th class="col-detalle">DETALLE A VERIFICAR</th>
                        <th class="col-lectura">LECTURA ACTUAL</th>
                        <th class="col-hora">HORA LECTURA</th>
                        <th class="col-observaciones">OBSERVACIONES</th>
                    </tr>
                </thead>
                <tbody>
            </HeaderTemplate>

            <ItemTemplate>
                <tr>
                    <td class="col-area">
                        <asp:Label ID="lblArea" runat="server" Text='<%# Eval("NombreArea") %>' />
                    </td>
                    
                    <td class="col-equipo"> 
                        <asp:Label ID="lblEquipo" runat="server" Text='<%# Eval("NombreInstrumento") %>' />
                    </td>
                    
                    <td class="col-detalle"> 
                        <asp:Label ID="lblActividad" runat="server" Text='<%# Eval("Actividad") %>' />
                    </td>
                    
                    <td class="col-lectura">
                        <div class="linea-manual-pequena"></div>
                    </td>
                    
                    <td class="col-hora">
                        <div class="linea-manual-pequena"></div>
                    </td>
                     
                    <td class="col-observaciones">
                        <div class="linea-manual-larga"></div>
                    </td>

                </tr>
            </ItemTemplate>

            <FooterTemplate>
                </tbody>
      
    </FooterTemplate>
           
        </asp:Repeater>
</table> 

    <h3>DETALLE DE COMPROBACIONES OBLIGATORIAS</h3>
    
    <table class="tabla-comprobaciones" cellspacing="0" cellpadding="0">
        <asp:Repeater ID="rptObligatorios" runat="server">
            
            <HeaderTemplate>
                <thead>
                    <tr>
                        <th class="col-area">ÁREA</th> 
                        <th class="col-equipo">EQUIPO</th>
                        <th class="col-lecturaeq">LECTURA EQUIPO</th>
                        <th class="col-lecturalab">LECTURA LAB</th>
                        <th class="col-hora">HORA LECTURA</th>
                        <th class="col-evidencia">EVIDENCIA</th>
                        <th class="col-observaciones">OBSERVACIONES</th>
                    </tr>
                </thead>
                <tbody>
            </HeaderTemplate>

            <ItemTemplate>
                <tr>
                    <td class="col-area">
                        <asp:Label ID="lblArea0" runat="server" Text='<%# Eval("NombreArea") %>' />
                    </td>
                    
                    <td class="col-equipo"> 
                        <asp:Label ID="lblEquipo0" runat="server" Text='<%# Eval("NombreInstrumento") %>' />
                    </td>
                    
                    <td class="col-lecturaeq"> 
                        <div class="linea-manual-pequena"></div>
                    </td>
                    
                    <td class="col-lecturalab"> 
                        <div class="linea-manual-pequena"></div>
                    </td>
                    
                    <td class="col-hora">
                        <div class="linea-manual-pequena"></div>
                    </td>

                    <td class="col-evidencia">
                        <div class="linea-manual-pequena"></div>
                    </td> 

                    <td class="col-observaciones">
                        <div class="linea-manual-larga"></div>
                    </td>

                    

                </tr>
            </ItemTemplate>

            <FooterTemplate>
                </tbody>
      
    </FooterTemplate>
           
        </asp:Repeater>
</table> 

        <br />
        
    </form>
</body>
</html>
