﻿
$(document).on("ready", function () {
  //  $("#hddRutaFichaDeposito").val($("#hddRutaFichaDep").val());
    //    $('iframe').contents().find("#uploadfile").on("change", SubirImagen);
    window.parent.document.getElementById("hddRutaFichaDeposito").value = $("#hddRutaFichaDep").val();
    $("#uploadfile").on("change", SubirImagen);
    $("#btnCargarImagen").on("click", CargarImagen);
});

function SubirImagen()
{
    $("#formuploadfile").submit();
}

function CargarImagen() {
    $("#uploadfile").click();
}