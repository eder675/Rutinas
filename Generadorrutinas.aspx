<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Generadorrutinas.aspx.cs" Inherits="Rutinas.Generadorrutinas" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Creando Formulario</title>
    <link rel="stylesheet" href="styles-shared.css"/>
    <link rel="stylesheet" href="estilosgenerador.css"/>
    <style type="text/css">
        @media screen {
            .aux-panel { display:flex; justify-content:center; align-items:center; min-height:80vh; }
            .aux-card  { background:rgba(255,255,255,0.95); border-radius:12px; box-shadow:0 4px 24px rgba(0,0,0,0.13); border-top:4px solid var(--c-primary); padding:32px 40px; min-width:360px; max-width:480px; }
            .aux-titulo { color:var(--c-primary-dark); font-size:1.15em; font-weight:700; text-align:center; margin:0 0 22px; }
            .aux-opcion-label { display:flex; align-items:center; gap:10px; font-size:1em; font-weight:600; cursor:pointer; margin-bottom:8px; }
            .aux-opcion-label input[type="radio"] { width:17px; height:17px; accent-color:var(--c-primary); cursor:pointer; }
            .aux-sub  { margin-left:28px; margin-bottom:12px; }
            .aux-ddl  { width:100%; padding:7px 10px; border:1.5px solid var(--c-border); border-radius:6px; font-size:0.95em; margin-top:4px; }
            .aux-cantidad-lbl { font-size:0.9em; color:#555; }
            .aux-cant { width:64px; padding:5px 8px; border:1.5px solid var(--c-border); border-radius:6px; text-align:center; font-size:1em; margin-top:4px; }
            .aux-btn-generar { display:block; width:100%; margin-top:24px; padding:13px; background:linear-gradient(135deg,var(--c-primary) 0%,var(--c-primary-dark) 100%); color:#fff; border:none; border-radius:8px; font-size:1.05em; font-weight:700; letter-spacing:0.04em; cursor:pointer; transition:transform 0.15s,filter 0.15s; }
            .aux-btn-generar:hover { transform:translateY(-2px); filter:brightness(1.08); }
            .aux-msg  { display:block; margin-top:8px; font-size:0.9em; color:#c00; text-align:center; }
        }
        @media print { .aux-panel { display:none !important; } }
    </style>
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
                    <span><asp:Label ID="Label6" runat="server" Text="Instrumentista:"></asp:Label>&nbsp;<asp:Label ID="lblname" runat="server" Text="Nombre" Font-Bold="True"></asp:Label></span>
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

    <%-- PANEL AUXILIAR: elección de tipo de rutina --%>
    <asp:Panel ID="pnlOpcionAuxiliar" runat="server" Visible="false" CssClass="aux-panel">
        <div class="aux-card">
            <h3 class="aux-titulo">¿Qué tipo de rutina va a realizar?</h3>
            <label class="aux-opcion-label">
                <asp:RadioButton ID="rdoRelevar" runat="server" GroupName="grpTipoAux" CssClass="aux-rdo-relevar" />
                Relevar a un instrumentista
            </label>
            <div id="divRelevar" class="aux-sub" style="display:none;">
                <asp:DropDownList ID="ddlInstrumentistas" runat="server" CssClass="aux-ddl" />
            </div>
            <label class="aux-opcion-label" style="margin-top:10px;">
                <asp:RadioButton ID="rdoTachos" runat="server" GroupName="grpTipoAux" CssClass="aux-rdo-tachos" />
                Rutina de TACHOS
            </label>
            <div id="divTachos" class="aux-sub" style="display:none;">
                <label class="aux-cantidad-lbl">Cantidad de equipos Brix:</label><br />
                <asp:TextBox ID="txtCantTachos" runat="server" Text="4" CssClass="aux-cant" MaxLength="2" />
            </div>
            <asp:Button ID="btnGenerarAuxiliar" runat="server" Text="GENERAR RUTINA"
                CssClass="aux-btn-generar" OnClick="btnGenerarAuxiliar_Click" />
            <asp:Label ID="lblAuxMsg" runat="server" Text="" CssClass="aux-msg" />
        </div>
    </asp:Panel>

    <%-- TABLA 1: INSTRUMENTOS --%>
    <asp:Panel ID="pnlInstrumentos" runat="server">
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
                        <td class="col-lectura"><div class="linea-manual-pequena"></div></td>
                        <td class="col-hora"><div class="linea-manual-pequena"></div></td>
                        <td class="col-observaciones"><div class="linea-manual-larga"></div></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></FooterTemplate>
            </asp:Repeater>
        </table>
    </asp:Panel>

    <%-- TABLA 2: COMPROBACIONES OBLIGATORIAS --%>
    <asp:Panel ID="pnlObligatorios" runat="server">
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
                        <td class="col-lecturaeq"><div class="linea-manual-pequena"></div></td>
                        <td class="col-lecturalab"><div class="linea-manual-pequena"></div></td>
                        <td class="col-hora"><div class="linea-manual-pequena"></div></td>
                        <td class="col-evidencia"><div class="linea-manual-pequena"></div></td>
                        <td class="col-observaciones"><div class="linea-manual-larga"></div></td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate></tbody></FooterTemplate>
            </asp:Repeater>
        </table>
    </asp:Panel>

    <%-- TABLA 3: DESMONTAJE --%>
    <asp:Panel ID="pnlDesmontaje" runat="server" Visible="false">
        <div class="seccion-desmontaje">
            <h3>DETALLE DE DESMONTAJE DE EQUIPOS</h3>
            <table class="tabla-desmontaje" cellspacing="0" cellpadding="0">
                <asp:Repeater ID="rptDesmontaje" runat="server">
                    <HeaderTemplate>
                        <thead>
                            <tr>
                                <th class="col-area">ÁREA</th>
                                <th class="col-equipo">EQUIPO</th>
                                <th class="col-hora-d">HORA INICIO</th>
                                <th class="col-hora-d">HORA FINALIZACIÓN</th>
                                <th class="col-obs-des">OBSERVACIONES</th>
                            </tr>
                        </thead>
                        <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="col-area">
                                <asp:Label ID="lblAreaD" runat="server" Text='<%# Eval("NombreArea") %>' />
                            </td>
                            <td class="col-equipo">
                                <div class="celda-equipo">
                                    <asp:Label ID="lblEquipoD" runat="server" Text='<%# Eval("NombreInstrumento") %>' />
                                </div>
                            </td>
                            <td class="col-hora-d"><div class="linea-manual-pequena"></div></td>
                            <td class="col-hora-d"><div class="linea-manual-pequena"></div></td>
                            <td class="col-obs-des"><div class="linea-manual-larga"></div></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate></tbody></FooterTemplate>
                </asp:Repeater>
            </table>
        </div>
    </asp:Panel>

        <br />

    </form>

    <script type="text/javascript">
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
            mergeAreaCells('.tabla-desmontaje');
        });

        var ALTO_CARTA_PX = 10.2 * 96;

        window.addEventListener('beforeprint', function () {
            var carta = document.querySelector('.formato-carta');
            if (!carta) return;
            carta.style.transform = '';
            carta.style.width = '';
            var altoContenido = carta.scrollHeight;
            if (altoContenido > ALTO_CARTA_PX) {
                var escala = ALTO_CARTA_PX / altoContenido;
                carta.style.transformOrigin = 'top center';
                carta.style.transform = 'scale(' + escala + ')';
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
