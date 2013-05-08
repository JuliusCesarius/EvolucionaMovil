(function ($) {
    $.simpleGrid = function (element, options) {
        this.options = {};
        element.data('simpleGrid', this);
        this.init = function (element, options) {
            this.options = $.extend({}, $.simpleGrid.defaultOptions, options);
            this.id = this.options.id;
            //Call private function
            updateElement(element, this.options);
        };

        //Public function
        this.load = function () {
            alert("");
        };

        this.bind = bindTableGrid;

        this.init(element, options);
    };

    $.fn.simpleGrid = function (options) {
        return this.each(function () {
            (new $.simpleGrid($(this), options));
        });
    };

    $.fn.simpleGrid.bind = bindTableGrid;

    function updateElement(element, options) {
        var id = element.attr("id");
        options.id = id;
        if ($(element).is("table")) {
            element.attr("id", "");
        }
        var cellsSinWidth = new Array();
        var contSinWidth = 0;
        var sumWidths = 0;
        if ($(element).is("table")) {
            var grid = jQuery('<div/>', {
                id: id,
                class: options.class
            });
            $(element).after($(grid));
            grid.append("<div class='sgHeader' ></div>");
            $(element).find("th").each(function () {
                var columHeader = jQuery("<div class='sgColumn'>" + this.innerHTML + "</div>");
                grid.find(".sgHeader").append(columHeader);
                if ($(this).width() != 0 && options.respectMesures) {
                    columHeader.width($(this).width());
                    sumWidths += $(this).width();
                } else if (false) {
                    //sumWidths=$(this).width();
                } else {
                    contSinWidth++;
                    cellsSinWidth.push(columHeader);
                }
            });
            var body = jQuery("<div class='sgBody' ></div>");
            grid.append(body);
            $(element).find("tr").each(function () {
                if ($(this).find("th").length > 0) {
                    return true;
                }
                var row = jQuery("<div class='sgRow'></div>");
                body.append(row);
                $(this).find("td").each(function () {
                    var cell = jQuery("<div class='sgCell'>" + this.innerHTML + "</div>");
                    if ($(this).width() != 0 && options.respectMesures) {
                        cell.width($(this).width());
                    } else if (false) {

                    } else {
                        cellsSinWidth.push(cell);
                    }
                    row.append(cell);
                });
            });
            if ($(element).width() != 0 && options.respectMesures) {
                grid.width($(element).width());
            }
            $(cellsSinWidth).each(function () {
                $(this).width((100 * (grid.width() - sumWidths) / contSinWidth) / grid.width() + "%");
            });
            $(element).hide();
        } else {
            if (options.autoLoad) {
                bindTableGrid(options);
            }
        }
    };

    $.simpleGrid.defaultOptions = {
        id: 'grd',
        class: 'sgGrid',
        width: '100%',
        respectMesures: false,
        autoLoad: true,
        url: undefined,
        columns: undefined,
        showHeaders: true,
        method: 'POST',
        pageSize: -1,
        pageNumber: -1,
        searchString: "",
        fechaInicio: null,
        fechaFin: null,
        data:null,
        selectedData: null,
        selectedURL: null,
        selectedFunction: null
    }

    function bindTableGrid(options) {
        var grdId = options.id;
        var Url = options.url;
        var columns = options.columns;
        var method = options.method;
        var successFunction = options.successFunction;
        var pageChangeFunction = options.pageChangeFunction;
        var errorFunction = options.errorFunction;
        var completeFunction = options.completeFunction;
        var pageSize = options.pageSize;
        var pageNumber = options.pageNumber;
        var searchString = options.searchString;
        var fechaInicio = options.fechaInicio;
        var fechaFin = options.fechaFin;
        var data = options.data;

        if (Url != undefined && Url != null) {
            $.ajax(
            {
                type: options.method,
                async: true,
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                url: Url,
                data: JSON.stringify({
                    'pageSize': pageSize,
                    'pageNumber': pageNumber,
                    'searchString': searchString,
                    'fechaInicio': fechaInicio,
                    'fechaFin': fechaFin,
                }),
                beforeSend: function (xhr) {
                    $('#' + grdId).addClass('ajaxRefreshing');
                    xhr.setRequestHeader('X-Client', 'jQuery');
                },
                success: function(result){
                    result = $.parseJSON(result);
                    onSuccess(result,grdId,options,columns,successFunction,pageChangeFunction);
                },
                complete: function () {
                    onComplete(grdId,completeFunction);
                }
            });
        }else{
            if (data!=null){
                onSuccess(data,grdId,options,columns,successFunction,pageChangeFunction);
                onComplete(grdId,completeFunction);
            }
        }
    }
    function onSuccess(result,grdId,options,columns,successFunction,pageChangeFunction) {
                var currentPage = result.CurrentPage;
                var pageSize = result.PageSize;
                var totalRows = result.TotalRows;

                var grid = $("#" + grdId);
                grid.html("");
                grid.append("<div class='sgHeader' ></div>");

                if (options.showHeaders) {
                    if (columns != undefined) {
                        $.each(columns, function (i, column) {
                            var columHeader = jQuery("<div class='sgColumn'>" + (column.displayName != undefined ? column.displayName : column.name) + "</div>");
                            grid.find(".sgHeader").append(columHeader);
                            if (column.width != undefined) {
                                columHeader.width(column.width);
                            }
                            if (column.cssClass != undefined) {
                                columHeader.addClass(column.cssClass);
                            }
                        });
                    } else {
                        if (result != null && result.Result.length > 0) {
                            var item = result.Result[0];
                            $.each(item, function (property) {
                                var columHeader = jQuery("<div class='sgColumn'>" + property + "</div>");
                                grid.find(".sgHeader").append(columHeader);
                                if (column.cssClass != undefined) {
                                    columHeader.addClass(column.cssClass);
                                }
                            });
                        }
                    }
                }

                var body = jQuery("<div class='sgBody' ></div>");
                grid.append(body);

                //si no trajo registros, aviso
                if (result.length == 0) {
                    $(body).append("<span class='label'>No existen registros.</span>");
                    return true;
                }

                //agrego registros 
                $.each(result.Result, function (i, item) {

                    var row = jQuery("<div class='sgRow'></div>");
                    row.data("item", item);
                    if(options!=null & options!=undefined &options.selectedURL!=null){
                        row.css("cursor","pointer");
                        row.on("click",function(event){
                            event.preventDefault();
                            if(options!=null && options!=undefined){
                                var dataValue = null;
                                if(options.selectedData != null){
                                    dataValue=item[options.selectedData];
                                }
                                if(options.selectedURL!=null){
                                    window.location.href=options.selectedURL+"/"+dataValue;
                                }
                            }
                        });
                    }
                    body.append(row);

                    if (columns == undefined) {
                        $.each(item, function (property) {
                            var cell = jQuery("<div class='sgCell'>" + item[property] + "</div>");
                            if (column.cssClass != undefined) {
                                cell.addClass(column.cssClass);
                            }
                            cell.val(item[property]);
                            row.append(cell);
                        });
                    } else {
                        $.each(columns, function (i, column) {
                            var content;
                            if (column.customTemplate != undefined) {
                                content = column.customTemplate;
                                var campos = column.customTemplate.match(/{\w+}/gi);
                                if (campos != null) {
                                    $.each(campos, function (i, campo) {
                                        content = content.replace(campo, item[campo.replace("{", "").replace("}", "")]);
                                    });
                                }
                            } else {
                                content = (column.formatFunction != undefined ? column.formatFunction(item[column.name]) : item[column.name]);
                            }
                            var cell = jQuery("<div class='sgCell'>" + content + "</div>");
                            cell.val(content);
                            if (column.width != undefined) {
                                cell.width(column.width);
                            }
                            if (column.cssClass != undefined) {
                                cell.addClass(column.cssClass);
                            }
                            row.append(cell);
                        });
                    }

                });
                if(pageSize<totalRows){
                    var pager = jQuery("<div class='sgPager'></div>");
                    var pages = totalRows/pageSize +1
                    for(var i=1;i<pages;i++){
                        pager.append("<a href='#' "+(i==currentPage+1?"class='selected'":"")+">"+i+"</a>");
                    }
                    pager.find("a").on("click",function(event){
                        if(!$(event.target).hasClass("selected")){
                            options.pageNumber=event.target.innerHTML-1;
                            bindTableGrid(options)
                            if (pageChangeFunction != undefined){
                                pageChangeFunction(event);
                            }
                        }
                    });
                    body.append(pager);
                }
                if (successFunction != undefined) {
                    successFunction();
                }
            }

            function onSelected(item,options){
                if(options!=null && options!=undefined){
                    var dataValue = null;
                    if(options.selectedData != null){
                        dataValue=item[options.selectedData];
                    }
                    if(options.selectedURL!=null){
                        $(window).location=options.selectedURL+"\\"+dataValue;
                    }
                }
            }

            function onComplete(grdId,completeFunction) {
                $('#' + grdId + "_container").removeClass('ajaxRefreshing');
                if (completeFunction != undefined) {
                    completeFunction();
                }
            }
})(jQuery);