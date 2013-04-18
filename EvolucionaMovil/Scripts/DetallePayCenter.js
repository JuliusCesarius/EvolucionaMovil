$(document).on("ready", function () {
    FormatoFecha();
});

function FormatoFecha() {
    var valor = $("#FechaIngreso").val();
    valor = $.datepicker.formatDate('dd/mm/yy', valor);
    alert(valor);
    $("#FechaIngreso").val(valor);
}