var t;
$(document).on("ready", function () {
    $("label").inFieldLabels();
    $(":text").on("click", function () { this.select(); });
    $(".tooltip").qtip({ content: $(this).attr("alt") });
    t = window.setInterval(fixLabels, 200);
    //Fix de los checkbox que no pasan el valor al form
    $("[type='checkbox']").on("change", fixCheckbox);
    $("[type='checkbox']").val($("[type='checkbox']").attr('checked') == "checked");
    //jQuery.ajax({ cache: false });
    $('.money').on("keydown", function (key) {
        $(this).data("prev", $(this).val());
        return true;
    });
    $('.money').on("keyup", function (key) {
        if (/^(|\d+\.\d{0,2}|\d+)$/.test($(this).val())) {
            return true;
        } else {
            $(this).val($(this).data("prev"));
            return false;
        }
    });

    //menu-drop-down
    $("#menu-top ul.child").removeClass("child");
    $("#menu-top ul.grandchild").removeClass("grandchild");
    $("#menu-top li").has("ul").hover(function () {
        $(this).addClass("current").children("ul").fadeIn();
    }, function () {
        $(this).removeClass("current").children("ul").stop(true, true).css("display", "none");
    });
    $("#ValidationMessage").show("blind", {}, 1000);
    $("#btn-message-aceptar").on("click", function (event) {
        event.preventDefault();
        $(this).parent().hide("blind", {}, 300);
    });

    //$("#ValidationMessage").removeClass("current").css("display", "none");

    //abre pagina para descargar Chrome
    $("#btnNavegador").on("click", function (event) {
        event.preventDefault();
        $(this).parent().hide("blind", {}, 300);
        window.open("https://www.google.com/intl/es/chrome/browser/?hl=es", 'window', '');
    });

    // $("#ValidationNavegador").show("blind", {}, 1000);
    DetectarNavegador();

    //Accordion
    $(".accordion").accordion({
        collapsible: true, active: false, heightStyle: "content"
    });

});

function fixCheckbox() {
    $(this).val($(this).attr('checked') == "checked");
}
function fixLabels() {
    clearInterval(t);
    $("label").inFieldLabels('refresh');
}

//validamos el navegador
function DetectarNavegador() {

   
    var is_chrome= navigator.userAgent.toLowerCase().indexOf('chrome/') > -1;
    if (is_chrome) {
        $("#BrowserMessage").hide("blind", {}, 300);
    }
    else {
        $("#BrowserMessage").show("blind", {}, 1000);
        //alert('Su navegador NO es Google Chrome');
    }

}

function showValidationMessage(mensaje, tipo) {
    $("#ValidationMessage").remove();
    $("<div id='ValidationMessage' class='" + tipo + "'><ul id='#content'><li>" + mensaje + "</li></ul><a href='#' id='btn-message-aceptar' class = 'freshbutton-blue small'>Aceptar</a></div>").insertAfter("#main");
    $("#ValidationMessage").show("blind", {}, 1000);
    $("#btn-message-aceptar").on("click", function (event) {
        event.preventDefault();
        $(this).parent().hide("blind", {}, 300);
    });
}