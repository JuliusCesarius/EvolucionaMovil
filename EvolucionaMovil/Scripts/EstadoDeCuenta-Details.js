$(document).on("ready", function () {
    if ($("#Aplicar").length > 0)
     {
        if ($("#Aplicar").attr('type') == "button") {
            $("#Aplicar").on("click", function () {
                MuestraComentario("Aplicar");
            });
        }
     }   
     if ($("#Rechazar").attr('type') == "button") {
            $("#Rechazar").on("click", function () {
                MuestraComentario("Rechazar");
            });
        }
    
    $("#ComentarioStaff").css("display", "none");
});

function MuestraComentario(action) {
   
        if ($("#ComentarioStaff").length) {
            $("#ComentarioStaff").css("display", "block");
            $("#Aplicar").css("display", "none");
            $("#Rechazar").css("display", "none");
            $("#action")[0].value = action;
            $("#actionName")[0].textContent = action;
        }

    }


    function OcultaComentario() {
        
            if ($("#ComentarioStaff").length) {
                $("#ComentarioStaff").css("display", "none");
                $("#Aplicar").css("display", "inline");
                $("#Rechazar").css("display", "inline");
            }
        
        $("#CambioEstatusVM_Comentario")[0].value = "";

    }

