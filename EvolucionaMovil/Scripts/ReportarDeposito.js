$(document).on("ready", function () {
    $("#Fecha").datepicker({ "dateFormat": "dd/mm/yy", maxDate: '0' });
    $("#btnSave").on("click", Save);
    $('#BancoId').on("change", BancoIdChanged);
    if ($("#BancoId").val() > 0) {
        $('#BancoId').trigger("change");
        $("#CuentaId").val($('#hddCuentaId').val());
    }
    $(".cuentadeposito").each(function (i, item) {
        $("form").validate().settings.rules[$(item).find("input[type=text]").attr("name")] = { "number": true };
    });
    $(".cuentadeposito input[type=text]").on("blur", CuentasDepositoChanged);
});
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
        if (bancoId != null) {
            var CuentaId = $("#CuentaId");
            CuentaId.html("");
            var cuentas = $.parseJSON($("#hddCuentas").val());
            $.each(cuentas, function (i, item) {
                if (item.BancoId == bancoId) {
                    CuentaId.append($('<option/>', {
                        value: item.CuentaId,
                        text: item.NumeroCuenta + " - " + item.Titular
                    }));
                }
            });
        }
}