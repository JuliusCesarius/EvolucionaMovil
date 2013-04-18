﻿$(document).on("ready", function () {
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
            pageSize: $("#pageSize").val()
        });
    });

});

function rebindGrid(options) {
    $('<input />').attr('type', 'hidden').attr('name', 'pageSize').attr('value', options.pageSize).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'pageNumber').attr('value', options.pageNumber).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'searchString').attr('value', options.searchString).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'fechaInicio').attr('value', options.fechaInicio).appendTo('form');
    $('<input />').attr('type', 'hidden').attr('name', 'fechaFin').attr('value', options.fechaFin).appendTo('form');
    $("form").submit();
}

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
        data: $.parseJSON($("#hddData").val()),
        columns: columns,
        successFunction: edoCuentaLoaded,
        pageChangeFunction: pageChanged,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}

function edoCuentaLoaded(){
    $(".qtip").qtip({content:$(this).attr("alt")});
    $(".sgCell.saldo").each(function (i, item) {
            var clase = $(item).val().replace("$", "") > 0 ? "abono" : "cargo";
            $(item).addClass(clase);
    });
}

function pageChanged(event) {
    var target = event.currentTarget != undefined ? event.currentTarget : event.srcElement;
    var pageNumber = $(target).text() - 1;
    rebindGrid({ 
        pageSize: $("#pageSize").val(),
        pageNumber: pageNumber
    });
}