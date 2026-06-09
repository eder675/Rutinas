<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardMantenimiento.aspx.cs"
    Inherits="Rutinas.DashboardMantenimiento" EnableEventValidation="false" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dashboard de Mantenimiento — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesdashboardmantenimiento.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="ddm-wrapper">

            <!-- ════════════════════════════════════════════════
                 ENCABEZADO
            ═════════════════════════════════════════════════ -->
            <div class="ddm-header">
                <asp:Button ID="btnSalir" runat="server" Text="&#8592; Salir"
                    CssClass="ddm-btn-salir" CausesValidation="false"
                    OnClick="btnSalir_Click" />
                <div class="ddm-header-center">
                    <h1>DASHBOARD DE MANTENIMIENTO</h1>
                    <p>Ingenio La Cabaña — Avance de viñetas por empleado</p>
                </div>
            </div>

            <!-- ════════════════════════════════════════════════
                 MÉTRICAS GLOBALES
            ═════════════════════════════════════════════════ -->
            <div class="ddm-metrics-grid">

                <div class="ddm-metric-card">
                    <span class="ddm-metric-label">Equipos Activos</span>
                    <asp:Label ID="lblTotalEquipos" runat="server" Text="0" CssClass="ddm-metric-value"></asp:Label>
                    <span class="ddm-metric-sub">no hibernados en Vinetas</span>
                </div>

                <div class="ddm-metric-card ddm-azul">
                    <span class="ddm-metric-label">Viñetas 2026</span>
                    <asp:Label ID="lblVinetas2026" runat="server" Text="0" CssClass="ddm-metric-value"></asp:Label>
                    <span class="ddm-metric-sub">equipos únicos con mantenimiento</span>
                </div>

                <div class="ddm-metric-card ddm-verde">
                    <span class="ddm-metric-label">% Avance Global</span>
                    <asp:Label ID="lblPorcentajeGlobal" runat="server" Text="0.0%" CssClass="ddm-metric-value"></asp:Label>
                    <asp:Literal ID="litBarraGlobal" runat="server"></asp:Literal>
                </div>

                <div class="ddm-metric-card ddm-naranja">
                    <span class="ddm-metric-label">Viñetas Hoy</span>
                    <asp:Label ID="lblHoy" runat="server" Text="0" CssClass="ddm-metric-value"></asp:Label>
                    <span class="ddm-metric-sub">todos los empleados</span>
                </div>

                <div class="ddm-metric-card ddm-morado">
                    <span class="ddm-metric-label">Viñetas Esta Semana</span>
                    <asp:Label ID="lblSemana" runat="server" Text="0" CssClass="ddm-metric-value"></asp:Label>
                    <span class="ddm-metric-sub">lunes a hoy</span>
                </div>

            </div>

            <!-- ════════════════════════════════════════════════
                 FILTROS Y ORDENAMIENTO
            ═════════════════════════════════════════════════ -->
            <div class="ddm-filter-panel">
                <label>Empleado:</label>
                <asp:DropDownList ID="ddlFiltroEmpleado" runat="server"
                    CssClass="ddm-filter-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlFiltroEmpleado_SelectedIndexChanged">
                </asp:DropDownList>

                <div class="ddm-filter-sep"></div>

                <label>Ordenar por:</label>
                <asp:Button ID="btnSortMasSemana" runat="server" Text="&#8593; Más semana"
                    CssClass="ddm-sort-btn" CausesValidation="false"
                    OnClick="btnSortMasSemana_Click" />
                <asp:Button ID="btnSortMenosSemana" runat="server" Text="&#8595; Menos semana"
                    CssClass="ddm-sort-btn" CausesValidation="false"
                    OnClick="btnSortMenosSemana_Click" />
                <asp:Button ID="btnSortMasHoy" runat="server" Text="&#8593; Más hoy"
                    CssClass="ddm-sort-btn" CausesValidation="false"
                    OnClick="btnSortMasHoy_Click" />
                <asp:Button ID="btnSortMenosHoy" runat="server" Text="&#8595; Menos hoy"
                    CssClass="ddm-sort-btn" CausesValidation="false"
                    OnClick="btnSortMenosHoy_Click" />
            </div>

            <!-- ════════════════════════════════════════════════
                 RANKING DE EMPLEADOS
            ═════════════════════════════════════════════════ -->
            <div class="ddm-section">
                <h2>Rendimiento por empleado</h2>
                <div class="ddm-table-wrap">
                    <asp:GridView ID="gvRanking" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddm-grid"
                        GridLines="None"
                        OnRowCommand="gvRanking_RowCommand"
                        OnRowDataBound="gvRanking_RowDataBound">
                        <Columns>
                            <asp:TemplateField HeaderText="Empleado">
                                <ItemTemplate>
                                    <span class="ddm-cell-nombre"><%# Eval("Nombre") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Hoy" ItemStyle-CssClass="ddm-cell-num">
                                <ItemTemplate>
                                    <asp:Label ID="lblHoyCell" runat="server" Text='<%# Eval("Hoy") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Semana" ItemStyle-CssClass="ddm-cell-num">
                                <ItemTemplate>
                                    <asp:Label ID="lblSemanaCell" runat="server" Text='<%# Eval("Semana") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Mes" ItemStyle-CssClass="ddm-cell-num">
                                <ItemTemplate>
                                    <asp:Label ID="lblMesCell" runat="server" Text='<%# Eval("Mes") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Año 2026" ItemStyle-CssClass="ddm-cell-num">
                                <ItemTemplate>
                                    <asp:Label ID="lblAnioCell" runat="server" Text='<%# Eval("Anio") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Pendientes" ItemStyle-CssClass="ddm-cell-num">
                                <ItemTemplate>
                                    <asp:Label ID="lblPendientesCell" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <ItemTemplate>
                                    <asp:Button ID="btnVerDetalle" runat="server"
                                        Text="Ver detalle"
                                        CommandName="VerDetalle"
                                        CommandArgument='<%# Eval("Nombre") %>'
                                        CssClass="ddm-btn-detalle"
                                        CausesValidation="false" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <p style="padding:16px; color:var(--c-text-muted); font-style:italic;">
                                No hay datos de empleados registrados.
                            </p>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- ════════════════════════════════════════════════
                 PANEL DE DETALLE DE EMPLEADO
            ═════════════════════════════════════════════════ -->
            <asp:Panel ID="pnlDetalle" runat="server" Visible="false" CssClass="ddm-detalle-panel">

                <div class="ddm-detalle-header">
                    <h2>Detalle: <asp:Label ID="lblDetalleNombre" runat="server" Text=""></asp:Label></h2>
                    <asp:Button ID="btnCerrarDetalle" runat="server" Text="&#10005; Cerrar"
                        CssClass="ddm-btn-cerrar" CausesValidation="false"
                        OnClick="btnCerrarDetalle_Click" />
                </div>

                <!-- Viñetas recientes -->
                <p class="ddm-detalle-sub">Viñetas recientes (últimas 50)</p>
                <div class="ddm-table-wrap">
                    <asp:GridView ID="gvVinetas" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="ddm-grid"
                        GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="TAG"          HeaderText="TAG" />
                            <asp:BoundField DataField="Descripcion"  HeaderText="Descripción" />
                            <asp:BoundField DataField="Fecha"        HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="Mantenimiento" HeaderText="Mantenimiento" />
                        </Columns>
                        <EmptyDataTemplate>
                            <p class="ddm-no-data" style="padding:12px;">Sin viñetas registradas para este empleado en 2026.</p>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <!-- Equipos pendientes -->
                <p class="ddm-detalle-sub">Equipos pendientes (sin viñeta en 2026)</p>
                <asp:Literal ID="litPendientes" runat="server"></asp:Literal>

            </asp:Panel>

        </div>
    </form>
</body>
</html>
