<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registrofallos.aspx.cs" Inherits="Rutinas.Registrofallos" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Registro de Fallos — Rutinas</title>
    <link rel="stylesheet" href="styles-shared.css" />
    <link rel="stylesheet" href="stylesregistrofallos.css" />
</head>
<body>
    <form id="form1" runat="server">

        <div class="rf-card">

            <!-- ENCABEZADO -->
            <div class="rf-header">
                <h1>REGISTRO DE FALLOS DE INSTRUMENTOS</h1>
                <p>Ingenio La Cabaña — Departamento de Instrumentación</p>
            </div>

            <!-- INSTRUMENTO -->
            <div class="rf-field">
                <asp:Label ID="Label1" runat="server" CssClass="rf-label" AssociatedControlID="TextBox1"
                    Text="INSTRUMENTO"></asp:Label>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="rf-input"
                    placeholder="TAG o nombre del instrumento"></asp:TextBox>
            </div>

            <hr class="rf-divider" />

            <!-- FALLO INDUCIDO -->
            <div class="rf-field">
                <asp:Label ID="Label2" runat="server" CssClass="rf-label"
                    Text='¿FALLO INDUCIDO? <span class="rf-hint">— determina si el fallo fue por causas naturales o tiene que ver con acciones humanas</span>'></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton1" runat="server" GroupName="FalloInducido" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton2" runat="server" GroupName="FalloInducido" Text="NO" />
                    </label>
                </div>
            </div>

            <hr class="rf-divider" />

            <!-- NIVEL DE DAÑO -->
            <div class="rf-field">
                <asp:Label ID="Label3" runat="server" CssClass="rf-label" AssociatedControlID="DropDownList1"
                    Text='NIVEL DE DAÑO <span class="rf-hint">— electrónica, transmisor esclavo, accesorio, software o pieza no reemplazable</span>'></asp:Label>
                <asp:DropDownList ID="DropDownList1" runat="server" CssClass="rf-select">
                </asp:DropDownList>
            </div>

            <!-- TIPO DE DAÑO -->
            <div class="rf-field">
                <asp:Label ID="Label4" runat="server" CssClass="rf-label" AssociatedControlID="DropDownList2"
                    Text='TIPO DE DAÑO <span class="rf-hint">— indica el tipo de daño que recibió el equipo</span>'></asp:Label>
                <asp:DropDownList ID="DropDownList2" runat="server" CssClass="rf-select">
                </asp:DropDownList>
            </div>

            <!-- COMENTARIOS ADICIONALES -->
            <div class="rf-field">
                <asp:Label ID="Label5" runat="server" CssClass="rf-label" AssociatedControlID="TextBox3"
                    Text="COMENTARIOS ADICIONALES"></asp:Label>
                <asp:TextBox ID="TextBox3" runat="server" CssClass="rf-textarea" TextMode="MultiLine"
                    placeholder="Describa detalles adicionales del fallo..."></asp:TextBox>
            </div>

            <hr class="rf-divider" />

            <!-- REEMPLAZAR POR NUEVO -->
            <div class="rf-field">
                <asp:Label ID="Label6" runat="server" CssClass="rf-label"
                    Text='¿REEMPLAZAR POR NUEVO EN EL PRÓXIMO MANTENIMIENTO? <span class="rf-hint">— indica si el equipo debe cambiarse por uno nuevo</span>'></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton3" runat="server" GroupName="Reemplazar" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton4" runat="server" GroupName="Reemplazar" Text="NO (se puede reparar)" />
                    </label>
                </div>
            </div>

            <!-- INTERVIENE EN EL PROCESO -->
            <div class="rf-field">
                <asp:Label ID="Label7" runat="server" CssClass="rf-label"
                    Text="INTERVIENE EN EL PROCESO"></asp:Label>
                <div class="rf-radio-group">
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton5" runat="server" GroupName="IntervieneProceso" Text="SI" />
                    </label>
                    <label class="rf-radio-option">
                        <asp:RadioButton ID="RadioButton6" runat="server" GroupName="IntervieneProceso" Text="NO" />
                    </label>
                </div>
            </div>

            <hr class="rf-divider" />

            <!-- SOLUCIÓN ACTUAL -->
            <div class="rf-field">
                <asp:Label ID="Label8" runat="server" CssClass="rf-label" AssociatedControlID="TextBox2"
                    Text='SOLUCIÓN ACTUAL <span class="rf-hint">— determina qué tipo de solución se le ha dado actualmente</span>'></asp:Label>
                <asp:TextBox ID="TextBox2" runat="server" CssClass="rf-textarea" TextMode="MultiLine"
                    placeholder="Describa la solución aplicada..."></asp:TextBox>
            </div>

            <!-- BOTÓN -->
            <asp:Button ID="Button1" runat="server" Text="INSERTAR DATOS" CssClass="rf-btn-submit" />

        </div>

    </form>
</body>
</html>
