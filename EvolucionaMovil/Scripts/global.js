$(document).on("ready", function () {
    $("label").inFieldLabels();
    $("input").on("click", function () { this.select(); });
    //Fix de los checkbox que no pasan el valor al form
    $("[type='checkbox']").on("change", fixCheckbox);
    $("[type='checkbox']").val($("[type='checkbox']").attr('checked') == "checked");
    //jQuery.ajax({ cache: false });
    $('.money').priceFormat({
        prefix: '',
        suffix: ''
    });
    //$(".money").mask("(999) 999-9999");
    //$("select").selectbox();   

    //    $('form').validate().settings.errorPlacement = function (error, element) {
    //        offset = element.offset();
    //        error.insertBefore(element)
    //        error.addClass('errorMessage');  // add a class to the wrapper
    //        error.css('position', 'absolute');
    //        error.css('left', element.outerWidth());
    //        //error.css('top', offset.top);
    //    };
});

function fixCheckbox() {
    $(this).val($(this).attr('checked') == "checked");
}