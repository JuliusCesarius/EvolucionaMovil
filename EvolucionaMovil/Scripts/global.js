var t;
$(document).on("ready", function () {
    $("label").inFieldLabels();
    $("input").on("click", function () { this.select(); });
    t = window.setInterval(fixLabels, 200);
    //Fix de los checkbox que no pasan el valor al form
    $("[type='checkbox']").on("change", fixCheckbox);
    $("[type='checkbox']").val($("[type='checkbox']").attr('checked') == "checked");
    //jQuery.ajax({ cache: false });
    $('.money').priceFormat({
        prefix: '',
        suffix: ''
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

});

function fixCheckbox() {
    $(this).val($(this).attr('checked') == "checked");
}
function fixLabels() {
    clearInterval(t);
    $("label").inFieldLabels('refresh');
}