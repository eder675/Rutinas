<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registrofallos.aspx.cs" Inherits="Rutinas.Registrofallos" %>

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

        <div class="rf-card">

            <!-- ENCABEZADO -->
            <div class="rf-header">
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
            <asp:Button ID="btninsert" runat="server" Text="INSERTAR DATOS" CssClass="rf-btn-submit" OnClick="btninsert_Click" />
            <asp:Label ID="lblMsgInsert" runat="server" Text="" Style="display:block; margin-top:10px; font-weight:600; text-align:center;" />

        </div>

    </form>
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
    </script>
</body>
</html>
