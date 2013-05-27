$(document).on("ready", function () {
    $("#SaveImageIFE").on("click", function (event) {
        event.preventDefault();
        $("#SelectImageIFE").click();
    });
    $("#SelectImageIFE").on("change", function (event) {
        event.preventDefault();
        UploadImage("IFE");
    });
    $("#SaveImageComp").on("click", function (event) {
        event.preventDefault();
        $("#SelectImageComp").click();
    });
    $("#SelectImageComp").on("change", function (event) {
        event.preventDefault();
        UploadImage("Comprobante");
    });
});

function UploadImage(typeImage) {
    var fileInput, imgDiv, imgForm;

    if (typeImage == "IFE") {
        fileInput = $('#SelectImageIFE');
        imgDiv = $("#ImageIFE");
        imgForm = $("#ImgFormIFE")
    }
    else {
        fileInput = $('#SelectImageComp');
        imgDiv = $("#ImageComp");
        imgForm = $("#ImgFormComp")
    }

    var maxSize = fileInput.data('max-size');

    imgDiv.html("");

    if (fileInput.get(0).files != null) {
        if (fileInput.get(0).files.length) {
            var oFile = fileInput.get(0).files[0];
            var rFilter = /^(image\/jpeg)$/i;
            if (!rFilter.test(oFile.type)) {
                imgDiv.html("<p>El archivo seleccionado no es un archivo de imagen válido.</p>");
            }
            else if (oFile.size > maxSize) {
                imgDiv.html("<p>El tamaño del archivo seleccionado es mayor al máximo permitido (4MB).</p>");
            }
            else {
                //Agregar evento load del target para cuando se devuelva el resultado se muestre la imagen donde corresponda
                $("#UploadTarget").on("load", function (event) {
                    event.preventDefault();
                    UploadImageComplete(typeImage);
                });
                imgForm.submit();
            }
        } else {
            imgDiv.html("<p>No se ha seleccionado ningún archivo.</p>");
        }
    }
}

function UploadImageComplete(typeImage) {
    var imgDiv, imgText, imgField;

    if (typeImage == "IFE") {
        $("#ImgFormIFE").trigger("reset");
        imgDiv = $("#ImageIFE");
        imgText = $("#NameImageIFE");
        imgField = $("#IFE");
    }
    else {
        $("#ImgFormComp").trigger("reset");
        imgDiv = $("#ImageComp");
        imgText = $("#NameImageComp");
        imgField = $("#Comprobante");
    }

    var newImg = $.parseJSON($("#UploadTarget").contents().find("#jsonResult")[0].innerHTML);

    imgDiv.html("");
    if (newImg.IsValid == false) {
        imgDiv.html("<p>" + newImg.Message + "</p>");
        return;
    }

    imgText.val(newImg.OriginalFileName);
    imgField.val(newImg.ImagePath);

    var img = new Image();
    img.src = newImg.ThumbnailPath;
    $(img).hide();
    $(img).appendTo(imgDiv);

    $(img).fadeIn(500, null);

    //Quitar evento que se agrega cuando se sube la imagen
    $("#UploadTarget").off("load");
}