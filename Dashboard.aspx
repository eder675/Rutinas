<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Rutinas.Dashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dashboard de Fallos — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesdashboard.css" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <div class="db-wrapper">

            <!-- ═══════════════════════════════════════════════
                 CABECERA
            ════════════════════════════════════════════════ -->
            <div class="db-header">
                <div class="db-header-left">
                    <asp:Button ID="btnSalir" runat="server" Text="&#8592; Salir"
                        CssClass="db-btn-salir" OnClick="btnSalir_Click" CausesValidation="false" />
                    <div>
                        <h1>Dashboard de Fallos de Instrumentos</h1>
                        <p>Ingenio La Cabaña — Departamento de Tecnologia Industrial</p>
                    </div>
                </div>
            </div>

            <!-- ═══════════════════════════════════════════════
                 MÉTRICAS
            ════════════════════════════════════════════════ -->
            <div class="db-section">
                <p class="db-section-title">Resumen General</p>
                <div class="db-metrics-grid">

                    <div class="db-metric-card">
                        <span class="db-metric-label">Total de Fallos</span>
                        <asp:Label ID="lblTotalFallos" runat="server" Text="0" CssClass="db-metric-value"></asp:Label>
                        <span class="db-metric-sub">registros en la base de datos</span>
                    </div>

                    <div class="db-metric-card db-metric-inducido">
                        <span class="db-metric-label">Fallos Inducidos</span>
                        <asp:Label ID="lblInducidos" runat="server" Text="0" CssClass="db-metric-value"></asp:Label>
                        <span class="db-metric-sub">causados por accion humana</span>
                    </div>

                    <div class="db-metric-card db-metric-natural">
                        <span class="db-metric-label">Fallos Naturales</span>
                        <asp:Label ID="lblNaturales" runat="server" Text="0" CssClass="db-metric-value"></asp:Label>
                        <span class="db-metric-sub">fallas por desgaste u otras causas</span>
                    </div>

                    <div class="db-metric-card db-metric-reemplazar">
                        <span class="db-metric-label">Pendientes de Reemplazo</span>
                        <asp:Label ID="lblReemplazar" runat="server" Text="0" CssClass="db-metric-value"></asp:Label>
                        <span class="db-metric-sub">equipos a reemplazar en mantenimiento</span>
                    </div>

                </div>
            </div>

            <!-- ═══════════════════════════════════════════════
                 FILTRO + BOTÓN EXPORTAR
            ════════════════════════════════════════════════ -->
            <div class="db-section">
                <p class="db-section-title">Registros Detallados</p>

                <div class="db-filter-panel">
                    <label for="<%= ddlFiltroNivel.ClientID %>">Filtrar por Nivel de Daño:</label>
                    <asp:DropDownList ID="ddlFiltroNivel" runat="server"
                        CssClass="db-filter-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlFiltroNivel_SelectedIndexChanged">
                    </asp:DropDownList>

                    <asp:Button ID="btnExportar" runat="server" Text="&#128190; Exportar Excel"
                        CssClass="db-btn-exportar" OnClick="btnExportar_Click" CausesValidation="false" />
                </div>

                <!-- TABLA -->
                <div class="db-table-wrap">
                    <asp:GridView ID="gvFallos" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="db-grid"
                        GridLines="None"
                        EmptyDataText=""
                        OnRowDataBound="gvFallos_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ID"            HeaderText="ID"            ItemStyle-Width="50px" />
                            <asp:BoundField DataField="Instrumentista" HeaderText="Instrumentista" />
                            <asp:BoundField DataField="TAG"           HeaderText="TAG"            ItemStyle-Width="90px" />
                            <asp:BoundField DataField="Instrumento"   HeaderText="Instrumento" />
                            <asp:TemplateField HeaderText="Inducido">
                                <ItemTemplate>
                                    <span class='<%# Convert.ToBoolean(Eval("FalloInducido")) ? "db-badge db-badge-si" : "db-badge db-badge-no" %>'>
                                        <%# Convert.ToBoolean(Eval("FalloInducido")) ? "SI" : "NO" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="NivelDano"     HeaderText="Nivel Daño" />
                            <asp:BoundField DataField="TipoDano"      HeaderText="Tipo Daño" />
                            <asp:BoundField DataField="PosibleCausa"  HeaderText="Posible Causa" />
                            <asp:TemplateField HeaderText="Reemplaza">
                                <ItemTemplate>
                                    <span class='<%# Convert.ToBoolean(Eval("Reemplaza")) ? "db-badge db-badge-si" : "db-badge db-badge-no" %>'>
                                        <%# Convert.ToBoolean(Eval("Reemplaza")) ? "SI" : "NO" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Interviene">
                                <ItemTemplate>
                                    <span class='<%# Convert.ToBoolean(Eval("Interviene")) ? "db-badge db-badge-si" : "db-badge db-badge-no" %>'>
                                        <%# Convert.ToBoolean(Eval("Interviene")) ? "SI" : "NO" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="db-empty-msg">No se encontraron registros para el filtro seleccionado.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- ═══════════════════════════════════════════════
                 GRÁFICAS
            ════════════════════════════════════════════════ -->
            <div class="db-section">
                <p class="db-section-title">Analisis Visual</p>
                <div class="db-charts-grid">

                    <!-- Dona: fallos por TipoDano -->
                    <div class="db-chart-card">
                        <p class="db-chart-title">Fallos por Tipo de Daño</p>
                        <div class="db-chart-canvas-wrap">
                            <canvas id="chartTipo"></canvas>
                        </div>
                    </div>

                    <!-- Barras: fallos por PosibleCausa -->
                    <div class="db-chart-card">
                        <p class="db-chart-title">Fallos por Posible Causa</p>
                        <div class="db-chart-canvas-wrap">
                            <canvas id="chartCausa"></canvas>
                        </div>
                    </div>

                </div>
            </div>

        </div><!-- /db-wrapper -->
    </form>

    <!-- Chart.js local -->
    <script src="Scripts/chart.umd.min.js"></script>

    <script type="text/javascript">
    (function () {
        /* ── Paleta de colores ── */
        var PALETTE = [
            '#388E3C','#2E7D32','#62D151','#1565C0','#0288D1',
            '#FBC02D','#E53935','#6D4C41','#00838F','#AD1457',
            '#558B2F','#4527A0','#FF6F00','#00695C','#D84315',
            '#37474F','#6A1B9A'
        ];

        function getColors(n) {
            var arr = [];
            for (var i = 0; i < n; i++) arr.push(PALETTE[i % PALETTE.length]);
            return arr;
        }

        /* ── Datos inyectados desde el servidor ── */
        var tipoDanoLabels  = typeof _tipoDanoLabels  !== 'undefined' ? _tipoDanoLabels  : [];
        var tipoDanoValues  = typeof _tipoDanoValues  !== 'undefined' ? _tipoDanoValues  : [];
        var causaLabels     = typeof _causaLabels     !== 'undefined' ? _causaLabels     : [];
        var causaValues     = typeof _causaValues     !== 'undefined' ? _causaValues     : [];

        /* ── Gráfica 1: DONA — Fallos por TipoDano ── */
        var ctxTipo = document.getElementById('chartTipo');
        if (ctxTipo && tipoDanoLabels.length > 0) {
            new Chart(ctxTipo, {
                type: 'doughnut',
                data: {
                    labels: tipoDanoLabels,
                    datasets: [{
                        data: tipoDanoValues,
                        backgroundColor: getColors(tipoDanoLabels.length),
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                font: { size: 11 },
                                padding: 14,
                                boxWidth: 14
                            }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                    var pct   = total > 0 ? Math.round(ctx.parsed / total * 100) : 0;
                                    return ctx.label + ': ' + ctx.parsed + ' (' + pct + '%)';
                                }
                            }
                        }
                    }
                }
            });
        }

        /* ── Gráfica 2: BARRAS HORIZONTAL — Fallos por PosibleCausa ── */
        var ctxCausa = document.getElementById('chartCausa');
        if (ctxCausa && causaLabels.length > 0) {
            new Chart(ctxCausa, {
                type: 'bar',
                data: {
                    labels: causaLabels,
                    datasets: [{
                        label: 'Cantidad de fallos',
                        data: causaValues,
                        backgroundColor: getColors(causaLabels.length),
                        borderRadius: 4,
                        borderWidth: 0
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    return ' ' + ctx.parsed.x + ' fallo(s)';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0,
                                font: { size: 11 }
                            },
                            grid: { color: 'rgba(0,0,0,0.06)' }
                        },
                        y: {
                            ticks: {
                                font: { size: 10 },
                                callback: function (val, idx) {
                                    var lbl = causaLabels[idx] || '';
                                    return lbl.length > 30 ? lbl.substring(0, 28) + '...' : lbl;
                                }
                            },
                            grid: { display: false }
                        }
                    }
                }
            });
        }
    })();
    </script>
</body>
</html>
