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
                $("#saldoActual").html(data.SaldoDisponible);
                $("#eventoActual").html(data.EventosDisponibles);
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

