<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Rutinas.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Generador de Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css"/>
    <link rel="stylesheet" href="styles2.css"/>
</head>
<body>
<form id="form1" runat="server">
   <header>
        <h1>Ingenio La Cabaña</h1>
        <nav>
            <ul>
                <li>
                    <asp:Button ID="registros" runat="server"
                                Text="📋 RUTINAS" OnClick="registros_Click"
                                CssClass="nav-button-link"/>
                </li>
                <li>
                    <asp:Button ID="btnFallos" runat="server"
                                Text="⚠️ FALLOS" OnClick="btnFallos_Click"
                                CssClass="nav-button-link" />
                </li>
                <li>
                    <asp:Button ID="btnDashboard" runat="server"
                                Text="📊 DASHBOARD" OnClick="btnDashboard_Click"
                                CssClass="nav-button-link" />
                </li>
                <li>
                    <asp:Button ID="btnDesmontaje" runat="server"
                                Text="🔧 DESMONTAJE" OnClick="btnDesmontaje_Click"
                                CssClass="nav-button-link" />
                </li>
                <li>
                    <asp:Button ID="btnDashboardDesmontaje" runat="server"
                                Text="📈 AVANCE" OnClick="btnDashboardDesmontaje_Click"
                                CssClass="nav-button-link" />
                </li>
                <li>
                    <asp:Button ID="logout" runat="server"
                                Text="🚪 SALIR" OnClick="logout_Click"
                                CssClass="nav-button-link" />
                </li>
                <li>
                </li>
            </ul>
        </nav>
    </header>

 <section id="panel">
    <div class="bienvenida">
        <div class="bienvenida-info">
            &nbsp;<asp:Label ID="lblnombremenu" runat="server" Text="Username" CssClass="bienvenida-nombre"></asp:Label>
        </div>
        <div id="relojDisplay" class="bienvenida-reloj"></div>
    </div>
    <div class="contenedor-panel">

        <asp:LinkButton ID="btnGenerarRutina" runat="server" OnClick="btnGenerarRutina_Click" CssClass="panel">
            <img src="media/documentoicono.jpg" alt="Generar Rutina"/>
            <h3>Generar Rutina</h3>
            <p>Generar la rutina para el turno actual</p>
        </asp:LinkButton>

        <asp:LinkButton ID="btnImprimirRutina" runat="server" OnClick="btnImprimirRutina_Click" CssClass="panel">
            <img src="media/iconoimpresora.jpg" alt="Imprimir Rutina"/>
            <h3>Imprimir Ultima Rutina</h3>
            <p>Imprime la ultima rutina generada en el turno</p>
        </asp:LinkButton>
    </div>
</section>
</form>
<footer>
        <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
    </footer>  
    <script src="shared.js"></script>
    <script type="text/javascript">
        function actualizarReloj() {
            var ahora = new Date();
            var dias  = ['Domingo','Lunes','Martes','Miércoles','Jueves','Viernes','Sábado'];
            var meses = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic'];
            var h = ahora.getHours().toString().padStart(2,'0');
            var m = ahora.getMinutes().toString().padStart(2,'0');
            var s = ahora.getSeconds().toString().padStart(2,'0');
            var txt = dias[ahora.getDay()] + ' ' + ahora.getDate() + ' ' + meses[ahora.getMonth()]
                    + ' &nbsp;|&nbsp; ' + h + ':' + m + ':' + s;
            document.getElementById('relojDisplay').innerHTML = txt;
        }
        actualizarReloj();
        setInterval(actualizarReloj, 1000);
    </script>
</body>
</html>
