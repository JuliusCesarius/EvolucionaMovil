$(document).on("ready", function () {
    bindGrid();
    if ($("#pageSize").val() == "") {
        $("#pageSize").val(10);
    }
    $("#fechaInicio").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#fechaFin").datepicker({ "dateFormat": "dd/mm/yy" });

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
        rebindGrid({pageSize: $("#pageSize").val()});
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

    $("#ProveedorName").on("keyup", function (event) {
        var code = event.which;
        if (code == 13) {
            event.preventDefault();
            rebindGrid();
        } else {
            if ($(this).val() == "") {
                event.preventDefault();
                $('#hddProveedorId').val("");
            }
        }
    });

    $("#ProveedorName").autocomplete({
        source: "Proveedores/FindProveedores",
        select: function (event, ui) {
            var label = ui.item.label;
            var v = ui.item.value;
            $('#hddProveedorId').val(v);
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
         { name: 'FechaCreacion', displayName: 'Fecha Captura', cssClass: 'fechacreacion', showToolTip: true },
         { name: 'FechaPago', displayName: 'Fecha Pago', cssClass: 'fechapago', showToolTip: true },
         { name: 'Referencia', displayName: 'Referencia', cssClass: 'referencia', showToolTip: true },
          { name: 'ProveedorName', displayName: 'Proveedor', cssClass: 'proveedor', showToolTip: true },
         { name: 'PayCenter', displayName: 'PayCenter', cssClass: 'PayCenter', showToolTip: true },
         { name: 'Banco', displayName: 'Banco/Cuenta', cssClass: 'banco', showToolTip: true, customTemplate: '{Banco} / {CuentaBancaria}' },
         { name: 'TipoCuenta', displayName: 'Cuenta Destino', cssClass: 'tipocuenta' },
         { name: 'StatusString', cssClass: 'status', displayName: 'Estatus', customTemplate: '<span alt="{Comentarios}" class=" {StatusString} qtip">{StatusString}</span>' },
         { name: 'Monto', displayName: 'Monto', cssClass: 'monto' }   
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
    $("#grdDepositos").simpleGrid({
        data: $.parseJSON($("#hddData").val()),
        columns: columns,
        selectedData: "AbonoId",
        selectedURL: "Depositos/Details",
        selectedFunction: grdRowSelected,
        successFunction: depositosLoaded,
        pageChangeFunction: pageChanged,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}

function grdRowSelected(item) {
    //alert(item);
}

function depositosLoaded() {
    $(".qtip").qtip({ content: $(this).attr("alt"), api: { beforeShow: comentariosBeforeShow, onRender: comentariosBeforeShow} });
    $(".sgCell.saldo").each(function (i, item) {
        var clase = $(item).val().replace("$", "") < 0 ? "cargo" : "abono";
        $(item).addClass(clase);
    });
}

function comentariosBeforeShow(event, o) {
   // alert(event);
}

function pageChanged(event) {
    var target = event.currentTarget != undefined ? event.currentTarget : event.srcElement;
    var pageNumber = $(target).text() - 1;
    rebindGrid({
        pageNumber: pageNumber
    });
}