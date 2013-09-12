using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using EvolucionaMovil.Models.Classes;
using cabinet.patterns.enums;
using EvolucionaMovil.Models.Extensions;
namespace EvolucionaMovil.Controllers
{
    public class ImgFichaDepositoController : CustomControllerBase
    {
        public ActionResult IndexPartial()
        {
            ViewBag.Ruta = "";
            ViewBag.Nombre = "";
            return PartialView();
        }
        [HttpPost]
        public ActionResult IndexPartial(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".")).ToLower();
                if (extension.ToLower() == ".png" || extension.ToLower() == ".jpg")
                {
                    var fileName = "imgtemp_" + DateTime.UtcNow.GetCurrentTime().ToString("yyyyMMdd")  + new Random().Next(0, 99999).ToString()+extension.ToLower();
                    var directoryTemp = Server.MapPath("~/temp/");
                    if (!Directory.Exists(directoryTemp))
                    {
                        Directory.CreateDirectory(directoryTemp);
                    }
                    var path = Path.Combine(directoryTemp, fileName);
                    string filepathToSave = "temp/" + fileName;
                    ViewBag.Ruta = "/temp/" + fileName;
                    ViewBag.Nombre = file.FileName;
                    try
                    {
                        file.SaveAs(path);
                    }
                    catch (Exception ex)
                    {
                        AddValidationMessage(enumMessageType.UnhandledException, "No fue posible guardar la imagen del comprobante: " + ex.Message);
                    }
                }
                else
                {
                    AddValidationMessage(enumMessageType.DataValidation, "La imagen no cumple con el formato");
                }
            }
            else
            {
                AddValidationMessage(enumMessageType.DataValidation, "Seleccione la imagen de la ficha de depósito.");
            }
            return PartialView();
        }
    }
}