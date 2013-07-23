$(document).on("ready", function () {
    $('#cuentas .open').on("click", openClick);
    $('#cuentas .cuenta').on("click", cuentaClick);
    $('.tiposervicio a').on("click", tipoCuentaClick);
    $('.proveedor').on("click", proveedorClick);
    $('#BancoId').on("change", bancoIdChanged);
    $('#BancoId').trigger("change");
    $("#saveConfig").on("click", saveConfigClick);
});
function openClick(event) {
    var selected = $(event.currentTarget)[0];
    //$(selected).parent().toggleClass("closed");
    $(selected).parent().find(".detail").toggle("blind");
}
function cuentaClick(event) {
    var selected = $(event.currentTarget)[0];
    $('#cuentas .cuenta').removeClass("selected");
    $(selected).addClass("selected");

    $('.proveedor').removeClass("selected");
    $(":checkbox").prop("checked", false);
    $('.hddSelected').val(false);

    $('.proveedor').show("blind");

    var provs = $.parseJSON($(selected).find(".hddProvs").val());
    $(provs).each(function (index) {
        $('.proveedor.' + this).trigger("click");
    });

    $('#CuentaBancariaId').val($(selected).find(".CuentaId").val());

    event.preventDefault();
    event.stopPropagation();
}
function tipoCuentaClick(event) {
    var selected = $(event.currentTarget)[0];
    $('.tiposervicio a').removeClass("selected");
    $(selected).addClass("selected");
    $('.proveedor').hide('blind');
    $('[tipo=' + $(selected).attr("tiposerv") + ']').parent().parent().show('blind');
}
function proveedorClick(event) {
    var selected = $(event.currentTarget)[0];
    $(selected).toggleClass("selected");
    $(selected).find(".hddSelected").val($(selected).hasClass("selected"));
    $(selected).find(":checkbox").prop("checked", $(selected).hasClass("selected"));
    $('.tiposervicio a').removeClass("selected");
}
function bancoIdChanged(event) {
    var selected = $(event.currentTarget)[0];
    $('.cuenta').hide("blind");
    $('.cuenta.' + $(selected).find(":selected")[0].id).show("blind");

    $('.tiposervicio a').removeClass("selected");
    $('.proveedor').removeClass("selected");
    $(":checkbox").prop("checked", false);
    $('.proveedor').show("blind");
}

function saveConfigClick(event) {
    $.post("CuentasBancarias/SaveConfig", $("#configForm").serialize(), function (data) {
        $("#cuenta_" + data.CuentaBancariaId).find(".hddProvs").val(JSON.stringify(data.Proveedores));

    });
}