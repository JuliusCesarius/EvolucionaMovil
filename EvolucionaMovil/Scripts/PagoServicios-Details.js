$(document).ready(function () {
    $("form").on("submit", function () {
        if ($("#ComentarioStaff").is(":visible")) {
            if ($("#CambioEstatusVM_Comentario")[0].value == "") {
                alert("Debe especificar un comentario");
                return false;
            }
        }
    });
});
function OcultaComentario() {
    if ($("#ComentarioPayCenter").length) {
        $("#ComentarioPayCenter").hide("blind");
        $("#Cancelar").show("blind");

    } else {
        if ($("#ComentarioStaff").length) {
            $("#ComentarioStaff").hide("blind");
            $("#buttons").show("blind");
            $("#Imagen").show("blind");
        }
    }
    $("#CambioEstatusVM_Comentario")[0].value = "";

}

function MuestraComentario(action) {
    $("#buttons").hide("blind");
    if ($("#ComentarioPayCenter").length) {
        $("#ComentarioPayCenter").show("blind");
        $("#Cancelar").hide("blind");

    } else {
        if ($("#ComentarioStaff").length) {
            $("#ComentarioStaff").show("blind");
            $("#action")[0].value = action;
            $("#actionName")[0].textContent = action;
        }
    }
}