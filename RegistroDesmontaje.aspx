<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RegistroDesmontaje.aspx.cs" Inherits="Rutinas.RegistroDesmontaje" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Registro de Desmontaje</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesregistrodesmontaje.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="rdm-wrapper">
            <div class="rdm-card">
                <h2 class="rdm-titulo">REGISTRO DE DESMONTAJE DE EQUIPOS</h2>
                <p class="rdm-subtitulo">Ingenio La Cabaña — Dpto. de Metrología y Tecnología Industrial</p>

                <div class="rdm-info-bloque">
                    <strong>Instrumentista:</strong> <asp:Label ID="lblNombreEmpleado" runat="server" Text="" />
                    &nbsp;&nbsp;
                    <strong>Fecha:</strong> <asp:Label ID="lblFechaHoy" runat="server" Text="" />
                </div>

                <p class="rdm-instrucciones">
                    Marque los equipos que desmontó durante el turno.<br />
                    — Los <strong>desmontados</strong> no volverán a aparecer.<br />
                    — Los <strong>no desmontados</strong> podrán aparecer nuevamente pasadas 24 horas.<br />
                    — Si indica <em>"No se puede desmontar hasta mantenimiento"</em>, tampoco volverá a aparecer.
                </p>

                <%-- Repeater de instrumentos pendientes --%>
                <asp:Repeater ID="rptPendientes" runat="server" OnItemDataBound="rptPendientes_ItemDataBound">
                    <ItemTemplate>
                        <div class="rdm-item">
                            <asp:HiddenField ID="hfTag"               runat="server" Value='<%# Eval("TAG") %>' />
                            <asp:HiddenField ID="hfDesmontajeId"      runat="server" Value='<%# Eval("DesmontajeInstrumentoId") %>' />

                            <div class="rdm-item-header">
                                <span class="rdm-area-badge"><%# Eval("NombreArea") %></span>
                                <span class="rdm-nombre"><%# Eval("NombreInstrumento") %></span>
                                <div class="rdm-radio-group">
                                    <label class="rdm-radio-label rdm-radio-si">
                                        <asp:RadioButton ID="rdoDesmontado"   runat="server" GroupName="grpDesmontaje" CssClass="rdm-rdo-si" />
                                        Desmontado
                                    </label>
                                    <label class="rdm-radio-label rdm-radio-no">
                                        <asp:RadioButton ID="rdoNoDesmontado" runat="server" GroupName="grpDesmontaje" CssClass="rdm-rdo-no" />
                                        No desmontado
                                    </label>
                                </div>
                            </div>

                            <%-- Panel de razón: visible solo si NO se marcó como desmontado --%>
                            <div class="rdm-razon-panel" id='<%# "pnlRazon_" + Container.ItemIndex %>'>
                                <div>
                                    <label>Motivo por el que no se desmontó:</label>
                                    <asp:DropDownList ID="ddlRazon" runat="server" CssClass="rdm-select">
                                        <asp:ListItem Value="">-- Seleccione --</asp:ListItem>
                                        <asp:ListItem Value="Falta de tiempo" Selected="True">Falta de tiempo</asp:ListItem>
                                        <asp:ListItem Value="Acceso restringido">Acceso restringido</asp:ListItem>
                                        <asp:ListItem Value="Falta de herramienta">Falta de herramienta</asp:ListItem>
                                        <asp:ListItem Value="Equipo en operacion">Equipo en operación</asp:ListItem>
                                        <asp:ListItem Value="Pendiente reprogramar">Pendiente reprogramar</asp:ListItem>
                                        <asp:ListItem Value="No se puede desmontar hasta mantenimiento">No se puede desmontar hasta mantenimiento</asp:ListItem>
                                        <asp:ListItem Value="Otro">Otro</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label>Detalle adicional (opcional):</label>
                                    <asp:TextBox ID="txtDetalle" runat="server" TextMode="MultiLine" CssClass="rdm-textarea" placeholder="Describa el motivo..." />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Label ID="lblSinPendientes" runat="server" Text="" CssClass="rdm-sin-pendientes" Visible="false" />

                <asp:Button ID="btnGuardar" runat="server" Text="GUARDAR Y GENERAR RUTINA"
                    CssClass="rdm-btn-guardar" OnClick="btnGuardar_Click" />
            </div>
        </div>
    </form>

    <script type="text/javascript">
        window.addEventListener('load', function () {
            var items = document.querySelectorAll('.rdm-item');
            items.forEach(function (item) {
                var rdoSi = item.querySelector('input.rdm-rdo-si');
                var rdoNo = item.querySelector('input.rdm-rdo-no');
                var panel = item.querySelector('.rdm-razon-panel');
                var lblSi = item.querySelector('.rdm-radio-si');
                var lblNo = item.querySelector('.rdm-radio-no');
                if (!panel) return;

                function actualizar() {
                    if (rdoNo && rdoNo.checked) {
                        panel.classList.add('visible');
                        if (lblNo) lblNo.classList.add('marcado');
                        if (lblSi) lblSi.classList.remove('marcado');
                    } else {
                        panel.classList.remove('visible');
                        if (lblNo) lblNo.classList.remove('marcado');
                        if (lblSi) lblSi.classList.toggle('marcado', rdoSi ? rdoSi.checked : false);
                    }
                }

                if (rdoSi) rdoSi.addEventListener('change', actualizar);
                if (rdoNo) rdoNo.addEventListener('change', actualizar);
                actualizar();
            });
        });
    </script>
</body>
</html>
