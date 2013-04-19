$(document).on("ready", function () {
    bindGrid();
    $("#pageSize").val(20);
    $("#fechaInicio").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#fechaFin").datepicker({ "dateFormat": "dd/mm/yy" });

    $("#pageSize").on("change", function () {
        bindGrid({ pageSize: $("#pageSize").val(), searchString: FiltroEstatus() });
    });
    $("#Todos").on("click", function () {
        bindGrid({ pageSize: $("#pageSize").val(), searchString: FiltroEstatus() });
    });
    $("#Activos").on("click", function () {
        bindGrid({ pageSize: $("#pageSize").val(), searchString: FiltroEstatus() });
    });
    $("#Inactivos").on("click", function () {
        bindGrid({ pageSize: $("#pageSize").val(), searchString: FiltroEstatus() });
    });

    $("#Actualizar").on("click", function (event) {
        event.preventDefault();
        bindGrid({
            fechaInicio: $("#fechaInicio").val(),
            fechaFin: $("#fechaFin").val(),
            pageSize: $("#pageSize").val(),
            searchString: FiltroEstatus()
        });
    });
});

function FiltroEstatus() {
    if ($("#Activos").prop("checked")) {
        return $("#Activos").val();
    }
    else if ($("#Inactivos").prop("checked")) {
        return $("#Inactivos").val();
    }
    else {
        return $("#Todos").val();
    }
}

function bindGrid(options) {
    var columns = [
         { name: 'UserName', displayName: 'Nombre Corto' },
         { name: 'Representante' },
         { name: 'Nombre', displayName: 'Empresa', width: '120px' },
         { name: 'Telefono', displayName: 'Teléfono' },
         { name: 'Celular' },
         { name: 'Email' },
         { name: 'FechaIngreso', displayName: 'Ingreso', formatFunction: FormatoFecha },
         { name: 'FechaCreacion', displayName: 'Creación', formatFunction: FormatoFecha },
         { displayName: '', customTemplate: '<a href="/PayCenters/Edit/{PayCenterId}">Editar<a/> | <a href="/PayCenters/Details/{PayCenterId}">Detalles<a/>' }
         ];
    if (options == undefined) {
        options = { pageSize: 20, pageNumber: 0 };
    }
    var pageSize = options.pageSize;
    var pageNumber = options.pageNumber;
    var searchString = options.searchString;
    var fechaInicio = options.fechaInicio;
    var fechaFin = options.fechaFin;
    $("#grdPaycenters").simpleGrid({
        url: "/PayCenters/GetPayCenters",
        columns: columns,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}

function FormatoFecha(value) {
    value = value.replace(/\-0/g, '-00');
    value = value.replace(/\-1/g, '-01');
    value = value.replace(/\-2/g, '-02');
    value = value.replace(/\-3/g, '-03');
    value = value.substring(0,12)
    return $.datepicker.formatDate('dd/mm/yy', new Date(value));
}