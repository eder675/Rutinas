<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Rutinas.Login" %>

<!DOCTYPE html>
<style type="text/css">
    .auto-style1 {
        background-position: 0% 0%;
        padding: 20px 30px;
        border-radius: 8px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        background-color: rgba(255, 255, 255, 0.90);
        width: 627px;
        background-image: none;
        background-repeat: repeat;
        background-attachment: scroll;
    }
</style>
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
    <title>Rutinas Cabana</title>
    <link rel="stylesheet" href="styles.css" />
    </head>
<body>
    
        <form id="form1" runat="server">
    
        <div class="auto-style1">
            <h1>Bienvenido a Rutinas MTI</h1>
            <p>
                <asp:TextBox ID="txtname" runat="server" input type="text" placeholder="Nombre y Apellido" Enabled="False" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtname" ErrorMessage="Ingrese su nombre" ForeColor="Red"></asp:RequiredFieldValidator>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:RadioButton ID="rbtInstrumentista" runat="server" AutoPostBack="True" Checked="True" ForeColor="#CC0000" GroupName="RolesDeUsuario" OnCheckedChanged="rbtInstrumentista_CheckedChanged" Text="Instrumentista" />
            </p>
            <p>
                <asp:TextBox ID="txtcodigo" runat="server" input type ="number" placeholder="Codigo de empleado"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtcodigo" ErrorMessage="Complete este campo " ForeColor="Red"></asp:RequiredFieldValidator>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <asp:RadioButton ID="rbtAdministrador" runat="server" AutoPostBack="True" ForeColor="#CC0000" GroupName="RolesDeUsuario" OnCheckedChanged="rbtAdministrador_CheckedChanged" Text="Administrador " />
            </p>
            <p>
                <asp:Button ID="btnentrar" runat="server" button type="submit" Text="INICIAR SESION" OnClick="btnentrar_Click" />
            </p
        </div>

    
    
      
        </form>

    
    
<footer>
        <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
    </footer>  
</body>
</html>
