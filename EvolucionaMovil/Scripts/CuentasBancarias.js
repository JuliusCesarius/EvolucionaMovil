var modif = false;
$(document).on("ready", function () {
    init();
    $('.proveedor').on("click", proveedorClick);
    $('#BancoId').on("change", bancoIdChanged);
    $("#saveConfig").on("click", saveConfigClick);
    $("#cuentaForm").dialog({ modal: true, autoOpen: false, title: "Nueva cuenta", width: 415 });
    $("#cuentaForm").find("form").on("submit", cuentaFormSubmit);
    $("#cuentaDetail").dialog({ modal: true, autoOpen: false, title: "Nueva cuenta", width: 415 });
    $("#cuentaDetail").find("form").on("submit", cuentaEditSubmit);
    $("#newCuenta").on("click", newCuentaClick);
    $('#BancoId').trigger("change");
});
function openClick(event) {
    var selected = $(event.currentTarget)[0];
    //$(selected).parent().toggleClass("closed");
    $(selected).parent().find(".detail").toggle("blind");
}

function init() {
    $('#cuentas .open').off("click", openClick);
    $('#cuentas .cuenta').off("click", cuentaClick);
    $('.tiposervicio a').off("click", tipoCuentaClick);
    $('.btnEdit').off("click", btnEditClick)

    $('#cuentas .open').on("click", openClick);
    $('#cuentas .cuenta').on("click", cuentaClick);
    $('.tiposervicio a').on("click", tipoCuentaClick);
    $('.btnEdit').on("click",btnEditClick)
    //$("#btnSaveCuenta").on("click", cuentaFormSubmit);
}

function cuentaClick(event) {
    if (modif) {
        modif = false;
        if (confirm("¿Desea guardar los cambios antes de continuar?")) {
            $("#saveConfig").trigger("click");
            return null;
        }
    }
    var selected = $(event.currentTarget)[0];
    $('#cuentas .cuenta').removeClass("selected");
    $(selected).addClass("selected");

    $('.proveedor').removeClass("selected");
    $(":checkbox").prop("checked", false);
    $('.hddSelected').val(false);

    $('.proveedor').show("blind");

    var provs = $.parseJSON($(selected).find(".hddProvs").val());
    $(provs).each(function (index) {
        var e = function () {
            var currentTarget = [];
        };
        e.currentTarget = [$('.proveedor.' + this)];
        selectProveedor(e);
    });

    $('#CuentaBancariaId').val($(selected).find(".CuentaId").val());

    $('.tiposervicio a').addClass("selected");
    event.preventDefault();
    event.stopPropagation();
}
function tipoCuentaClick(event) {
    var selected = $(event.currentTarget)[0];
    $('.tiposervicio a').removeClass("selected");
    $(selected).addClass("selected");
    $('.proveedor').hide('blind');
    $('[tipo=' + $(selected).attr("tiposerv") + ']').parent().parent().show('blind');
}
function selectProveedor(event) {
    var selected = $(event.currentTarget)[0];
    $(selected).toggleClass("selected");
    $(selected).find(".hddSelected").val($(selected).hasClass("selected"));
    $(selected).find(":checkbox").prop("checked", $(selected).hasClass("selected"));
    $('.tiposervicio a').removeClass("selected");    
}
function proveedorClick(event) {
    modif = true;
    selectProveedor(event);
}
function bancoIdChanged(event) {
    if (modif) {
        modif = false;
        if (confirm("¿Desea guardar los cambios antes de continuar?")) {
            $("#saveConfig").trigger("click");
            return null;
        }
    }
    var selected = $(event.currentTarget)[0];
    $('.cuenta').hide("blind");
    $('.cuenta.' + $(selected).find(":selected")[0].id).show("blind");

    $(".hddBancoId").val($(selected).find(":selected")[0].id);

    $('.tiposervicio a').addClass("selected");
    $('.proveedor').removeClass("selected");
    $(":checkbox").prop("checked", false);
    $('.proveedor').show("blind");
}

function saveConfigClick(event) {
    modif = false;
    $.post("CuentasBancarias/SaveConfig", $("#configForm").serialize(), function (data) {
        $("#cuenta_" + data.CuentaBancariaId).find(".hddProvs").val(JSON.stringify(data.Proveedores));
        showValidationMessage("Guardado con éxito", "Succeed");
    });
}
function cuentaFormSubmit(event) {
    modif = false;
    $.post("CuentasBancarias/Create", $("#cuentaForm").find("form").serialize(), function (data) {
        if (data.indexOf("detail") != -1) {
            showValidationMessage("Guardado con éxito", "Succeed");
            $("#cuentaForm").dialog("close");
            $(data).insertBefore("#newCuenta");
            init();
        } else {
            $("#cuentaForm").html(data);
            $("#cuentaForm").find("form").on("submit", cuentaFormSubmit);
            $("label").inFieldLabels();
        }
    });
    event.preventDefault();
    event.stopPropagation();
}

function newCuentaClick(event) {
    $("#cuentaForm").dialog("open");
}
function btnEditClick(event) {
    var cuentaId = $($(event.currentTarget)[0]).parent().parent().parent().find(".CuentaId").val();
    $.get("CuentasBancarias/_Edit", { "id": cuentaId }, function (data) {
        $("#cuentaDetail").html(data);
        $("#cuentaDetail").find("form").on("submit", cuentaEditSubmit);
        $("label").inFieldLabels();
        $("#cuentaDetail").dialog("open");
    });
}

function cuentaEditSubmit(event) {
    modif = false;
    $.post("CuentasBancarias/_Edit", $("#cuentaDetail").find("form").serialize(), function (data) {
        if (data.indexOf("detail") != -1) {
            showValidationMessage("Guardado con éxito", "Succeed");
            $("#cuentaDetail").dialog("close");
            $(".cuenta.selected").replaceWith($(data).addClass("selected"));
            init();
        } else {
            $("#cuentaDetail").html(data);
            $("#cuentaDetail").find("form").on("submit", cuentaFormSubmit);
            $("label").inFieldLabels();
        }
    });
    event.preventDefault();
    event.stopPropagation();
}