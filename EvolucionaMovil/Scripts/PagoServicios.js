$(document).on("ready", function () {
    bindGrid();
    if ($("#pageSize").val() == "") {
        $("#pageSize").val(10);
    }
    $("#fechaInicio").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#fechaFin").datepicker({ "dateFormat": "dd/mm/yy" });
    if ($(".saldos span").html().indexOf("-") != -1) {
        $($(".saldos span")[0]).addClass("cargo");
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
        event.preventDefault();
        rebindGrid({
            fechaInicio: $("#fechaInicio").val(),
            fechaFin: $("#fechaFin").val(),
            pageSize: $("#pageSize").val(),
            onlyAplicados: $("#aplicadosOnly")[0].checked,
            searchString: $("#searchString").val()
        });
    });

});

function rebindGrid(options) {
    $('<input />').attr('type', 'hidden').attr('name', 'pageSize').attr('value', options.pageSize).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'pageNumber').attr('value', options.pageNumber).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'searchString').attr('value', options.searchString).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'fechaInicio').attr('value', options.fechaInicio).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'fechaFin').attr('value', options.fechaFin).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'onlyAplicados').attr('value', options.OnlyAplicados).appendTo('form');
    $("form").submit();
}

function bindGrid(options) {
    var columns = [
         { name: 'FechaCreacion', displayName: 'Fecha', cssClass: 'fechacreacion' },
         { name: 'Folio', displayName: 'Folio', cssClass: 'folio', customTemplate: '<a href="Details/{PagoId}">{Folio}</a>' },
         { name: 'NombreCliente', displayName: 'Cliente', cssClass: 'cliente'},
         { name: 'PayCenterName', displayName: 'Paycenter', cssClass: 'paycenter' },
         { name: 'Servicio', cssClass: 'servicio' },
         { name: 'Status', cssClass: 'status', displayName: 'Estatus', customTemplate: '<span alt="{Comentarios}" class=" {Status} qtip">{Status}</span>' },
         { name: 'FechaVencimiento', displayName: 'Vencimiento', cssClass: 'fechavencimiento' },
         { name: 'Monto', cssClass: 'monto' },
         ];
    if (options == undefined) {
        options = { pageSize: 10, pageNumber: 0 };
    }
    var pageSize = options.pageSize == undefined ? 10 : options.pageSize;
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
            $(item).addClass($(item).val().replace("$", "") > 0 ? "abono" : "cargo");
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