$(document).on("ready", function () {
    bindGrid();
    if ($("#pageSize").val() == "") {
        $("#pageSize").val(30);
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
            $(".Rechazado,.Aplicado,.Cancelado").parent().parent().hide("blind", {}, 1000);
        } else {
            $(".Rechazado,.Aplicado,.Cancelado").parent().parent().show("blind", {}, 1000);
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
        event.preventDefault();
        rebindGrid({
            fechaInicio: $("#fechaInicio").val(),
            fechaFin: $("#fechaFin").val(),
            pageSize: $("#pageSize").val(),
            onlyAplicados: $("#aplicadosOnly")[0].checked,
            searchString: $("#searchString").val()
        });
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

function rebindGrid(options) {
    if (options != undefined) {
        $('<input />').attr('type', 'hidden').attr('name', 'pageNumber').attr('value', options.pageNumber).appendTo('form');
    }
    $("form").submit();
}

function bindGrid(options) {
    var columns = [
         { name: 'FechaCreacion', displayName: 'Fecha', cssClass: 'fechacreacion', showToolTip: true },
         { name: 'Folio', displayName: 'Folio', cssClass: 'folio', customTemplate: '<a href="/PagoServicios/Details/{PagoId}">{Folio}</a>' },
         { name: 'NombreCliente', displayName: 'Cliente', cssClass: 'cliente', showToolTip: true },
         { name: 'PayCenterName', displayName: 'Paycenter', cssClass: 'paycenter', showToolTip: true },
         { name: 'Servicio', cssClass: 'servicio', showToolTip: true },
         { name: 'Status', cssClass: 'status', displayName: 'Estatus', customTemplate: '<span alt="{Comentarios}" class=" {Status} qtip">{Status}</span>' },
         { name: 'FechaVencimiento', displayName: 'Vencimiento', cssClass: 'fechavencimiento', showToolTip: true },
         { name: 'Monto', cssClass: 'monto' },
         ];
    if (options == undefined) {
        options = { pageSize: 30, pageNumber: 0 };
    }
    var pageSize = options.pageSize == undefined ? 30 : options.pageSize;
    var pageNumber = options.pageNumber==undefined?0:options.pageNumber;
    var searchString = options.searchString;
    var fechaInicio = options.fechaInicio;
    var fechaFin = options.fechaFin;
    var onlyAplicados = options.onlyAplicados;

    $("#grdPagoServicios").simpleGrid({
        data: $.parseJSON($("#hddData").val()),
        columns: columns,
        successFunction: edoCuentaLoaded,
        pageChangeFunction: pageChanged,
        selectedData: "PagoId",
        selectedURL: "PagoServicios/Details",
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin,
        onlyAplicados: onlyAplicados
    });

//    $("#grdPagoServicios").simpleGrid({
//        url: "/PagoServicios/GetPagoServicios",
//        columns: columns,
//        successFunction: edoCuentaLoaded,
//        pageSize: pageSize,
//        pageNumber: pageNumber,
//        searchString: searchString,
//        fechaInicio: fechaInicio,
//        fechaFin: fechaFin,
//        onlyAplicados: onlyAplicados
//    });
}

function edoCuentaLoaded(){
    $(".qtip").qtip({content:$(this).attr("alt")});
    $(".saldo").each(function (i, item) {
        if (i == 0) {
            $(item).addClass($(item).val().replace("$", "") < 0 ? "cargo" : "abono");
        }
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
        onlyAplicados: $("#aplicadosOnly")[0].checked,
        searchString: $("#searchString").val()
    });
}