<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Generadorrutinas.aspx.cs" Inherits="Rutinas.Generadorrutinas" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Creando Formulario</title>
    <link rel="stylesheet" href="styles-shared.css"/>
    <link rel="stylesheet" href="estilosgenerador.css"/>
    </head>
<body>
    <form id="form1" runat="server">
        
        <div class="encabezado-rutina">
            &nbsp;<div class="encabezado-titulos">
                <asp:Label ID="Label1" runat="server" Text="INGENIO LA CABAÑA"></asp:Label><br />
                <asp:Label ID="Label2" runat="server" Text="DEPARTAMENTO DE TECNOLOGIA INDUSTRIAL"></asp:Label><br />
                <asp:Label ID="Label3" runat="server" Text="LISTA DE VERIFICACION DE RUTINA DE INSTRUMENTISTA"></asp:Label>
            </div>
            <div class="encabezado-info">
                <div class="fila-firma">
                    <span><asp:Label ID="Label6" runat="server" Text="Instrumentista:"></asp:Label>&nbsp;<asp:Label ID="lblname" runat="server" Text="Nombre" Font-Bold="True" Font-Overline="False" Font-Strikeout="False" Font-Underline="False"></asp:Label></span>
                    <span>Firma: __________________ Revisó y aprobó:___________________</span>
                    <asp:Label ID="Label7" runat="server" Text="Dia Zafra: "></asp:Label>
                    <asp:Label ID="lblzafra" runat="server" Text="Label"></asp:Label>
                </div>
                <div class="fila-centro">
                    <asp:Label ID="Label4" runat="server" Text="Fecha:"></asp:Label>&nbsp;<asp:Label ID="lblfecha" runat="server" Text="fecha actual" Font-Bold="True"></asp:Label>
                </div>
                <div class="fila-centro">
                    <asp:Label ID="Label5" runat="server" Text="Turno"></asp:Label>:&nbsp;<asp:Label ID="lblturno" runat="server" Text="Turno Actual" Font-Bold="True"></asp:Label>
                </div>
            </div>
        </div>

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
                        <div class="celda-equipo">
                            <asp:Label ID="lblEquipo" runat="server" Text='<%# Eval("NombreInstrumento") %>' />
                        </div>
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
                        <div class="celda-equipo">
                            <asp:Label ID="lblEquipo0" runat="server" Text='<%# Eval("NombreInstrumento") %>' />
                        </div>
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

    <script type="text/javascript">
        // Fusiona las celdas de ÁREA consecutivas con el mismo valor,
        // aplica escritura vertical y oculta los duplicados.
        function mergeAreaCells(tableSelector) {
            var table = document.querySelector(tableSelector);
            if (!table) return;
            var tbody = table.querySelector('tbody');
            if (!tbody) return;

            var rows = tbody.querySelectorAll('tr');
            var prevCell = null;
            var prevText = '';
            var span = 1;

            for (var i = 0; i < rows.length; i++) {
                var cell = rows[i].cells[0];
                if (!cell) continue;
                var text = cell.textContent.trim();

                if (prevCell !== null && text === prevText) {
                    span++;
                    prevCell.rowSpan = span;
                    cell.style.display = 'none';
                } else {
                    prevCell = cell;
                    prevText = text;
                    span = 1;
                    cell.classList.add('area-vertical');
                }
            }
        }

        window.addEventListener('load', function () {
            mergeAreaCells('.tabla-instrumentos');
            mergeAreaCells('.tabla-comprobaciones');
        });

        // AUTO-AJUSTE AL IMPRIMIR
        // Carta portrait con márgenes 0.4in arriba/abajo: área imprimible vertical ≈ 10.2in = 979px a 96dpi
        var ALTO_CARTA_PX = 10.2 * 96;

        window.addEventListener('beforeprint', function () {
            var carta = document.querySelector('.formato-carta');
            if (!carta) return;
            // Resetear escala previa para medir la altura real
            carta.style.transform = '';
            carta.style.width = '';
            var altoContenido = carta.scrollHeight;
            if (altoContenido > ALTO_CARTA_PX) {
                var escala = ALTO_CARTA_PX / altoContenido;
                // Escalar desde el centro superior para no desbordar el margen derecho
                carta.style.transformOrigin = 'top center';
                carta.style.transform = 'scale(' + escala + ')';
                // NO se expande el ancho: el contenido escalado cabe dentro del área imprimible
            }
        });

        window.addEventListener('afterprint', function () {
            var carta = document.querySelector('.formato-carta');
            if (carta) {
                carta.style.transform = '';
                carta.style.width = '';
            }
        });
    </script>
</body>
</html>
