<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Rutinas.Login" %>

<!DOCTYPE html>
<div id="divNotificacion" 
    style="display:none; 
           position: fixed; 
           top: 20px; 
           right: 20px; 
           padding: 15px; 
           background-color: #4CAF50; /* Color de éxito, cámbialo si es error */
           color: white; 
           border-radius: 5px; 
           z-index: 1000;
           box-shadow: 0 4px 8px rgba(0,0,0,0.2);">
    <span id="spanMensaje"></span>
</div>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Rutinas Cabana</title>
    <link rel="stylesheet" href="styles-shared.css"/>
    <link rel="stylesheet" href="styles.css" />
    </head>
<body>
    
        <form id="form1" runat="server">
    
        <div class="auto-style1">
            <h1>Bienvenido a Rutinas MTI</h1>
            <p>
                <asp:TextBox ID="txtcodigo" runat="server" TextMode="Password" placeholder="Codigo de empleado" Width="220px"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtcodigo" ErrorMessage="Complete este campo" ForeColor="Red"></asp:RequiredFieldValidator>
            </p>
            <p>
                <asp:Button ID="btnentrar" runat="server" Text="INICIAR SESION" OnClick="btnentrar_Click" />
                <asp:Button ID="btnAbout" runat="server" BorderColor="#CCFF66" BorderStyle="Outset" OnClick="Button1_Click" Text="Acerca de." CausesValidation="False" Width="143px" />
            </p>
            <footer>
                <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
            </footer>
        </div>

        </form>  
    <script src="shared.js?v=2"></script>
</body>
</html>
