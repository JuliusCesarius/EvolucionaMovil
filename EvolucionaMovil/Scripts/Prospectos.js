$(document).on("ready", function () {
    bindGrid();
    $("#pageSize").val(30);
    $("#fechaInicio").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#fechaFin").datepicker({ "dateFormat": "dd/mm/yy" });
    $("#Confirmacion").dialog({
        modal: true,
        resizable: false,
        autoOpen: false
    });
    $("#Actualizar").on("click", function (event) {
        event.preventDefault();
        FiltrarRegistros();
    });
    $("#pageSize").on("change", function () {
        FiltrarRegistros();
    });
    $("#Mensaje").dialog({
        modal: true,
        resizable: false,
        autoOpen: false,
        buttons: { "Aceptar": function () { $("#Mensaje").dialog("close"); } }
    });
});
function bindGrid(options) {
    var columns = [
         { name: 'Nombre', displayName: 'Nombre', width: '100px' },
         { name: 'Empresa',  displayName: 'Empresa', width: '110px' },
         { name: 'Telefono', displayName: 'Teléfono', formatFunction: ValidaNull },
         { name: 'Celular', formatFunction: ValidaNull },
         { name: 'Email', width: '100px' },
          { name: 'Comentario', width: '130px', formatFunction: ValidaNull },
         { name: 'FechaCreacion', displayName: 'Fecha', formatFunction: FormatoFecha },
         { name: 'PayCenterName', displayName: 'Paycenter' }, { displayName: '', width: Actions.colwidth, customTemplate: Actions.links },
         ];
    if (options == undefined) {
        options = { pageSize: 30, pageNumber: 0 };
    }
    var pageSize = options.pageSize;
    var pageNumber = options.pageNumber;
    var searchString = options.searchString;
    var fechaInicio = options.fechaInicio;
    var fechaFin = options.fechaFin;
    $("#grdProspectos").simpleGrid({
        url: "/Prospectos/GetProspectos",
        columns: columns,
        successFunction: ConfiguraLinkActiva,
        pageSize: pageSize,
        pageNumber: pageNumber,
        searchString: searchString,
        fechaInicio: fechaInicio,
        fechaFin: fechaFin
    });
}


function ConfiguraLinkActiva() {
//    $("a.linkActiva").each(function (i, item) {
//        var accion = $(item).html() == "true" ? "Desactivar" : "Activar";
//        var id = $(item).prop("id").replace("Accion", "")
//        $(item).html(accion);
//        $(item).on("click", function (event) {
//            event.preventDefault();
//            MostrarConfirmacion(id, accion);
//        });
//    });
}

function MostrarConfirmacion(id, accion) {
    $("#Confirmacion").dialog({
        title: accion + " Prospecto",
        buttons: { "Si": function () { EjecutarAccion(id, accion); }, "No": function () { $("#Confirmacion").dialog("close"); } }
    });
    $("#MensajeConfirmacion").html("¿Está seguro de querer " + accion.toLowerCase() + " al Prospecto?");

    $("#Confirmacion").dialog("open");
}

function FormatoFecha(value) {
    year = value.substring(0, 4);
    month = value.substring(5, 7);
    day = value.substring(8, 10);
    return day + "/" + month + "/" + year;
}

function ValidaNull(value) {
    if (value == null || value == "undefined") 
    {
        value = "";
    }
    return value;
}

function FiltrarRegistros() {
    //Agregar filtro por nombre, se separa de lado del servidor
    var FiltroCadena = ""
    if ($("#nombre").val() != "") {
        FiltroCadena = $("#nombre").val();
    }

    bindGrid({
        fechaInicio: $("#fechaInicio").val(),
        fechaFin: $("#fechaFin").val(),
        pageSize: $("#pageSize").val(),
        searchString: FiltroCadena
    });
}

function EjecutarAccion(id, accion) {
    var urlAccion;
    var sendData;

    if (accion == "Eliminar") {
        urlAccion = "/Prospectos/Delete";
        sendData = { 'id': id };
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
                MostrarResultado(accion + " Prospecto", result);
            }
            else {
                MostrarResultado(accion + " Prospecto", "No se ha podido determinar si se ejecutó la acción.");
            }
            FiltrarRegistros();
        },
        error: function () {
            MostrarResultado(accion + " Prospecto", "Se ha producido un error al ejecutar la acción.")
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