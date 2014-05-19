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
        if ($("#hddSaldoDisponible").val() != "") {
            var SaldoDisp = parseFloat($("#hddSaldoDisponible")[0].value);
            var Importe = parseFloat($("#Importe")[0].value);

            var MontoFinanciamiento = parseFloat($("#hddMaximoFinanciar").val());
            var Comision = parseFloat($("#hddComision")[0].value);
            var ComisionACobrar = 0;
            var ContEventos = parseInt($("#hddEventos")[0].value);
            var SaldoFinal = 0;
            if (Comision > 0 && ContEventos <= 0) {
                if (ContEventos <= 0) {
                    SaldoFinal = SaldoDisp - (Importe + Comision);
                    ComisionACobrar = Comision;
                }
            }
            else {
                SaldoFinal = parseFloat(SaldoDisp - Importe);
            }
            //var SaldoFinal = parseFloat(SaldoDisp - Importe);
            if (ContEventos > 0) {
                $("#detail-comision").html("NA");
            } else {
                $("#detail-comision").html(Comision * -1);
                $("#detail-comision").formatCurrency();
            }

            $("#detail-importe").html(Importe * -1);
            $("#detail-importe").formatCurrency();

            $("#saldoFinal")[0].value = SaldoFinal.toString();
            $("#saldoFinal")[0].textContent = SaldoFinal.toString();
            $("#saldoFinal").formatCurrency();

            if (SaldoFinal < 0) {

                $("#saldoFinal").removeClass("saldos");
                $("#saldoFinal").addClass("cargo");

                if (((SaldoDisp+MontoFinanciamiento) - (Importe +ComisionACobrar) < 0)) {
                    $("#btnCreate").hide("blind");
                    $("#divFinan").show();
                    $("#Mensaje").html("No cuenta con saldo disponible para realizar el pago y no está autorizado para realizar un finaciamiento");
                }
                else {
                    $("#divFinan").show();
                    $("#btnCreate").show("blind");
                    $("#Mensaje").html("No cuenta con saldo disponible para realizar el pago. Sin embargo, cuenta con un financiamiento por parte de la empresa de ");
                    var $montoFinan = $("<span class='montoFinan fxl fwb'><span>");
                    $montoFinan.html(MontoFinanciamiento)
                    $montoFinan.formatCurrency();
                    $("#Mensaje").append($montoFinan);
                }
            }
            else {
                $("#btnCreate").show("blind");
                $('#btnCreate').removeAttr('disabled');
                $("#saldoFinal").removeClass("cargo");
                $("#saldoFinal").addClass("saldos");
                $("#divFinan").css("display", "none");
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
            $.getJSON("/PayCenters/GetComisionFinanciamiento/" + v, function (data) {
                $("#hddComision").val(data.Comision);
                var financiamentoFinal = $("#hddSaldoDisponible").val() < 0 ? data.Financiamiento + parseFloat($("#hddSaldoDisponible").val()) : data.Financiamiento;
                $("#hddMaximoFinanciar").val(financiamentoFinal);
            });
            $.getJSON("/PayCenters/GetSaldosPagoServicio?PayCenterId=" + v, function (data) {
                $("#saldoDisponible").html(data.SaldoDisponible);
                $("#saldoDisponible").formatCurrency();
                $("#detail-saldo").html(data.SaldoDisponible);
                $("#detail-saldo").formatCurrency();
                $("#eventoDisponible").html(data.EventosDisponibles);
                $("#hddSaldoDisponible").val(data.SaldoDisponible);
                $("#hddEventos")[0].value = data.EventosDisponibles;
                if ($("#ImporteString")[0].value != "") {
                    $("#ImporteString")[0].value = 0;
                }
                $("#ImporteString").formatCurrency();
                if ($("#detail-importe")[0].value != "") {
                    $("#detail-importe")[0].value = 0;
                }
                $("#detail-importe").formatCurrency();
                $('#btnCreate').removeAttr('disabled');
                $("#saldoFinal").html(data.SaldoDisponible);
                $("#saldoFinal").formatCurrency();

                $("#Mensaje")[0].textContent = "";
                //Todo: Aqui se carga de nuevo el finaciamiento y los eventos del paycenter.
                //$("#hddComision").html(data.SaldoDisponible);
                //$("#hddMaximoFinanciar").html(data.EventosDisponibles);
                //Eventos
                var EventosDisp = parseInt($("#hddEventos")[0].value);
                $("#eventoFinal").html(EventosDisp > 0 ? EventosDisp - 1 : 0);
                if (EventosDisp > 0) {
                    $("#detail-eventosFinales").show("blind");
                    $("#detail-comision").html("NA");
                } else {
                    $("#detail-eventosFinales").hide("blind");
                    var Comision = parseFloat($("#hddComision").val());
                    $("#detail-comision").html(Comision);
                    $("#detail-comision").formatCurrency();
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
                var nombre = this.DetalleServicioId;
                divCampos.append("<div class='editor-label'><label for='" + nombre + "'>" + this.Campo + "</label></div><div id='div" + this.DetalleServicioId + "'  class='editor-field'></div>");
                $(divEditor).append($('<input/>').attr('id', nombre).attr('name', nombre).attr('type', 'Text').attr('data-val', true).attr('data-val-required', "El campo es requerido").addClass('text-box single-line'));
                $(divEditor).append($('<span/>').attr('data-valmsg-for', nombre).attr('data-valmsg-replace', true).addClass('field-validation-error'));
                setValidation(nombre, this.Tipo);

                var EsRefencia = this.EsReferencia;
                if (EsRefencia) 
                { //typeof EsRefencia === 'string' &&
                    var CampoReferencia = "<div class='editor-field'><input class='text-box single-line ui-autocomplete-input' id='IdRefConfirm' name='Referencia' type='text' value='' autocomplete='off'>" +
                    "<span id='msgRefConfirm' class='field-validation-error' generate='true' data-valmsg-for='IdRefConfirm' data-valmsg-replace='true'>" +
                    "<span id='msgRefConfirm1' for='IdRefConfirm' generated='true' class=''>El campo Confirmar Referencia es requerido.</span></span>"+
                    "<input type='hidden' id='hdReferencia' name='hdReferencia' value='" + nombre + "' /></div>";

                    divCampos.append("<div class='editor-label'><label id='refCapConfirmacion' for='IdRefConfirm'>Confirmar " + this.Campo + "</label> </div>" + CampoReferencia);                   
                }

            });
            $("label").inFieldLabels();
            $.validator.unobtrusive.parseDynamicContent($(divCampos));
            if ($("#hdReferencia"))
            {
                if ($("#" + $("#hdReferencia").val())[0])
                {

                    $("#" + $("#hdReferencia").val()).on("blur", ReferenciaChanged);
                    $("#" + $("#hdReferencia").val()).on("focus", ChangetoText);
                     $("#IdRefConfirm").on("blur", ValidateRefencia);
                     $("#IdRefConfirm").on("focus", ChangetoPw);
                }
            }

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

function ValidateRefencia() {
    var refvalid = true;
    var nombreRef = $("#hdReferencia").val();
    if (!($("#" + nombreRef).val().trim()) == "") {

        if ($("#IdRefConfirm").val().trim() != $("#" + nombreRef).val().trim()) {
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

    if (refvalid) {
        $("#btnCreate").show();
    }
    else {

        $("#btnCreate").hide();
    }
    return refvalid;
}


function ReferenciaChanged(event) {
    var nombreRef = $("#hdReferencia").val();
    changeType("#" + nombreRef, "password");
}

function ChangetoText(event) {
    var nombreRef = $("#hdReferencia").val();
    changeType("#" + nombreRef, "text");
}

function ChangetoPw(event) {
    var nombreRef = $("#hdReferencia").val();
    if ($("#" + nombreRef)[0].value.trim() != "" && $("#" + nombreRef).prop('type') == "text") {
        changeType("#" + nombreRef, "password");
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
