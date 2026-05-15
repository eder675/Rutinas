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

            <!-- CARRITO DE SELECCIONADOS -->
            <div id="divCarrito" class="dd-carrito" style="display:none;">
                <div class="dd-carrito-header">
                    <div>
                        <span class="dd-carrito-titulo">Equipos a declarar como desmontados</span>
                        <span id="spanCarritoConteo" class="dd-carrito-conteo"></span>
                    </div>
                    <asp:Button ID="btnGuardar" runat="server" Text="GUARDAR DECLARACION"
                        CssClass="dd-btn-guardar" OnClick="btnGuardar_Click" />
                </div>
                <div id="divCarritoLista" class="dd-carrito-lista"></div>
            </div>
            <asp:HiddenField ID="hfSeleccionados" runat="server" Value="[]" />

            <!-- RESULTADOS DE BUSQUEDA -->
            <div id="divResultados" style="display:none;">
                <div class="dd-results-bar">
                    <span id="spanConteo" class="dd-conteo"></span>
                </div>
                <asp:HiddenField ID="hfDatos" runat="server" />
                <div id="divLista" class="dd-lista"></div>
            </div>

            <asp:Label ID="lblMsg" runat="server" CssClass="dd-msg" Text=""></asp:Label>

        </div>

        <!-- TOAST -->
        <div id="dd-toast" class="dd-toast"></div>

        <script type="text/javascript">

        // ── CARRITO (acumulador de seleccionados) ─────────────────
        var carrito = {}; // { tag: {tag, descripcion, area} }
        var hfSelId = '<%= hfSeleccionados.ClientID %>';

        function agregarAlCarrito(item) {
            carrito[item.tag] = item;
            actualizarCarrito();
        }

        function quitarDelCarrito(tag) {
            delete carrito[tag];
            // Desmarcar el radio en resultados si está visible
            var fn = 'rb_' + tag.replace(/[^a-zA-Z0-9]/g, '_');
            $('input[name="' + fn + '"][value="0"]').prop('checked', true);
            $('input[name="' + fn + '"]').closest('.dd-item').removeClass('dd-item-desmontado');
            actualizarCarrito();
        }

        function actualizarCarrito() {
            var items = Object.values(carrito);
            $('#' + hfSelId).val(JSON.stringify(items));

            if (!items.length) {
                $('#divCarrito').slideUp(200);
                return;
            }

            var html = '';
            items.forEach(function (item) {
                var tagEsc = item.tag.replace(/'/g, "\\'");
                html += '<div class="dd-carrito-item">';
                html +=   '<div class="dd-item-info">';
                html +=     '<span class="dd-item-tag">'  + $('<span>').text(item.tag).html()         + '</span>';
                html +=     '<span class="dd-item-desc">' + $('<span>').text(item.descripcion).html() + '</span>';
                html +=     '<span class="dd-item-area">' + $('<span>').text(item.area).html()        + '</span>';
                html +=   '</div>';
                html +=   '<button type="button" class="dd-carrito-quitar" onclick="quitarDelCarrito(\'' + tagEsc + '\')">&#10005;</button>';
                html += '</div>';
            });

            $('#divCarritoLista').html(html);
            $('#spanCarritoConteo').text('(' + items.length + ')');
            $('#divCarrito').slideDown(200);
        }

        function limpiarCarrito() {
            carrito = {};
            actualizarCarrito();
            $('#divResultados').hide();
            $('#divLista').empty();
        }

        // ── CARGAR ÁREAS AL INICIAR ───────────────────────────────
        $.getJSON('ObtenerAreas.ashx', function (areas) {
            var $ddl = $('#ddlAreaFiltro');
            $.each(areas, function (i, area) {
                $ddl.append($('<option>').val(area).text(area));
            });
        });

        // ── BUSQUEDA AJAX ──────────────────────────────────────────
        var busqTimer;

        function ejecutarBusqueda() {
            var q    = $('#txtBusqueda').val().trim();
            var area = $('#ddlAreaFiltro').val();

            if (q.length < 2 && area === '') return;

            $('#btnBuscarJS').prop('disabled', true).text('Buscando...');

            $.getJSON('BuscarEquiposArea.ashx', { q: q, area: area })
                .done(function (data) {
                    if (!data.length) {
                        $('#divResultados').hide();
                        $('#divLista').empty();
                        mostrarToast('No se encontraron equipos.', 'error');
                        return;
                    }

                    var html = '';
                    $.each(data, function (i, item) {
                        var fn        = 'rb_' + item.tag.replace(/[^a-zA-Z0-9]/g, '_');
                        var enCarrito = carrito.hasOwnProperty(item.tag);
                        var checked0  = enCarrito ? '' : 'checked';
                        var checked1  = enCarrito ? 'checked' : '';
                        var clase     = enCarrito ? 'dd-item dd-item-desmontado' : 'dd-item';

                        html += '<div class="' + clase + '" data-tag="'  + $('<span>').text(item.tag).html()
                             + '" data-desc="' + $('<span>').text(item.descripcion).html()
                             + '" data-area="' + $('<span>').text(item.area).html() + '">';
                        html +=   '<div class="dd-item-info">';
                        html +=     '<span class="dd-item-tag">'  + $('<span>').text(item.tag).html()         + '</span>';
                        html +=     '<span class="dd-item-desc">' + $('<span>').text(item.descripcion).html() + '</span>';
                        html +=     '<span class="dd-item-area">' + $('<span>').text(item.area).html()        + '</span>';
                        html +=   '</div>';
                        html +=   '<div class="dd-item-opciones">';
                        html +=     '<label class="dd-radio-label dd-radio-no"><input type="radio" name="' + fn + '" value="0" ' + checked0 + '> No desmontado</label>';
                        html +=     '<label class="dd-radio-label dd-radio-si"><input type="radio" name="' + fn + '" value="1" ' + checked1 + '> Desmontado &#10003;</label>';
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
                    $('#btnBuscarJS').prop('disabled', false).text('Buscar');
                });
        }

        // Botón manual
        $('#btnBuscarJS').on('click', function () {
            clearTimeout(busqTimer);
            ejecutarBusqueda();
        });

        // Tiempo real al escribir (debounce 400ms)
        $('#txtBusqueda').on('input', function () {
            clearTimeout(busqTimer);
            var q    = $(this).val().trim();
            var area = $('#ddlAreaFiltro').val();
            if (q.length < 2 && area === '') { return; }
            busqTimer = setTimeout(ejecutarBusqueda, 400);
        });

        // Al cambiar el área busca inmediatamente
        $('#ddlAreaFiltro').on('change', function () {
            clearTimeout(busqTimer);
            ejecutarBusqueda();
        });

        // ── RADIO CHANGE: agregar/quitar del carrito ──────────────
        $(document).on('change', 'input[type="radio"]', function () {
            var $item = $(this).closest('.dd-item');
            var esDesmontado = $(this).val() === '1';
            $item.toggleClass('dd-item-desmontado', esDesmontado);

            var item = {
                tag:         $item.data('tag'),
                descripcion: $item.data('desc'),
                area:        $item.data('area')
            };

            if (esDesmontado) agregarAlCarrito(item);
            else              quitarDelCarrito(item.tag);
        });

        // Enter fuerza búsqueda inmediata sin esperar el debounce
        $('#txtBusqueda').on('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); clearTimeout(busqTimer); ejecutarBusqueda(); }
        });

        // ── TOAST ─────────────────────────────────────────────────
        function mostrarToast(msg, tipo) {
            var $t = $('#dd-toast');
            $t.removeClass('dd-toast-ok dd-toast-error')
              .addClass(tipo === 'error' ? 'dd-toast-error' : 'dd-toast-ok')
              .text(msg).css({ display: 'flex', opacity: 0 }).animate({ opacity: 1 }, 200);
            setTimeout(function () {
                $t.animate({ opacity: 0 }, 400, function () { $(this).hide(); });
            }, 4000);
        }

    </script>

    </form>
    <script src="shared.js"></script>
</body>
</html>
