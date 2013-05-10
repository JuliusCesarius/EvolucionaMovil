$(document).on("ready", function () {
    $("a.ImagenPayCenter").colorbox({ imgError: "No se pudo cargar la imagen.", xhrError: "No se pudo cargar la imagen.", maxWidth: "95%", maxHeight: "95%" });

    var divEstatus = $("#Estatus");
    if (divEstatus != null) {
        if ($("#Accion").val() == "Activar") {
            divEstatus.addClass("PaycenterInactivo");
        }
        else {
            divEstatus.addClass("PaycenterActivo");
        }
    }

    var btnAccion = $("#EjecutarAccion")
    if (btnAccion != null) {
        btnAccion.on("click", function (event) {
            event.preventDefault();
            $("#Confirmacion").dialog("open");
        });
        $("#Confirmacion").dialog({
            modal: true,
            resizable: false,
            autoOpen: false,
            buttons: { "Si": function () { EjecutarAccion(); }, "No": function () { $("#Confirmacion").dialog("close"); } }
        });
    }
});

function EjecutarAccion() {
    $.ajax(
    {
        type: 'POST',
        async: false,
        dataType: 'text',
        url: "/PayCenters/Activate",
        data: { 'id': $("#PayCenterId").val(), 'accion': $("#Accion").val() },
        success: function (result) {
            if (result != null) {

                if (result.search("éxito") > 0) {
                    $("#ResConfirmacion").html(result + " Actualice la página para ver el nuevo estatus.");
                    $("#EjecutarAccion").prop("disabled", true);
                }
                else {
                    $("#ResConfirmacion").html(result);
                }
            }
            else {
                $("#ResConfirmacion").html("No se ha podido determinar si se ejecutó la acción.");
            }
        },
        error: function () {
            $("#ResConfirmacion").html("Se ha producido un error al ejecutar la acción.");
        }
    });
    $("#Confirmacion").dialog("close");
    $("#ResConfirmacion").addClass("message");
}