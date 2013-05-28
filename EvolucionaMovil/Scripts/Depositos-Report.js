$(document).on("ready", function () {
    $("#FechaPago").datepicker({ "dateFormat": "dd/mm/yy", maxDate: '0' });
    $("#ProveedorId").on("change", proveedorChanged);
    if (!$("[name='TipoCuenta']").is(':checked')) {
        $($("[name='TipoCuenta']")[0]).prop('checked', true);
    }
    $("[name='TipoCuenta']").on("click", function () {
        createProvedoresDropdown($(this).val());
    });
    createProvedoresDropdown(0);
    $("#PayCenterName").autocomplete({
        source: "PayCenters/FindPayCenter",
        select: function (event, ui) {
            var label = ui.item.label;
            var v = ui.item.value;
            $('#hddPayCenterId').val(v);
            this.value = label;
            return false;
        },
        focus: function (event, ui) {
            event.preventDefault();
            $(this).val(ui.item.label);
        },
        open: function () {
            $(this).autocomplete('widget').css('z-index', 100);
            return false;
        }
    });
    $("#btnSave").on("click", Save);
    $("#CuentaBancariaId").on("change", cuentaBancariaChanged);

    // $("#CuentaId").on("change", function () { $("#hddTipoCuenta").val($("#CuentaId").find(":selected").text()); });
    $("#CuentaId").on("change", CuentaDeposito);
    CuentaDeposito();
    $('#BancoId').on("change", BancoIdChanged);
    if ($("#BancoId").val() > 0) {
        $('#BancoId').trigger("change");
        $("#CuentaBancariaId").val($('#hddCuentaBancariaId').val());
    }
    $("#MontoString").on("keyup", function () {
        $("#Monto").val($("#MontoString").val().replace(",", ""));
    });
    $(".cuentadeposito").each(function (i, item) {
        $("form").validate().settings.rules[$(item).find("input[type=text]").attr("name")] = { "number": true };
    });
    $(".cuentadeposito input[type=text]").on("blur", CuentasDepositoChanged);
});

//$("#Rechazar").css("display", "inline");

function proveedorChanged(event) {
    if ($(event.currentTarget)[0].selectedOptions.length) {
        var selected = $($(event.currentTarget)[0].selectedOptions[0]).data();
        createBancosDropdown(selected.Bancos);
    } else {
        $("#BancoId").html("");
    }
}

function cuentaBancariaChanged() {
    if ($("#CuentaBancariaId").find(":selected").length > 0) {
        var cuenta = $($("#CuentaBancariaId").find(":selected")[0]).data();
        if (cuenta.Detalles != null && cuenta.Detalles != "") {
            $("#details").html("<label for='det1' ><span class='fwb'>Detalles:</span> " + cuenta.Detalles + "</label>");
            $("#details").show("blind", {}, 500);
        } else {
            $("#details").hide("blind", {}, 500);
        }
        if (cuenta.Comprobante) {
            $("#Imagen").show("blind", {}, 500);
        } else {
            $("#Imagen").hide("blind", {}, 500);
        }
    } else {
        $("#Imagen").hide("blind", {}, 500);
        $("#details").hide("blind", {}, 500);
    }
    $("#hddCuentaBancaria").val($("#CuentaBancariaId").find(":selected").text());
}

function createBancosDropdown(bancos) {
    $("#BancoId").html("");
    $.each(bancos, function (i, item) {
        $("#BancoId").append($('<option/>', {
            value: item.BancoId,
            text: item.Nombre
        }).data(item));
    });
    $("#BancoId").trigger("change");
}

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
function createProvedoresDropdown(TipoCuenta) {
    $("#ProveedorId").html("");
    var proveedores = $.parseJSON($("#hddProveedores").val());
    $.each(proveedores, function (i, item) {
        if (item.TipoCuenta == TipoCuenta) {
            $("#ProveedorId").append($('<option/>', {
                value: item.ProveedorId,
                text: item.Nombre
            }).data(item));
        }
        //if (i == 0) { cdefault = item.CuentaBancariaId; }
    });
    $("#ProveedorId").trigger("change");
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
            var cuentas = $("#BancoId").find(":selected").data().CuentasBancarias;
            $.each(cuentas, function (i, item) {
                if (item.BancoId == bancoId) {
                    CuentaBancariaId.append($('<option/>', {
                        value: item.CuentaId,
                        text: item.Caption
                    }).data(item));
                }
                CuentaBancariaId.trigger("change");
                if (i == 0) { cdefault = item.CuentaBancariaId; }
            });


            if (cdefault > 0) {
                $("#CuentaBancariaId option[value=" + cdefault + "]").attr("selected", true);
                $("#hddCuentaBancaria").val($("#CuentaBancariaId").find(":selected").text());
            }


        }
    }
}

