$(document).on("ready", function () {
    $("label").inFieldLabels();
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

