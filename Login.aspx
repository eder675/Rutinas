<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Rutinas.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Rutinas Cabana</title>
    <link rel="stylesheet" href="styles.css" />
    <style type="text/css">
        .auto-style1 {
            width: 588px;
            height: 85px;
        }
    </style>
</head>
<body>
    
        <form id="form1" runat="server">
    
        <div class="CONTENEDOR">
            <h1>Bienvenido a Rutinas MTI</h1>
            <p>
                <asp:TextBox ID="txtname" runat="server" input type="text" placeholder="Nombre y Apellido" required></asp:TextBox>
            </p>
            <p>
                <asp:TextBox ID="txtcodigo" runat="server" input type ="number" placeholder="Codigo de empleado" required></asp:TextBox>
            </p>
            <p>
                <asp:Button ID="btnentrar" runat="server" button type="submit" Text="INICIAR SESION" OnClick="btnentrar_Click" />
            </p
        </div>

    
    
      
        </form>

    
    
      
</body>
</html>
