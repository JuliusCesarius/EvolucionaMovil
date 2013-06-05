$(document).on("ready", function () {
    $("#btnSave").on("click", ValidateCheck);
    $("#ViewInfo").on("click", ShowInfo);
    $("#btnPerfecto").on("click", HideInfo);
    //$("#acceptTerms").on("change", DisplayValidateCheck);
    $("#preafiliacioninfo").css("display","none");
});

function ValidateCheck(event) {
    if (!$("#acceptTerms").prop("checked")) {
        $("#checkMessage").html("Es necesario que lea y acepte el “Aviso de Privacidad”.");
        event.preventDefault();
    }
    else { 
        $("#checkMessage").html("");
    }
}
function ShowInfo() {
    $("#preafiliacioninfo").css("display", "");
    $("#preafiliacion").css("display", "none");
}
function HideInfo() {
    $("#preafiliacioninfo").css("display", "none");
    $("#preafiliacion").css("display", "");
}

//function DisplayValidateCheck(event) {
//    if (!$("#acceptTerms").prop("checked")) {
//        $("#checkMessage").style("");
//    }
//}