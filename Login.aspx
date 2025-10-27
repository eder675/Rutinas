<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Rutinas.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Rutinas Cabana</title>
    <link rel="stylesheet" href="styles.css" />
    </head>
<body>
    
        <form id="form1" runat="server">
    
        <div class="CONTENEDOR">
            <h1>Bienvenido a Rutinas MTI</h1>
            <p>
                <asp:TextBox ID="txtname" runat="server" input type="text" placeholder="Nombre y Apellido" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtname" ErrorMessage="Ingrese su nombre" ForeColor="Red"></asp:RequiredFieldValidator>
            </p>
            <p>
                <asp:TextBox ID="txtcodigo" runat="server" input type ="number" placeholder="Codigo de empleado"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtcodigo" ErrorMessage="Complete este campo " ForeColor="Red"></asp:RequiredFieldValidator>
            </p>
            <p>
                <asp:Button ID="btnentrar" runat="server" button type="submit" Text="INICIAR SESION" OnClick="btnentrar_Click" />
            </p
        </div>

    
    
      
        </form>

    
    
      
</body>
</html>
