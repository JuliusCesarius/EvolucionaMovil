$(document).on("ready", function () {
    bindGrid();
    $("#pageSize").val(20);
    $("#fechaInicio").datepicker({"dateFormat":"dd/mm/yy"});
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
            bindGrid({ pageSize: $("#pageSize").val() });
        }
    });
    $("#Actualizar").on("click", function (event) {        
        event.preventDefault();
        bindGrid({
            fechaInicio: $("#fechaInicio").val(),
            fechaFin: $("#fechaFin").val(),
            pageSize: $("#pageSize").val()
        });
    });

});

function bindGrid(options) {
    var columns = [
         { name: 'FechaCreacion', displayName: 'Fecha', cssClass: 'fechacreacion' },
         { name: 'Clave', displayName: 'Clave', cssClass: 'clave' },
         { name: 'Concepto', cssClass: 'concepto' },
         { name: 'Status', cssClass: 'status', displayName: 'Estatus', customTemplate: '<span alt="{Motivo}" class=" {Status} qtip">{Status}</span>' },
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
    $("#grdEstadoDeCuenta").simpleGrid({
        url: "/EstadoDeCuenta/GetEstadoCuenta",
        columns: columns,
        successFunction: edoCuentaLoaded,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}

function edoCuentaLoaded(){
    $(".qtip").qtip({content:$(this).attr("alt")});
    $(".saldo").each(function (i, item) {
        if (i == 0) {
            $(item).addClass($(item).val().replace("$", "") > 0 ? "abono" : "cargo");
        }
    });    
}