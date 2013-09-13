$(document).on("ready", function () {
    $(":checkbox").on("click", checkbox_click);
    $(":checkbox").each(function (i, item) { $(item).data("precio", $(item).attr("precio")); });
});

function checkbox_click(event) {
    var sumPrice = 0;
    $(":checkbox").parent().removeClass("sgSelRow");
    $(":checkbox").parent().find(".hddSelected").val(false);
    $(":checked").parent().find(".hddSelected").val(true);
    $(":checked").parent().addClass("sgSelRow");
    $(":checked").each(function (i, item) {
        sumPrice += parseFloat($(item).data("precio"));
    });
    var saldoActual = $("#saldoDisponible").html().replace("$", "").replace(",", "");
    var total = saldoActual - sumPrice;
    $("#subtotal").html(sumPrice.toFixed(2));
    $("#subtotal").formatCurrency();
    $("#total").html(total.toFixed(2));
    $("#total").formatCurrency();
    if (total > 0) {
        $("#total").removeClass("negative");
    } else {
        $("#total").addClass("negative");
    }
}