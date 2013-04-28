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
            $(data).each(function () {
                divCampos.html(" <div class='editor-label'><label for='" + this.Campo + "'>" + this.Campo + "</label></div><div id='div" + this.Campo + "'  class='editor-field'></div>");
                $(("#div" + this.Campo)).append($('<input/>').attr('name', this.Campo).attr('type','Text'));                
            });
        });
    }
}

