$(document).on("ready", function () {
    $("#FechaVencimiento").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#FechaVencimiento").val();

    $("#ServicioId").change(function () { getDetalleServicio(); });
});

function getDetalleServicio() {
    var servicioId = $("#ServicioId").val();
    if (servicioId > 0) {
        $.getJSON("/PagoServicios/getDetalleServicio?servicioId=" + servicioId, function (data) {
            var divCampos = $("#camposAdicionales");
            divCampos.html("");
            $(data).each(function () {
                divCampos.append(" <div class='editor-label'><label for='" + this.Campo + "'>" + this.Campo + "</label></div><div id='div" + this.DetalleServicioId + "'  class='editor-field'></div>");
                $(("#div" + this.DetalleServicioId)).append($('<input/>').attr('name', this.Campo).attr('type', 'Text'));
            });
        });
    }
}

