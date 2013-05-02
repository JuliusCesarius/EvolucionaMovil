
$(document).ready(function () {
    //OcultaComentario();
   // $("Cancelar").click(MuestraComentario);
    //$("#NoCancelar").click(OcultaComentario());
});

function OcultaComentario() {
    if ($("#ComentarioPayCenter").length) {
        $("#ComentarioPayCenter").css("display", "none");
        $("#Cancelar").css("display", "block");
    
    } else {
        if ($("#ComentarioStaff").length) {
            $("#ComentarioStaff").css("display", "none");
            $("#Aplicar").css("display", "inline");
            $("#Imagen").css("display", "inline");
        }
    }
    $("#CambioEstatusVM_Comentario")[0].value = "";

}

function MuestraComentario(action) {
    if ($("#ComentarioPayCenter").length) {
        $("#ComentarioPayCenter").css("display", "block");
        $("#Cancelar").css("display", "none");

    } else {
        if ($("#ComentarioStaff").length) {
            $("#ComentarioStaff").css("display", "block");
            $("#Aplicar").css("display", "none");
            $("#Rechazar").css("display", "none");
            $("#action")[0].value = action;
            $("#actionName")[0].textContent = action;
        }
    }
}

function goBack() {
  parent.history .back();
   
}
   