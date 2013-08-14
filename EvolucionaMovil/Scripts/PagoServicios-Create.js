(function ($) {
    $.validator.unobtrusive.parseDynamicContent = function (selector) {
        //use the normal unobstrusive.parse method
        $.validator.unobtrusive.parse(selector);

        //get the relevant form
        var form = $(selector).first().closest('form');

        //get the collections of unobstrusive validators, and jquery validators
        //and compare the two
        var unobtrusiveValidation = form.data('unobtrusiveValidation');
        var validator = form.validate();

        $.each(unobtrusiveValidation.options.rules, function (elname, elrules) {
            if (validator.settings.rules[elname] == undefined) {
                var args = {};
                $.extend(args, elrules);
                args.messages = unobtrusiveValidation.options.messages[elname];
                //edit:use quoted strings for the name selector
                $("[name='" + elname + "']").rules("add", args);
            } else {
                $.each(elrules, function (rulename, data) {
                    if (validator.settings.rules[elname][rulename] == undefined) {
                        var args = {};
                        args[rulename] = data;
                        args.messages = unobtrusiveValidation.options.messages[elname][rulename];
                        //edit:use quoted strings for the name selector
                        $("[name='" + elname + "']").rules("add", args);
                    }
                });
            }
        });
    }
})($);

$(document).on("ready", function () {
    $("#FechaVencimiento").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#FechaVencimiento").val();
    $("#ServicioId").change(function () { getDetalleServicio(); });
    $("#ServicioId").trigger("change");
    $("#ImporteString").on("keyup", function () {
        $("#Importe").val($("#ImporteString").val().replace(",", ""));
        if ($("#hddSaldoDisponible")[0].textContent != "") {
            var SaldoDisp = parseFloat($("#hddSaldoDisponible")[0].value);
            var Importe = parseFloat($("#Importe")[0].value);

            var MontoFinanciamiento = parseFloat($("#hddMaximoFinanciar")[0].value);
            var Comision = parseFloat($("#hddComision")[0].value);
            var ContEventos = parseInt($("#hddEventos")[0].value);
            var SaldoFinal = 0;
            if (Comision > 0 && ContEventos <= 0) {
                if (ContEventos <= 0) {
                    SaldoFinal = SaldoDisp - (Importe + Comision);
                }
            }
            else {
                SaldoFinal = parseFloat(SaldoDisp - Importe);
            }
            //var SaldoFinal = parseFloat(SaldoDisp - Importe);

            $("#saldoFinal")[0].value = SaldoFinal.toString();
            $("#saldoFinal")[0].textContent = SaldoFinal.toString();

            if (SaldoFinal < 0) {

                $("#saldoFinal").removeClass("saldos");
                $("#saldoFinal").addClass("cargo");

                if ((MontoFinanciamiento + SaldoFinal) < 0) {
                    $("#btnCreate").attr('disabled', '-1');
                    //document.getElementById("btnCreate").setAttribute('disabled', true);
                    $("#divMensaje").show();
                    $("#Mensaje")[0].textContent = "No cuenta con saldo disponible para realizar el pago y no está autorizado para realizar un finaciamiento";
                }
                else {
                    $("#divMensaje").show();
                    $("#Mensaje")[0].textContent = "No cuenta con saldo disponible para realizar el pago pero tiene finaciamiento de $:" + MontoFinanciamiento.toString();
                }
            }
            else {
                $('#btnCreate').removeAttr('disabled');
                $("#saldoFinal").removeClass("cargo");
                $("#saldoFinal").addClass("saldos");
                $("#divMensaje").css("display", "none");
            }
        }
    });
    setValidation("Importe", 4);
    getURLParameter();
    $("#PayCenterName").autocomplete({
        source: "/PayCenters/FindPayCenter",
        select: function (event, ui) {
            var label = ui.item.label;
            var v = ui.item.value;
            $('#hddPayCenterId').val(v);
            $.getJSON("/PayCenters/GetSaldosPagoServicio?PayCenterId=" + v, function (data) {
                $("#saldoDisponible").html(data.SaldoDisponible);
                $("#eventoDisponible").html(data.EventosDisponibles);
                $("#hddSaldoDisponible")[0].value = data.SaldoDisponible;
                $("#hddSaldoDisponible").html(data.SaldoDisponible);
                $("#hddEventos")[0].value = data.EventosDisponibles;
                if ($("#ImporteString")[0].value != "") {
                    $("#ImporteString")[0].value = 0;
                }
                $('#btnCreate').removeAttr('disabled');
                $("#saldoFinal")[0].textContent = "";
                $("#Mensaje")[0].textContent = "";
                //Todo: Aqui se carga de nuevo el finaciamiento y los eventos del paycenter.
                //$("#hddComision").html(data.SaldoDisponible);
                //$("#hddMaximoFinanciar").html(data.EventosDisponibles);
                //Eventos
                var EventosDisp = parseInt($("#hddEventos")[0].value);
                if (EventosDisp > 0) {
                    $("#eventoFinal")[0].textContent = EventosDisp - 1;
                }

                $(".saldos").show();
            });
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
});

function getDetalleServicio() {
    var servicioId = $("#ServicioId").val();
    if (servicioId > 0) {
        $.getJSON("/PagoServicios/getDetalleServicio?servicioId=" + servicioId, function (data) {
            var divCampos = $("#camposAdicionales");
            divCampos.html("");
            $(data).each(function (i, item) {
                var divEditor = ("#div" + this.DetalleServicioId);
                var nombre = this.Campo.replace(/ /g, '_').replace(/\./g, '_');
                divCampos.append("<div class='editor-label'><label for='" + nombre + "'>" + this.Campo + "</label></div><div id='div" + this.DetalleServicioId + "'  class='editor-field'></div>");
                $(divEditor).append($('<input/>').attr('id', nombre).attr('name', nombre).attr('type', 'Text').attr('data-val', true).attr('data-val-required', "El campo es requerido").addClass('text-box single-line'));
                $(divEditor).append($('<span/>').attr('data-valmsg-for', nombre).attr('data-valmsg-replace', true).addClass('field-validation-error'));
                setValidation(nombre, this.Tipo);
            });
            $("label").inFieldLabels();
            $.validator.unobtrusive.parseDynamicContent($(divCampos));
        });
    }
}

function getURLParameter() {
    var id = decodeURI((RegExp('.*?(\\d+)').exec(location.pathname) || [, null])[1]);
    if (id > 0) {
        $("#ServicioId").val(id);
        $("#ServicioId").trigger("change")
    }
}

function setValidation(nombre, tipo) {
    nombre = "#" + nombre;
    switch (tipo) { //Activar todo cuando resuelva lo del placeholder
        case 0: //Cadena
            break;
        case 1: //Entero
            $(nombre).attr("data-val-number", "El campo solo acepta números");
            break;
        case 2: case 4: //Flotante, Dinero
            $(nombre).attr('data-val-regex-pattern', '^[0-9/\.]*$').attr('data-val-regex', 'Solo decimales');
            break;
        case 3: //Fecha
            $(nombre).datepicker({ "dateFormat": "dd/mm/yy" });
            break;
    }
    $(nombre).attr('value', '');
}

