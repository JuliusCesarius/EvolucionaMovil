$(document).on("ready", function () {
    bindGrid();
    if ($("#pageSize").val() == "") {
        $("#pageSize").val(10);
    }
    $("#fechaInicio").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#fechaFin").datepicker({ "dateFormat": "dd/mm/yy" });
    if ($(".saldos span").length > 0) {
        if ($(".saldos span").html().indexOf("-") != -1) {
            $($(".saldos span")[0]).addClass("cargo");
        }
    }
    $("#aplicadosOnly").on("click", function () {
        if ($("#aplicadosOnly").prop("checked")) {
            $(".Rechazado,.Procesando,.Cancelado").parent().parent().hide("blind", {}, 1000);
        } else {
            $(".Rechazado,.Procesando,.Cancelado").parent().parent().show("blind", {}, 1000);
        }
    });
    $("#pageSize").on("keyup", function (event) {
        var code = event.which; // recommended to use e.which, it's normalized across browsers
        if (code == 13) {
            event.preventDefault();
            rebindGrid({ pageSize: $("#pageSize").val() });
        }
    });
    $("#Actualizar").on("click", function (event) {

        if (CompararDosFechas($("#fechaInicio").val(), $("#fechaFin").val())) {
            alert("La Fecha de inicio no puede ser mayo a la fecha final de búsqueda.")
        }
        else {
            event.preventDefault();
            rebindGrid({ pageSize: $("#pageSize").val() });
        }
    });

    $("#PayCenterName").on("keyup", function (event) {
        var code = event.which;
        if (code == 13) {
            event.preventDefault();
            rebindGrid();
        } else {
            if ($(this).val() == "") {
                event.preventDefault();
                $('#hddPayCenterId').val("");
            }
        }
    });

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

});

function CompararDosFechas(fechainicio, fechafin) {

    if (fechainicio != "" || fechafin != "") {
        if (fechainicio == "")
            fechainicio = $.format.date(new Date().toString(), "dd/mm/yyyy").toString();
            //$.datepicker.formatDate('dd/mm/yy', new Date()).toString();
        if (fechafin == "") {
            //$.format.date("2009-12-18 10:54:50.546", "dd/MM/yyyy");
            fechafin = $.format.date(new Date().toString(), "dd/mm/yyyy").toString();
            $("#fechaFin").val(fechafin);
        }
    }
    var dt1 = fechainicio.split('/');
    var dt2 = fechafin.split('/');
    var date1 = new Date(dt1[2], dt1[1]-1, dt1[0]);
    var date2 = new Date(dt2[2], dt2[1]-1, dt2[0]);

    if ((date1 > date2)) { return true; } else { return false; }
}

function rebindGrid(options) {
    if (options != undefined) {
        $('<input />').attr('type', 'hidden').attr('name', 'pageNumber').attr('value', options.pageNumber).appendTo('form');
    }
    $("form").submit();
}

function bindGrid(options) {
    var columns = [
         { name: 'FechaCreacion', displayName: 'Fecha', cssClass: 'fechacreacion', showToolTip: true },
         { name: 'Clave', displayName: 'Clave', cssClass: 'clave' },
         { name: 'Concepto', cssClass: 'concepto', showToolTip: true },
         { name: 'Status', cssClass: 'status', displayName: 'Estatus', customTemplate: '<span alt="{Comentarios}" class=" {StatusString} qtip">{StatusString}</span>' },
         { name: 'Abono', cssClass: 'abono' },
         { name: 'Cargo', cssClass: 'cargo' },
         { name: 'Saldo', cssClass: "saldo" }
         ];
    if (options == undefined) {
        options = { pageSize: 20, pageNumber: 0 };
    }
    var pageSize = options.pageSize;
    var pageNumber = options.pageNumber;
    var searchString = options.searchString;
    var fechaInicio = options.fechaInicio;
    var fechaFin = options.fechaFin;
    var onlyAplicados = options.onlyAplicados;
    $("#grdEstadoDeCuenta").simpleGrid({
        data: $.parseJSON($("#hddData").val()),
        columns: columns,
        successFunction: edoCuentaLoaded,
        selectedURL: "EstadoDeCuenta/Details",
        selectedData: "MovimientoId",
        selectedFunction: grdRowSelected,
        successFunction: edoCuentaLoaded,
        pageChangeFunction: pageChanged,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin,
         onlyAplicados:onlyAplicados
    });
}

function grdRowSelected(item) {
    //alert(item);
}

function edoCuentaLoaded(){
    $(".qtip").qtip({content:$(this).attr("alt")});
    $(".sgCell.saldo").each(function (i, item) {
            var clase = $(item).val().replace("$", "") < 0 ? "cargo" : "abono";
            $(item).addClass(clase);
    });
}

function pageChanged(event) {
    var target = event.currentTarget != undefined ? event.currentTarget : event.srcElement;
    var pageNumber = $(target).text() - 1;
    rebindGrid({ 
        pageSize: $("#pageSize").val(),
        pageNumber: pageNumber,
        fechaInicio: $("#fechaInicio").val(),
        fechaFin: $("#fechaFin").val(),
        onlyAplicados: $("#aplicadosOnly")[0].checked
    });
}