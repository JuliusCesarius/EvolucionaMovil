
$(document).ready(function () {
    $("form").on("submit", function () {
        if ($("#ComentarioStaff").is(":visible")) {
            if ($("#CambioEstatusVM_Comentario")[0].value == "") {
                alert("Debe especificar un comentario");
                return false;
            }
        }
    });
    $("#FichaDeposito").hide("blind");
    $("#VerImagen").on("click", function () {
        OcultaMuestraImagen();
    }
    );
    //$("#NoCancelar").click(OcultaComentario());
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

function OcultaMuestraImagen() {

    displaying = $("#FichaDeposito").css("display");
    if (displaying == "block") {

        $("#FichaDeposito").fadeOut('slow', function () {
            $("#FichaDeposito").hide("blind");
            $("#VerImagen")[0].value = "Ver ficha de depósito"
        });

    } else {
        $("#FichaDeposito").fadeIn('slow', function () {
            $("#FichaDeposito").show("blind");
            $("#VerImagen")[0].value = "Ocultar ficha de depósito"
        });

    }
}

function goBack() {
  parent.history .back();
   
}
   