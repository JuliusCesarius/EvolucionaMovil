$(document).on("ready", function () {
    $("#FechaPago").datepicker({ "dateFormat": "dd/mm/yy", maxDate: '0' });
   /* $("#IdRefConfirm").attr("autocomplete", "off");
    $("#IdRefConfirm")[0].value = $("#Referencia")[0].value;
    ValidateRefencia();
    $("#Referencia").on("blur", ReferenciaChanged);
    $("#Referencia").on("focus", ChangetoText);
    $("#IdRefConfirm").on("blur", ValidateRefencia);
    $("#IdRefConfirm").on("focus", ChangetoPw);*/
    $("#ProveedorId").on("change", proveedorChanged);
    if (!$("[name='TipoCuenta']").is(':checked')) {
        $($("[name='TipoCuenta']")[0]).prop('checked', true);
    }
    $("[name='TipoCuenta']").on("click", function () {
        createProvedoresDropdown($(this).val());
    });
    createProvedoresDropdown(0);
    $("#PayCenterName").autocomplete({
        source: "/PayCenters/FindPayCenter",
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
    var target = (event.currentTarget) ? event.currentTarget : event.srcElement;
    if ($(target)[0].selectedOptions.length) {
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
    var found = false;
    $.each($.parseJSON($("#refCaptions").val()), function (index, item) {
        if ($("#BancoId").val() == item.BancoId && parseInt($("#CuentaBancariaId").val()) == item.CuentaId) {
            $("#refCap").html(item.ReferenceCaption);
            found = true;
            return false;
        } else {
            if ($("#BancoId").val() == item.BancoId && item.CuentaId == 0) {
                $("#refCap").html(item.ReferenceCaption);
                found = true;
            }
        }
    });
    if (!found) {
        $("#refCap").html("Referencia");
    }
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

    if (!ValidateRefencia()) {
        event.preventDefault();
    }
    else {
        ChangetoText();
    }

}
function createProvedoresDropdown(TipoCuenta) {
    $("#ProveedorId").html("");
    var proveedores = $.parseJSON($("#hddProveedores").val());
    if (proveedores != null) {
        $.each(proveedores, function (i, item) {
            if (item.TipoCuenta == TipoCuenta) {
                $("#ProveedorId").append($('<option/>', {
                    value: item.ProveedorId,
                    text: item.Nombre
                }).data(item));
            }
            //if (i == 0) { cdefault = item.CuentaBancariaId; }
        });
    }
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

/*
function ValidateRefencia() {
    var refvalid = true;
    if (!($("#Referencia").val().trim()) == "") {

        if ($("#IdRefConfirm").val().trim() != $("#Referencia").val().trim()) {
            $("#msgRefConfirm").show("slow");
            $("#msgRefConfirm").removeClass("field-validation-valid");
            $("#msgRefConfirm").addClass("field-validation-error");
           // $("#msgRefConfirm").css("display", "block");
            $("#msgRefConfirm")[0].textContent = "La Referencia no es igual con la referencia de confirmación."
            $("#IdRefConfirm")[0].value = "";
            //$("#IdRefConfirm").focus();
            refvalid = false;
        }
        else {
             $("#msgRefConfirm").hide();
        }

     }
      else { $("#msgRefConfirm").hide(); }

    return refvalid;
}

function ReferenciaChanged(event) {
   changeType("#Referencia", "password");
}

function ChangetoText(event) {
    changeType("#Referencia", "text");
}

function ChangetoPw(event) {
    if ($("#Referencia")[0].value.trim() != "" && $("#Referencia").prop('type') == "text") {
        changeType("#Referencia", "password");
    }
}

function changeType(x, type) {
    x = $(x);
    if (x.prop('type') == type)
        return x; //That was easy.
    try {
        return x.prop('type', type); //Stupid IE security will not allow this
    } catch (e) {
        //Try re-creating the element (yep... this sucks)
        //jQuery has no html() method for the element, so we have to put into a div first
        var html = $("<div>").append(x.clone()).html();
        var regex = /type=(\")?([^\"\s]+)(\")?/; //matches type=text or type="text"
        //If no match, we add the type attribute to the end; otherwise, we replace
        var tmp = $(html.match(regex) == null ?
            html.replace(">", ' type="' + type + '">') :
            html.replace(regex, 'type="' + type + '"'));
        //Copy data from old element
        tmp.data('type', x.data('type'));
        var events = x.data('events');
        var cb = function (events) {
            return function () {
                //Bind all prior events
                for (i in events) {
                    var y = events[i];
                    for (j in y)
                        tmp.bind(i, y[j].handler);
                }
            }
        } (events);
        x.replaceWith(tmp);
        setTimeout(cb, 10); //Wait a bit to call function
        return tmp;
    }
}
*/