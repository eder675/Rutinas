<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registrofallos.aspx.cs" Inherits="Rutinas.Registrofallos" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Registro de Fallos — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesregistrofallos.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">

        <div id="rf-toast" class="rf-toast"></div>

        <div class="rf-card">

            <!-- ENCABEZADO -->
            <div class="rf-header">
                <asp:Button ID="btnSalir" runat="server" Text="← Salir" OnClick="btnSalir_Click"
                    CssClass="rf-btn-salir" CausesValidation="false" />
                <h1>REGISTRO DE FALLOS DE INSTRUMENTOS</h1>
                <p>Ingenio La Cabaña — Departamento de Tecnologia Industrial</p>
            </div>

            <!-- INSTRUMENTO -->
            <div class="rf-field">
                <asp:Label ID="Label1" runat="server" CssClass="rf-label" Text="INSTRUMENTO"></asp:Label>
                <div class="rf-search-wrap">
                    <input type="text" id="txtBusqueda" class="rf-input" placeholder="Busque por TAG o descripción..." autocomplete="off" />
                    <div id="divResultados" class="rf-autocomplete"></div>
                </div>
                <asp:HiddenField ID="hfTAG" runat="server" />
                <asp:HiddenField ID="hfDescripcion" runat="server" />
                <div class="rf-tag-desc">
                    <div>
                        <span class="rf-label-sm">TAG</span>
                        <asp:TextBox ID="txtTAG" runat="server" CssClass="rf-input rf-input-readonly" ReadOnly="true" Width="90px"></asp:TextBox>
                    </div>
                    <div class="rf-tag-desc-full">
                        <span class="rf-label-sm">Descripción</span>
                        <asp:TextBox ID="txtdescripcion" runat="server" CssClass="rf-input rf-input-readonly" ReadOnly="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <hr class="rf-divider" />

            <!-- FALLO INDUCIDO -->
            <div class="rf-field">
                <asp:Label ID="Label2" runat="server" CssClass="rf-label"
                    Text='¿FALLO INDUCIDO? <span class="rf-hint">— determina si el fallo fue por causas naturales o tiene que ver con acciones humanas</span>'></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbinducitrue" runat="server" GroupName="FalloInducido" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbinducifalse" runat="server" GroupName="FalloInducido" Text="NO" />
                    </label>
                </div>
            </div>

            <hr class="rf-divider" />

            <!-- NIVEL DE DAÑO -->
            <div class="rf-field">
                <asp:Label ID="Label3" runat="server" CssClass="rf-label" AssociatedControlID="ddlniveldano"
                    Text='NIVEL DE DAÑO <span class="rf-hint">— electrónica, transmisor esclavo, accesorio, software o pieza no reemplazable</span>'></asp:Label>
                <asp:DropDownList ID="ddlniveldano" runat="server" CssClass="rf-select">
                </asp:DropDownList>
            </div>

            <!-- TIPO DE DAÑO -->
            <div class="rf-field">
                <asp:Label ID="Label4" runat="server" CssClass="rf-label" AssociatedControlID="ddltipodano"
                    Text='TIPO DE DAÑO <span class="rf-hint">— indica el tipo de daño que recibió el equipo</span>'></asp:Label>
                <asp:DropDownList ID="ddltipodano" runat="server" CssClass="rf-select" OnSelectedIndexChanged="ddltipodano_SelectedIndexChanged">
                </asp:DropDownList>
            </div>

            <div class="rf-field">
                <asp:Label ID="Label9" runat="server" CssClass="rf-label" AssociatedControlID="ddlcausadano"
                    Text='POSIBLE CAUSA DEL DAÑO &lt;span class="rf-hint"&gt;— Posible causa del fallo del instrumento&lt;/span&gt;'></asp:Label>
                <asp:DropDownList ID="ddlcausadano" runat="server" CssClass="rf-select">
                </asp:DropDownList>
            </div>

            <br />

            <!-- COMENTARIOS ADICIONALES -->
            <div class="rf-field">
                <asp:Label ID="Label5" runat="server" CssClass="rf-label" AssociatedControlID="txtcomentario"
                    Text="COMENTARIOS ADICIONALES"></asp:Label>
                <asp:TextBox ID="txtcomentario" runat="server" CssClass="rf-textarea" TextMode="MultiLine"
                    placeholder="Describa detalles adicionales del fallo..."></asp:TextBox>
            </div>

            <hr class="rf-divider" />

            <!-- REEMPLAZAR POR NUEVO -->
            <div class="rf-field">
                <asp:Label ID="Label6" runat="server" CssClass="rf-label"
                    Text='¿REEMPLAZAR POR NUEVO EN EL PRÓXIMO MANTENIMIENTO? <span class="rf-hint">— indica si el equipo debe cambiarse por uno nuevo</span>'></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbtremplazatrue" runat="server" GroupName="Reemplazar" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbtremplazafalse" runat="server" GroupName="Reemplazar" Text="NO (se puede reparar)" />
                    </label>
                </div>
            </div>

            <!-- INTERVIENE EN EL PROCESO -->
            <div class="rf-field">
                <asp:Label ID="Label7" runat="server" CssClass="rf-label"
                    Text="INTERVIENE EN EL PROCESO"></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbtintervienetrue" runat="server" GroupName="IntervieneProceso" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="rbtintervienefalse" runat="server" GroupName="IntervieneProceso" Text="NO" />
                    </label>
                </div>
            </div>

            <hr class="rf-divider" />

            <!-- SOLUCIÓN ACTUAL -->
            <div class="rf-field">
                <asp:Label ID="Label8" runat="server" CssClass="rf-label" AssociatedControlID="ddlsolucion"
                    Text='SOLUCIÓN ACTUAL <span class="rf-hint">— determina qué tipo de solución se le ha dado actualmente</span>'></asp:Label>
                <asp:DropDownList ID="ddlsolucion" runat="server" CssClass="rf-select">
                </asp:DropDownList>
            </div>

            <!-- BOTÓN -->
            <asp:Label ID="Label10" runat="server" CssClass="rf-label" Text="Nombre de quien realiza el registro: "></asp:Label>
            <asp:Label ID="lblname" runat="server" Text="Label"></asp:Label>
            <asp:Button ID="btninsert" runat="server" Text="INSERTAR DATOS" CssClass="rf-btn-submit"
                OnClick="btninsert_Click" OnClientClick="return mostrarConfirmacion();" />
            <asp:Label ID="lblMsgInsert" runat="server" Text="" Style="display:block; margin-top:10px; font-weight:600; text-align:center;" />

        </div>

    </form>

    <!-- MODAL DE CONFIRMACIÓN — fuera del form para evitar conflictos con flexbox del body -->
    <div id="modal-overlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(0,0,0,0.45); z-index:9999; align-items:center; justify-content:center;">
        <div style="background:#fff; border-radius:12px; border-top:4px solid #2E7D32; box-shadow:0 8px 32px rgba(0,0,0,0.22); padding:28px 32px 24px; width:90%; max-width:520px; max-height:90vh; overflow-y:auto; box-sizing:border-box; font-family:Arial,sans-serif;">
            <h2 style="font-size:1.1em; color:#1B5E20; margin:0 0 18px; font-weight:700;">Confirmar registro</h2>
            <table style="width:100%; border-collapse:collapse; font-size:0.88em; margin-bottom:20px;">
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666; width:42%;">Instrumentista</td><td style="padding:7px 6px;" id="m-instrumentista"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">TAG</td><td style="padding:7px 6px;" id="m-tag"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Instrumento</td><td style="padding:7px 6px;" id="m-instrumento"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Fallo inducido</td><td style="padding:7px 6px;" id="m-inducido"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Nivel de daño</td><td style="padding:7px 6px;" id="m-nivel"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Tipo de daño</td><td style="padding:7px 6px;" id="m-tipo"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Posible causa</td><td style="padding:7px 6px;" id="m-causa"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Reemplazar</td><td style="padding:7px 6px;" id="m-reemplaza"></td></tr>
                <tr style="border-bottom:1px solid #e0e0e0;"><td style="padding:7px 6px; font-weight:600; color:#666;">Interviene proceso</td><td style="padding:7px 6px;" id="m-interviene"></td></tr>
                <tr><td style="padding:7px 6px; font-weight:600; color:#666;">Solución actual</td><td style="padding:7px 6px;" id="m-solucion"></td></tr>
            </table>
            <div style="display:flex; gap:10px; justify-content:flex-end;">
                <button type="button" onclick="cerrarModal()" style="padding:9px 22px; border-radius:8px; font-size:0.9em; font-weight:600; font-family:inherit; cursor:pointer; background:#f5f5f5; border:1.5px solid #ccc; color:#333;">Cancelar</button>
                <button type="button" onclick="confirmarInsercion()" style="padding:9px 22px; border-radius:8px; font-size:0.9em; font-weight:600; font-family:inherit; cursor:pointer; background:linear-gradient(135deg,#2E7D32,#1B5E20); color:#fff; border:none;">Aceptar</button>
            </div>
        </div>
    </div>
    <script type="text/javascript">
    (function () {
        var timer;
        var $input   = $('#txtBusqueda');
        var $results = $('#divResultados');
        var tagId    = '<%= txtTAG.ClientID %>';
        var descId   = '<%= txtdescripcion.ClientID %>';
        var hfTagId  = '<%= hfTAG.ClientID %>';
        var hfDescId = '<%= hfDescripcion.ClientID %>';

        $input.on('keyup', function () {
            clearTimeout(timer);
            var q = $(this).val().trim();
            if (q.length < 2) { $results.hide().empty(); return; }

            timer = setTimeout(function () {
                $.getJSON('BuscarInstrumento.ashx', { q: q }, function (data) {
                    $results.empty();
                    if (data.length === 0) {
                        $results.append('<div class="rf-ac-item rf-ac-empty">Sin resultados</div>');
                    } else {
                        $.each(data, function (i, item) {
                            $('<div class="rf-ac-item"></div>')
                                .html('<strong>' + $('<span>').text(item.tag).html() + '</strong> &mdash; ' + $('<span>').text(item.descripcion).html())
                                .on('click', function () {
                                    $('#' + tagId).val(item.tag);
                                    $('#' + descId).val(item.descripcion);
                                    $('#' + hfTagId).val(item.tag);
                                    $('#' + hfDescId).val(item.descripcion);
                                    $input.val(item.tag + ' — ' + item.descripcion);
                                    $results.hide().empty();
                                })
                                .appendTo($results);
                        });
                    }
                    $results.show();
                });
            }, 300);
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('.rf-search-wrap').length)
                $results.hide();
        });
    })();

    // ── CASCADA DE DROPDOWNS ──
    var filtros = {
        'Transmisor o posicionador': {
            tipos: ['No se auto calibra','Se calibra pero tiempo despues falla','Fallo por alta temperatura','Fallo por alta vibracion'],
            causas: ['Ruido eléctrico','No es el posicionador indicado para la aplicacion','No es el transmisor apropiado para la aplicacion','Mala parametrización','Transmisor mal instalado'],
            soluciones: ['Reemplazo temporal por otro transmisor','Se calibró','Se cambio por otro transmisor con otro tipo de lectura','Reemplazo por otra marca de posicionador']
        },
        'Electrónica del transmisor': {
            tipos: ['Fallo por sensor golpeado','Fallo de señal (4-20mA)','Fallo electronico por condiciones inapropiadas','Daño físico por caida','Sobrecalentamiento por fallo electronico','Sobrecalentamiento por mala ubicacion','Corrosión de bornes','Canal dañado (4-20mA)','Salida analógica dañada','Entrada analógica dañada'],
            causas: ['No fue protegido contra el clima adverso','Desgaste natural','Ruido eléctrico','Vibraciones excesivas','Mal manejo del equipo','Falta de mantenimiento','Condiciones ambientales (lluvia)','Condiciones ambientales (calor excesivo)','Agua que entro por la coraza','Fue golpeado por una persona','Impacto por rayo','Ubicacion donde se puede inundar o caer agua','Derrame de miel u otro liquido que pueda filtrarse'],
            soluciones: ['Reemplazo temporal','Reemplazo de placa','Reparacion en el taller','Reemplazo de la electronica','Reemplazo de su sensor','Fue movido de donde estaba','Se tiro cable desde otro lado o solo se cambio el cable','Se le puso techo provisional','Se agregó o adaptó pieza que ayuda a estabilizarse','Cambio de canal de la salida analogica','Cambio de canal de la entrada analógica']
        },
        'Transmisor esclavo': {
            tipos: ['Fallo por sensor golpeado','Fallo de señal (4-20mA)','Fallo electronico por condiciones inapropiadas','Daño físico por caida','Sobrecalentamiento por fallo electronico','Sobrecalentamiento por mala ubicacion','Corrosión de bornes','Canal dañado (4-20mA)','Salida analógica dañada','Entrada analógica dañada'],
            causas: ['No fue protegido contra el clima adverso','Desgaste natural','Ruido eléctrico','Vibraciones excesivas','Mal manejo del equipo','Falta de mantenimiento','Condiciones ambientales (lluvia)','Condiciones ambientales (calor excesivo)','Agua que entro por la coraza','Fue golpeado por una persona','Impacto por rayo','Ubicacion donde se puede inundar o caer agua','Derrame de miel u otro liquido que pueda filtrarse'],
            soluciones: ['Reemplazo temporal','Reemplazo de placa','Reparacion en el taller','Reemplazo de la electronica','Reemplazo de su sensor','Fue movido de donde estaba','Se tiro cable desde otro lado o solo se cambio el cable','Se le puso techo provisional','Se agregó o adaptó pieza que ayuda a estabilizarse','Cambio de canal de la salida analogica','Cambio de canal de la entrada analógica']
        },
        'Placa perteneciente al equipo': {
            tipos: ['Fallo por sensor golpeado','Fallo de señal (4-20mA)','Fallo electronico por condiciones inapropiadas','Daño físico por caida','Sobrecalentamiento por fallo electronico','Sobrecalentamiento por mala ubicacion','Corrosión de bornes','Canal dañado (4-20mA)','Salida analógica dañada','Entrada analógica dañada','Fallo eléctrico por humedad','Fallo eléctrico por mala conexion','Fallo eléctrico por inundacion','Fallo eléctrico por sobretension'],
            causas: ['No fue protegido contra el clima adverso','Desgaste natural','Ruido eléctrico','Vibraciones excesivas','Mal manejo del equipo','Falta de mantenimiento','Condiciones ambientales (lluvia)','Condiciones ambientales (calor excesivo)','Sobretensión eléctrica','Tuberias y coraza en malas condiciones','Los cables que llegan al equipo no estan protegidos','Agua que entro por la coraza','Agua que entro por una abertura','Caja de campo en mal estado','Fue golpeado por una persona','Impacto por rayo','Ubicacion donde se puede inundar o caer agua','Derrame de miel u otro liquido que pueda filtrarse'],
            soluciones: ['Reemplazo temporal','Reemplazo de placa','Reparacion en el taller','Reemplazo de la electronica','Reemplazo de su sensor','Fue movido de donde estaba','Se tiro cable desde otro lado o solo se cambio el cable','Se le puso techo provisional','Se agregó o adaptó pieza que ayuda a estabilizarse','Cambio de canal de la salida analogica','Cambio de canal de la entrada analógica']
        },
        'Fuente de poder': {
            tipos: ['Fallo eléctrico por humedad','Fallo eléctrico por mala conexion','Fallo eléctrico por inundacion','Fallo eléctrico por sobretension','Corrosión de bornes'],
            causas: ['Condiciones ambientales (lluvia)','Sobretensión eléctrica','Tuberias y coraza en malas condiciones','Los cables que llegan al equipo no estan protegidos','Agua que entro por la coraza','Agua que entro por una abertura','Caja de campo en mal estado','Impacto por rayo','Ubicacion donde se puede inundar o caer agua'],
            soluciones: ['Reemplazo temporal','Reemplazo de placa','Reparacion en el taller','Fue movido de donde estaba','Se tiro cable desde otro lado o solo se cambio el cable','Se le puso techo provisional']
        },
        'Accesorio mecanico': {
            tipos: ['Fallo mecánico por desacople','Fallo mecanico por mal montaje','Fallo mecanico por golpe','Fallo de sellos','Fuga de aire'],
            causas: ['Sellos dañados por temperatura','Empaque o sello no reemplazado en mantenimiento','Fue golpeado por una persona','Le cayo algo encima'],
            soluciones: ['Reemplazo temporal','Reemplazo de KIT de sellos','Reemplazo de pieza dañada','Reparación en sitio','Reparacion en el taller','Se instalo un soporte extra','Fue movido de donde estaba']
        },
        'Software o parametrización incorrecta': {
            tipos: ['No se auto calibra','No responde el teclado','Se reinicia por si solo','Esta bloqueado y no hay clave para desbloquear'],
            causas: ['Descarga de programa con errores de compilacion','Software corrompido','Mal parametrizado','No estaba parametrizado'],
            soluciones: ['Reemplazo temporal','Reemplazo de placa','Reemplazo de la CU','Reemplazo de componente dañado','Reinstalar firmware']
        }
    };

    var nivelId   = '<%= ddlniveldano.ClientID %>';
    var tipoId    = '<%= ddltipodano.ClientID %>';
    var causaId   = '<%= ddlcausadano.ClientID %>';
    var solucionId = '<%= ddlsolucion.ClientID %>';

    function poblarDDL(id, opciones) {
        var $ddl = $('#' + id);
        $ddl.empty().append('<option value="">-- Seleccione --</option>');
        $.each(opciones || [], function(i, op) {
            $ddl.append($('<option>').val(op).text(op));
        });
    }

    $('#' + nivelId).on('change', function() {
        var f = filtros[$(this).val()];
        poblarDDL(tipoId,    f ? f.tipos     : []);
        poblarDDL(causaId,   f ? f.causas    : []);
        poblarDDL(solucionId, f ? f.soluciones : []);
    });

    var btnInsertUniqueId = '<%= btninsert.UniqueID %>';

    function mostrarConfirmacion() {
        var $lbl = $('#<%= lblMsgInsert.ClientID %>');
        $lbl.text('');

        var tag = $('#<%= txtTAG.ClientID %>').val();
        if (!tag) { $lbl.text('Seleccione un instrumento antes de guardar.'); return false; }

        var nivelVal   = $('#' + nivelId).val();
        var tipoVal    = $('#' + tipoId).val();
        var causaVal   = $('#' + causaId).val();
        var solucionVal = $('#' + solucionId).val();

        if (!nivelVal)   { $lbl.text('Seleccione el nivel de daño.');   return false; }
        if (!tipoVal)    { $lbl.text('Seleccione el tipo de daño.');    return false; }
        if (!causaVal)   { $lbl.text('Seleccione la posible causa.');   return false; }
        if (!solucionVal){ $lbl.text('Seleccione la solución actual.'); return false; }

        var inducido   = $('#<%= rbinducitrue.ClientID %>').is(':checked') ? 'SI' : ($('#<%= rbinducifalse.ClientID %>').is(':checked') ? 'NO' : '');
        var reemplaza  = $('#<%= rbtremplazatrue.ClientID %>').is(':checked') ? 'SI' : ($('#<%= rbtremplazafalse.ClientID %>').is(':checked') ? 'NO' : '');
        var interviene = $('#<%= rbtintervienetrue.ClientID %>').is(':checked') ? 'SI' : ($('#<%= rbtintervienefalse.ClientID %>').is(':checked') ? 'NO' : '');

        if (!inducido)   { $lbl.text('Indique si el fallo fue inducido.');             return false; }
        if (!reemplaza)  { $lbl.text('Indique si el equipo debe reemplazarse.');        return false; }
        if (!interviene) { $lbl.text('Indique si el equipo interviene en el proceso.'); return false; }

        $('#m-instrumentista').text($('#<%= lblname.ClientID %>').text());
        $('#m-tag').text(tag);
        $('#m-instrumento').text($('#<%= txtdescripcion.ClientID %>').val());
        $('#m-inducido').text(inducido);
        $('#m-nivel').text(nivelVal);
        $('#m-tipo').text(tipoVal);
        $('#m-causa').text(causaVal);
        $('#m-reemplaza').text(reemplaza);
        $('#m-interviene').text(interviene);
        $('#m-solucion').text(solucionVal);

        $('#modal-overlay').css({ display: 'flex', opacity: 0 }).stop().animate({ opacity: 1 }, 200);
        return false;
    }

    function cerrarModal() {
        $('#modal-overlay').animate({ opacity: 0 }, 150, function () { $(this).hide(); });
    }

    function confirmarInsercion() {
        $('#modal-overlay').hide();
        __doPostBack(btnInsertUniqueId, '');
    }

    function mostrarToast(mensaje, tipo) {
        var $t = $('#rf-toast');
        $t.removeClass('rf-toast-ok rf-toast-error')
          .addClass(tipo === 'error' ? 'rf-toast-error' : 'rf-toast-ok')
          .text(mensaje).fadeIn(200);
        setTimeout(function () { $t.fadeOut(400); }, 4000);
    }
    </script>
</body>
</html>
