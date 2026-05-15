<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardDesmontaje.aspx.cs"
    Inherits="Rutinas.DashboardDesmontaje" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dashboard de Desmontaje — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesdashboarddesmontaje.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="Scripts/chart.umd.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="ddb-wrapper">

            <!-- ════════════════════════════════════════════════
                 ENCABEZADO
            ═════════════════════════════════════════════════ -->
            <div class="ddb-header">
                <asp:Button ID="btnSalir" runat="server" Text="&#8592; Salir"
                    CssClass="ddb-btn-salir" CausesValidation="false"
                    OnClick="btnSalir_Click" />

                <div class="ddb-header-center">
                    <h1>DASHBOARD DE DESMONTAJE</h1>
                    <p>Ingenio La Cabaña — Avance de desmontaje de instrumentos</p>
                </div>
            </div>

            <!-- ════════════════════════════════════════════════
                 MÉTRICAS GLOBALES
            ═════════════════════════════════════════════════ -->
            <div class="ddb-metrics-grid">

                <div class="ddb-metric-card">
                    <span class="ddb-metric-label">Total Equipos</span>
                    <asp:Label ID="lblTotal" runat="server" Text="0" CssClass="ddb-metric-value"></asp:Label>
                    <span class="ddb-metric-sub">registrados en Vinetas</span>
                </div>

                <div class="ddb-metric-card ddb-verde">
                    <span class="ddb-metric-label">Desmontados</span>
                    <asp:Label ID="lblDesmontados" runat="server" Text="0" CssClass="ddb-metric-value"></asp:Label>
                    <asp:Label ID="lblDeltaDia" runat="server" Text="" CssClass="ddb-metric-delta" Visible="false"></asp:Label>
                    <span class="ddb-metric-sub">declarados como desmontados</span>
                </div>

                <div class="ddb-metric-card ddb-rojo">
                    <span class="ddb-metric-label">Pendientes</span>
                    <asp:Label ID="lblPendientes" runat="server" Text="0" CssClass="ddb-metric-value"></asp:Label>
                    <span class="ddb-metric-sub">faltan por desmontar</span>
                </div>

                <div class="ddb-metric-card ddb-azul">
                    <span class="ddb-metric-label">% Avance Global</span>
                    <asp:Label ID="lblPorcentaje" runat="server" Text="0.0%" CssClass="ddb-metric-value"></asp:Label>
                    <asp:Label ID="lblDeltaPct" runat="server" Text="" CssClass="ddb-metric-delta" Visible="false"></asp:Label>
                    <span class="ddb-metric-sub">sobre el total de equipos</span>
                </div>

            </div>

            <!-- ════════════════════════════════════════════════
                 FILTRO + EXPORTAR
            ═════════════════════════════════════════════════ -->
            <div class="ddb-filter-panel">
                <label for="<%= ddlAreaFiltro.ClientID %>">Filtrar por área:</label>
                <asp:DropDownList ID="ddlAreaFiltro" runat="server"
                    CssClass="ddb-filter-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlAreaFiltro_SelectedIndexChanged">
                </asp:DropDownList>

                <asp:Button ID="btnExportar" runat="server" Text="&#128190; Exportar Excel"
                    CssClass="ddb-btn-exportar" OnClick="btnExportar_Click"
                    CausesValidation="false" />
            </div>

            <!-- ════════════════════════════════════════════════
                 AVANCE POR ÁREA
            ═════════════════════════════════════════════════ -->
            <div class="ddb-section">
                <h2>Avance por área</h2>
                <div class="ddb-table-wrap">
                    <asp:GridView ID="gvAvanceAreas" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddb-grid"
                        GridLines="None"
                        OnRowDataBound="gvAvanceAreas_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="" ItemStyle-Width="28px">
                                <ItemTemplate>
                                    <span class="ddb-expand-icon">▶</span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Area"        HeaderText="Área" />
                            <asp:BoundField DataField="Total"       HeaderText="Total"       ItemStyle-Width="70px" />
                            <asp:BoundField DataField="Desmontados" HeaderText="Desmontados" ItemStyle-Width="100px" />
                            <asp:BoundField DataField="Pendientes"  HeaderText="Pendientes"  ItemStyle-Width="90px" />
                            <asp:TemplateField HeaderText="% Avance" ItemStyle-Width="180px">
                                <ItemTemplate>
                                    <div class="ddb-progress-wrap">
                                        <div class="ddb-progress-bar-bg">
                                            <div class="ddb-progress-bar-fill"
                                                 style='width:<%# string.Format("{0:0.0}", Eval("AvancePct")) %>%;'></div>
                                        </div>
                                        <span class="ddb-progress-label"><%# string.Format("{0:0.0}%", Eval("AvancePct")) %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="ddb-empty-msg">No hay datos de áreas disponibles.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- ════════════════════════════════════════════════
                 ÚLTIMOS 30 INSTRUMENTOS DECLARADOS
            ═════════════════════════════════════════════════ -->
            <div class="ddb-section">
                <h2>Últimos 30 instrumentos declarados</h2>
                <div class="ddb-table-wrap">
                    <asp:GridView ID="gvUltimos" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddb-grid"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="TAG"              HeaderText="TAG"            ItemStyle-Width="90px" />
                            <asp:BoundField DataField="Descripcion"      HeaderText="Descripción" />
                            <asp:BoundField DataField="Area"             HeaderText="Área"           ItemStyle-Width="160px" />
                            <asp:BoundField DataField="FechaDeclaracion" HeaderText="Fecha"          ItemStyle-Width="130px" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                            <asp:BoundField DataField="NombreEmpleado"   HeaderText="Declarado por"  ItemStyle-Width="160px" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="ddb-empty-msg">No hay declaraciones registradas aún.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

    <script type="text/javascript">

        function escHtml(str) { return $('<span>').text(str || '').html(); }

        $(document).on('click', '.ddb-area-row', function () {
            var $row   = $(this);
            var area   = $row.attr('data-area');
            var $detalle = $row.next('.ddb-detail-row');

            // Si ya existe la fila de detalle, solo toggle
            if ($detalle.length) {
                $detalle.toggleClass('ddb-detail-hidden');
                $row.toggleClass('ddb-area-row-open');
                return;
            }

            $row.addClass('ddb-area-row-loading');

            $.getJSON('ObtenerEquiposPorArea.ashx', { area: area })
                .done(function (data) {
                    var cols = $row.find('td').length;

                    var html = '<tr class="ddb-detail-row"><td colspan="' + cols + '">';
                    html += '<div class="ddb-detail-wrap">';

                    // Caja desmontados
                    html += '<div class="ddb-detail-box ddb-detail-desm">';
                    html += '<div class="ddb-detail-box-titulo">&#10003; Desmontados (' + data.desmontados.length + ')</div>';
                    html += '<div class="ddb-detail-lista">';
                    if (data.desmontados.length) {
                        data.desmontados.forEach(function (eq) {
                            html += '<div class="ddb-detail-item">';
                            html += '<span class="ddb-di-tag">'  + escHtml(eq.tag)         + '</span>';
                            html += '<span class="ddb-di-desc">' + escHtml(eq.descripcion) + '</span>';
                            if (eq.fecha) html += '<span class="ddb-di-fecha">' + escHtml(eq.fecha) + ' — ' + escHtml(eq.nombre) + '</span>';
                            html += '</div>';
                        });
                    } else {
                        html += '<div class="ddb-detail-empty">Ninguno declarado aún.</div>';
                    }
                    html += '</div></div>';

                    // Caja pendientes
                    html += '<div class="ddb-detail-box ddb-detail-pend">';
                    html += '<div class="ddb-detail-box-titulo">&#9203; Pendientes (' + data.pendientes.length + ')</div>';
                    html += '<div class="ddb-detail-lista">';
                    if (data.pendientes.length) {
                        data.pendientes.forEach(function (eq) {
                            html += '<div class="ddb-detail-item">';
                            html += '<span class="ddb-di-tag">'  + escHtml(eq.tag)         + '</span>';
                            html += '<span class="ddb-di-desc">' + escHtml(eq.descripcion) + '</span>';
                            html += '</div>';
                        });
                    } else {
                        html += '<div class="ddb-detail-empty">&#127881; ¡Área completamente desmontada!</div>';
                    }
                    html += '</div></div>';

                    html += '</div></td></tr>';

                    $row.after(html);
                    $row.removeClass('ddb-area-row-loading').addClass('ddb-area-row-open');
                })
                .fail(function () {
                    $row.removeClass('ddb-area-row-loading');
                });
        });
    </script>

        </div><!-- /ddb-wrapper -->

    </form>
    <script src="shared.js?v=2"></script>
</body>
</html>

