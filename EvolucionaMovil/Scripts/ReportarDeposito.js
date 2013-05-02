﻿$(document).on("ready", function () {
    $("#FechaPago").datepicker({ "dateFormat": "dd/mm/yy", maxDate: '0' });
    $("#btnSave").on("click", Save);
    $("#CuentaBancariaId").on("change", function () { $("#hddCuentaBancaria").val($("#CuentaBancariaId").find(":selected").text()); });

    // $("#CuentaId").on("change", function () { $("#hddTipoCuenta").val($("#CuentaId").find(":selected").text()); });
    $("#CuentaId").on("change", CuentaDeposito);
    CuentaDeposito();
    $('#BancoId').on("change", BancoIdChanged);
    if ($("#BancoId").val() > 0) {
        $('#BancoId').trigger("change");
        $("#CuentaBancariaId").val($('#hddCuentaBancariaId').val());
    }

    $(".cuentadeposito").each(function (i, item) {
        $("form").validate().settings.rules[$(item).find("input[type=text]").attr("name")] = { "number": true };
    });
    $(".cuentadeposito input[type=text]").on("blur", CuentasDepositoChanged);
});

//$("#Rechazar").css("display", "inline");

function CuentaDeposito() {
    $("#hddTipoCuenta").val($("#CuentaId").find(":selected").text());
    var cuentaDepositoId = $("#CuentaId").val();
    if (cuentaDepositoId != null && cuentaDepositoId > 0) {
        $("#Imagen").css("display", "inline");
    }
    else {
        $("#Imagen").css("display", "none");
    }
}
function CuentasDepositoChanged(event) {
    var monto = parseFloat($("#Monto").val());
    if (isNaN(monto)) {
        alert("Primero debe especificar un monto");
        $(this).val("");
    }
    var cuentas = 0;
    $(".cuentadeposito input[type=text]").each(function (i, item) {
        if ($(item).val() > 0) {
            cuentas += parseFloat($(item).val());
        }
    });
    if (monto != cuentas && cuentas > 0 && event == null) {
        if (!$(".cuentadeposito input[type=text]").hasClass("input-validation-error")) {
            alert("La suma de las cuentas debe sumar el monto especificado");
            $(".cuentadeposito input[type=text]").addClass("input-validation-error");
        }
        return false;
    }
    if (monto < cuentas) {
        if (!$(".cuentadeposito input[type=text]").hasClass("input-validation-error")) {
            alert("La suma de las cuentas no puede ser mayor al monto especificado");
            $(".cuentadeposito input[type=text]").addClass("input-validation-error");
        }
        return false;
    }
    $(".cuentadeposito input[type=text]").removeClass("input-validation-error");
    return true;
}
function Save(event) {
    if (!$("#acceptTerms").prop("checked")) {
        alert("Primero debe de aceptar las condiciones");
        event.preventDefault();
    }
    if (!CuentasDepositoChanged(null)) {
        event.preventDefault();
    }
}
function BancoIdChanged() {
    var bancoId = $(this).val();
    $("#hddBanco").val($("#BancoId").find(":selected").text());
    if (bancoId != null) {
        var CuentaBancariaId = $("#CuentaBancariaId");
        CuentaBancariaId.html("");
        if (bancoId == 0) {
            CuentaBancariaId.html("<option/>");
            CuentaBancariaId.val(0);
        } else {
        var cdefault = 0;
            var cuentas = $.parseJSON($("#hddCuentas").val());
            $.each(cuentas, function (i, item) {
                if (item.BancoId == bancoId) {
                    CuentaBancariaId.append($('<option/>', {
                        value: item.CuentaBancariaId,
                        text: item.NumeroCuenta + " - " + item.Titular
                    }));
                }
                if (i == 0) { cdefault = item.CuentaBancariaId; }
            });


            if (cdefault > 0) {
                $("#CuentaBancariaId option[value=" + cdefault + "]").attr("selected", true);
                $("#hddCuentaBancaria").val($("#CuentaBancariaId").find(":selected").text());
            }


        }
    }
}

