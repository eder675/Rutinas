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
                 DETALLE DE EQUIPOS
            ═════════════════════════════════════════════════ -->
            <!-- DESMONTADOS -->
            <div class="ddb-section">
                <h2>Equipos Desmontados <asp:Label ID="lblContDesmontados" runat="server" CssClass="ddb-seccion-conteo"/></h2>
                <div class="ddb-table-wrap">
                    <asp:GridView ID="gvDesmontados" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddb-grid"
                        GridLines="None"
                        OnRowDataBound="gvDesmontados_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="TAG"         HeaderText="TAG"         ItemStyle-Width="90px" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                            <asp:BoundField DataField="Area"        HeaderText="Área"        ItemStyle-Width="140px" />
                            <asp:TemplateField HeaderText="Fecha Declaración" ItemStyle-Width="140px">
                                <ItemTemplate>
                                    <%# Eval("FechaDeclaracion") != null
                                        ? Convert.ToDateTime(Eval("FechaDeclaracion")).ToString("dd/MM/yyyy HH:mm")
                                        : "—" %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="NombreEmpleado" HeaderText="Declarado por" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="ddb-empty-msg">No hay equipos desmontados para el área seleccionada.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- PENDIENTES -->
            <div class="ddb-section">
                <h2>Equipos Pendientes <asp:Label ID="lblContPendientes" runat="server" CssClass="ddb-seccion-conteo"/></h2>
                <div class="ddb-table-wrap">
                    <asp:GridView ID="gvPendientes" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddb-grid"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="TAG"         HeaderText="TAG"         ItemStyle-Width="90px" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                            <asp:BoundField DataField="Area"        HeaderText="Área"        ItemStyle-Width="140px" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="ddb-empty-msg">No hay equipos pendientes para el área seleccionada.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <div id="divMostrarMas" style="text-align:center; margin-top:12px; display:none;">
                    <button type="button" class="ddb-btn-mostrar" onclick="mostrarTodosPendientes()">
                        Mostrar todos los pendientes ▼
                    </button>
                </div>
            </div>

    <script type="text/javascript">
        var FILAS_INICIALES = 10;

        function aplicarLimitePendientes() {
            var $filas = $('#<%= gvPendientes.ClientID %> tr:not(:first-child)');
            if ($filas.length > FILAS_INICIALES) {
                $filas.slice(FILAS_INICIALES).hide();
                $('#divMostrarMas').show();
            }
        }

        function mostrarTodosPendientes() {
            $('#<%= gvPendientes.ClientID %> tr').show();
            $('#divMostrarMas').hide();
        }

        $(document).ready(function () {
            aplicarLimitePendientes();
        });
    </script>

        </div><!-- /ddb-wrapper -->

    </form>
</body>
</html>
