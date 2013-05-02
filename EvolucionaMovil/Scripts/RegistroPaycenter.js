$(document).on("ready", function () {
    $("#FechaIngreso").datepicker({ "dateFormat": "dd/mm/yy", maxDate: '0' });
    $("#PayCenterPadre").on("change", function (event) {
        event.preventDefault();
        BuscarPaycenter();
    });
    $("#btnGuardar").on("click", function (event) {
        event.preventDefault();
        $("#FormDatos").submit();
    });

//    $("#MaximoAFinanciar").on("click", function (event) {
//        event.preventDefault();
//        var valor = $("#MaximoAFinanciar").val();
//        valor = valor.replace(/\,/g, '');
//        $("#MaximoAFinanciar").val(valor);
//    });
});

function BuscarPaycenter() {
    var captura = $("#PayCenterPadre").val();
    if (captura != "") {
        $.ajax(
        {
            type: 'POST',
            async: true,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            url: "/PayCenters/GetPayCenterRecomienda",
            data: JSON.stringify({ 'searchString': captura }),
            success: function (result) {
                result = $.parseJSON(result);
                if (result != null) {
                    $("#PayCenterPadre").val(result.UserName);
                    $("#PayCenterPadreId").val(result.PayCenterId);
                }
                else {
                    $("#PayCenterPadre").val("");
                    $("#PayCenterPadreId").val(0);
                    alert("El PayCenter especificado no está registrado.");
                }
            }
        });
    }
    else {
        $("#PayCenterPadreId").val(0);
    }
}