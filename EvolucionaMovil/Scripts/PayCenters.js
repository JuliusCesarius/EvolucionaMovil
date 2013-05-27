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
        FiltrarRegistros();
    });

    $("#Confirmacion").dialog({
        modal: true,
        resizable: false,
        autoOpen: false
    });

    $("#Mensaje").dialog({
        modal: true,
        resizable: false,
        autoOpen: false,
        buttons: { "Aceptar": function () { $("#Mensaje").dialog("close"); } }
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

function FiltrarRegistros() {
    //Agregar filtro por nombre, se separa de lado del servidor
    var FiltroCadena = FiltroEstatus();
    if ($("#nombre").val() != "") {
        FiltroCadena = FiltroCadena + "," + $("#nombre").val();
    }

    bindGrid({
        fechaInicio: $("#fechaInicio").val(),
        fechaFin: $("#fechaFin").val(),
        pageSize: $("#pageSize").val(),
        searchString: FiltroCadena
    });
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
//         { name: 'FechaCreacion', displayName: 'Creación', formatFunction: FormatoFecha },
         {displayName: '', width: Actions.colwidth, customTemplate: Actions.links },
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
        successFunction: ConfiguraLinkActiva,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}

function FormatoFecha(value) {
    //value = value.replace(/\-0/g, '-00');
    //value = value.replace(/\-1/g, '-01');
    //value = value.replace(/\-2/g, '-02');
    //value = value.replace(/\-3/g, '-03');
    //return $.datepicker.formatDate('dd/mm/yy', value);
    year = value.substring(0, 4);
    month = value.substring(5, 7);
    day = value.substring(8, 10);
    return day + "/" + month + "/" + year;
}

function ConfiguraLinkActiva() {
    $("a.linkActiva").each(function (i, item) {
        var accion = $(item).html() == "true" ? "Desactivar" : "Activar";
        var id = $(item).prop("id").replace("Accion", "")
        $(item).html(accion);
        $(item).on("click", function (event) {
            event.preventDefault();
            MostrarConfirmacion(id, accion);
        });
    });
}

function MostrarConfirmacion(id, accion) {
    $("#Confirmacion").dialog({
        title: accion + " PayCenter",
        buttons: { "Si": function () { EjecutarAccion(id, accion); }, "No": function () { $("#Confirmacion").dialog("close"); } }
    });
    $("#MensajeConfirmacion").html("¿Está seguro de querer " + accion.toLowerCase() + " al PayCenter?");

    $("#Confirmacion").dialog("open");
}

function EjecutarAccion(id, accion) {
    var urlAccion;
    var sendData;

    if (accion == "Eliminar"){
        urlAccion = "/PayCenters/Delete";
        sendData = { 'id': id };
    }
    else {
        urlAccion = "/PayCenters/Activate";
        sendData = { 'id': id, 'accion': accion };
    }

    $.ajax(
    {
        type: 'POST',
        async: false,
        dataType: 'text',
        url: urlAccion,
        data: sendData,
        success: function (result) {
            if (result != null) {
                MostrarResultado(accion + " PayCenter", result);
            }
            else {
                MostrarResultado(accion + " PayCenter", "No se ha podido determinar si se ejecutó la acción.");
            }
            FiltrarRegistros();
        },
        error: function () {
            MostrarResultado(accion + " PayCenter", "Se ha producido un error al ejecutar la acción.")
        }
    });
}

function MostrarResultado(Titulo, Mensaje) {
    $("#Mensaje").dialog({
        title: Titulo
    });
    $("#MensajeAccion").html(Mensaje);
    $("#Confirmacion").dialog("close");
    $("#Mensaje").dialog("open");
}