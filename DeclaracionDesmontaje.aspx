<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeclaracionDesmontaje.aspx.cs"
    Inherits="Rutinas.DeclaracionDesmontaje" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Declaracion de Desmontaje — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesdeclaracion.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div class="dd-card">

            <!-- ENCABEZADO -->
            <div class="dd-header">
                <asp:Button ID="btnSalir" runat="server" Text="&larr; Salir"
                    CssClass="dd-btn-salir" CausesValidation="false"
                    OnClick="btnSalir_Click" />
                <h1>DECLARACION DE DESMONTAJE</h1>
                <p>Ingenio La Cabaña — Departamento de Tecnologia Industrial</p>
            </div>

            <!-- INFO EMPLEADO -->
            <div class="dd-info-empleado">
                <span>Declarado por: </span>
                <asp:Label ID="lblNombre" runat="server" CssClass="dd-nombre-empleado"></asp:Label>
            </div>

            <!-- BUSQUEDA -->
            <div class="dd-search-section">
                <div class="dd-search-row">
                    <div class="dd-search-field">
                        <label class="dd-label" for="ddlAreaFiltro">Filtrar por área</label>
                        <select id="ddlAreaFiltro" class="dd-input">
                            <option value="">-- Todas las áreas --</option>
                        </select>
                    </div>
                    <div class="dd-search-field dd-search-field-grow">
                        <label class="dd-label" for="txtBusqueda">Buscar por TAG o descripción</label>
                        <input type="text" id="txtBusqueda" class="dd-input"
                            placeholder="Opcional: escriba TAG o descripción..." autocomplete="off" />
                    </div>
                    <div class="dd-search-btn-wrap">
                        <label class="dd-label">&nbsp;</label>
                        <button type="button" id="btnBuscarJS" class="dd-btn-buscar">Buscar</button>
                    </div>
                </div>
            </div>

            <!-- RESULTADOS -->
            <div id="divResultados" style="display:none;">

                <div class="dd-results-bar">
                    <span id="spanConteo" class="dd-conteo"></span>
                    <asp:Button ID="btnGuardar" runat="server" Text="GUARDAR DECLARACION"
                        CssClass="dd-btn-guardar" OnClick="btnGuardar_Click" />
                </div>

                <asp:HiddenField ID="hfDatos" runat="server" />

                <div id="divLista" class="dd-lista"></div>

                <div class="dd-results-bar dd-results-bar-bottom">
                    <span></span>
                    <asp:Button ID="btnGuardar2" runat="server" Text="GUARDAR DECLARACION"
                        CssClass="dd-btn-guardar" OnClick="btnGuardar_Click" />
                </div>

            </div>

            <asp:Label ID="lblMsg" runat="server" CssClass="dd-msg" Text=""></asp:Label>

        </div>

        <!-- TOAST -->
        <div id="dd-toast" class="dd-toast"></div>

        <script type="text/javascript">

        // ── CARGAR ÁREAS AL INICIAR ───────────────────────────────
        $.getJSON('ObtenerAreas.ashx', function (areas) {
            var $ddl = $('#ddlAreaFiltro');
            $.each(areas, function (i, area) {
                $ddl.append($('<option>').val(area).text(area));
            });
        });

        // ── BUSQUEDA AJAX ──────────────────────────────────────────
        $('#btnBuscarJS').on('click', function () {
            var q    = $('#txtBusqueda').val().trim();
            var area = $('#ddlAreaFiltro').val();

            if (q.length < 2 && area === '') {
                mostrarToast('Seleccione un área o ingrese al menos 2 caracteres.', 'error');
                return;
            }

            var $btn = $(this).prop('disabled', true).text('Buscando...');

            $.getJSON('BuscarEquiposArea.ashx', { q: q, area: area })
                .done(function (data) {
                    if (!data.length) {
                        mostrarToast('No se encontraron equipos.', 'error');
                        return;
                    }

                    // Guardar JSON en hidden field para el postback
                    $('#<%= hfDatos.ClientID %>').val(JSON.stringify(data));

                    // Construir HTML de items
                    var html = '';
                    $.each(data, function (i, item) {
                        var fn = 'rb_' + item.tag.replace(/[^a-zA-Z0-9]/g, '_');
                        html += '<div class="dd-item" data-tag="' + $('<span>').text(item.tag).html() + '">';
                        html +=   '<div class="dd-item-info">';
                        html +=     '<span class="dd-item-tag">'  + $('<span>').text(item.tag).html()         + '</span>';
                        html +=     '<span class="dd-item-desc">' + $('<span>').text(item.descripcion).html() + '</span>';
                        html +=     '<span class="dd-item-area">' + $('<span>').text(item.area).html()        + '</span>';
                        html +=   '</div>';
                        html +=   '<div class="dd-item-opciones">';
                        html +=     '<label class="dd-radio-label dd-radio-no">';
                        html +=       '<input type="radio" name="' + fn + '" value="0" checked> No desmontado';
                        html +=     '</label>';
                        html +=     '<label class="dd-radio-label dd-radio-si">';
                        html +=       '<input type="radio" name="' + fn + '" value="1"> Desmontado &#10003;';
                        html +=     '</label>';
                        html +=   '</div>';
                        html += '</div>';
                    });

                    $('#divLista').html(html);
                    $('#spanConteo').text(data.length + ' equipo(s) encontrado(s)');
                    $('#divResultados').slideDown(300);
                })
                .fail(function (xhr) {
                    mostrarToast('Error ' + xhr.status + ': ' + xhr.statusText, 'error');
                })
                .always(function () {
                    $btn.prop('disabled', false).text('Buscar');
                });
        });

        // ── RESALTAR ITEM AL MARCAR DESMONTADO ───────────────────
        $(document).on('change', 'input[type="radio"]', function () {
            var $item = $(this).closest('.dd-item');
            $item.toggleClass('dd-item-desmontado', $(this).val() === '1');
        });

        // ── ENTER EN CAMPO DE BUSQUEDA ────────────────────────────
        $('#txtBusqueda').on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                $('#btnBuscarJS').trigger('click');
            }
        });

        // ── TOAST ─────────────────────────────────────────────────
        function mostrarToast(msg, tipo) {
            var $t = $('#dd-toast');
            $t.removeClass('dd-toast-ok dd-toast-error')
              .addClass(tipo === 'error' ? 'dd-toast-error' : 'dd-toast-ok')
              .text(msg)
              .css({ display: 'flex', opacity: 0 })
              .animate({ opacity: 1 }, 200);

            setTimeout(function () {
                $t.animate({ opacity: 0 }, 400, function () {
                    $(this).hide();
                });
            }, 4000);
        }

    </script>

    </form>
</body>
</html>
