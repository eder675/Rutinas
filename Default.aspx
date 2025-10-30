<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Rutinas.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Generador de Rutinas</title>
    <link rel="stylesheet" href="styles2.css">
</head>
<body>
   <header>
        <h1>Ingenio La Cabaña</h1>
        <nav>
            <ul>
                <li><a href="Registros.aspx">Registro de rutinas</a></li> 
                <li><a href="LoginDeveloper.aspx">Login como desarrollador</a></li> 
                <li><a href="AcercaDe.aspx">Acerca de.</a></li>
            </ul>
        </nav>
    </header>

 <section id="panel">
        <h2>Seleccione Una Opcion</h2>
        <div class="contenedor-panel">
            <div class="panel">
                <img src="media/documentoicono.jpg" alt="Generar Rutina">
                <h3>Generar Rutina</h3>
                <p>Generar la rutina para el turno actual</p>
            </div>
            <div class="panel">
                <img src="media/iconoimpresora.jpg" alt="Imprimir Rutina">
                <h3>Imprimir Ultima Rutina</h3>
                <p>Imprime la ultima rutina generada en el turno</p>
            </div>
        </div>
    </section>

<footer>
        <p>&copy; 2025 Departamento de Metrologia y Tecnologia Industrial - Todos los derechos reservados.</p>
    </footer>  
</body>
</html>
