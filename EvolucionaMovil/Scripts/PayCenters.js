$(document).on("ready", function () {
    bindGrid();
});

function bindGrid(options) {
    var columns = [
         { name: 'NombreCorto', displayName: 'Nombre Corto' },
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
    $("#grdPaycenters").simpleGrid({
        url: "/PayCenters/GetPayCenters",
        columns: columns,
        pageSize: pageSize,
        pageNumber: pageNumber
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