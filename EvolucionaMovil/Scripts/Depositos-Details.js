
$(document).ready(function () {
    $("#FichaDeposito").css("display", "none");
    $("#VerImagen").on("click", function () {
        OcultaMuestraImagen();
    }
    );
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

function OcultaMuestraImagen() {

    displaying = $("#FichaDeposito").css("display");
    if (displaying == "block") {

        $("#FichaDeposito").fadeOut('slow', function () {
            $("#FichaDeposito").css("display", "none");
            $("#VerImagen")[0].value = "Ver ficha de depósito"
        });

    } else {
        $("#FichaDeposito").fadeIn('slow', function () {
            $("#FichaDeposito").css("display", "block");
            $("#VerImagen")[0].value = "Ocultar ficha de depósito"
        });

    }
}

function goBack() {
  parent.history .back();
   
}
   