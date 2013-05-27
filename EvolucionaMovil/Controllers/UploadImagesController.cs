using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using EvolucionaMovil.Models;
using System.Web.Mvc;
using System.IO;

namespace EvolucionaMovil.Controllers
{
    public class UploadImagesController : Controller
    {
        [HttpPost]
        public WrappedJsonResult UploadImage(HttpPostedFileWrapper imageFile)
        {
            if (imageFile == null || imageFile.ContentLength == 0)//Validar que se haya subido el archivo
            {
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = "El archivo no pudo ser subido.",
                        ImagePath = string.Empty,
                        ThumbnailPath = string.Empty,
                        OriginalFileName = string.Empty
                    }
                };
            }
            else if (imageFile.ContentType != "image/jpeg")//validar que el archivo subido sea una imagen
            {
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = "El archivo subido no es un archivo de imagen.",
                        ImagePath = string.Empty,
                        ThumbnailPath = string.Empty,
                        OriginalFileName = string.Empty
                    }
                };
            }
            else if (imageFile.ContentLength > 4194304)//Validar que el tamaño no sea mayor al máximo permitido
            {
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = "El tamaño del archivo es mayor al máximo permitido (4MB).",
                        ImagePath = string.Empty,
                        ThumbnailPath = string.Empty,
                        OriginalFileName = string.Empty
                    }
                };
            }


            var directoryTemp = Server.MapPath("~/UploadImages/");
            if (!Directory.Exists(directoryTemp))
            {
                Directory.CreateDirectory(directoryTemp);
            }
            directoryTemp = Server.MapPath("~/UploadImages/Thumbnails/");
            if (!Directory.Exists(directoryTemp))
            {
                Directory.CreateDirectory(directoryTemp);
            }
            try
            {
                //Guardar imagen en el servidor
                string fileName = String.Format("{0}.jpg", Guid.NewGuid().ToString());
                string imagePath = Path.Combine(Server.MapPath(Url.Content("~/UploadImages")), fileName);
                string thumbnailPath = Path.Combine(Server.MapPath(Url.Content("~/UploadImages/Thumbnails")), fileName);

                //Si el tamaño es mayor al tamaño máximo en que se debe guardar entonces se usa el método para comprimir y guardar
                if (imageFile.ContentLength > 2097152)
                {
                    SaveCompressImage(2097152, imageFile.InputStream, imagePath);
                }
                else
                {
                    imageFile.SaveAs(imagePath);
                }
                //Generar miniatura de la imagen
                GenerateThumbnail(imageFile.InputStream, thumbnailPath);

                //Regresar la ruta de la imagen guardada para mostrarla
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = true,
                        Message = string.Empty,
                        ImagePath = Url.Content(String.Format("~/UploadImages/{0}", fileName)),
                        ThumbnailPath = Url.Content(String.Format("~/UploadImages/Thumbnails/{0}", fileName)),
                        OriginalFileName = imageFile.FileName
                    }
                };
            }
            catch
            {
                return new WrappedJsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = "Se ha producido un error al subir el archivo.",
                        ImagePath = string.Empty,
                        ThumbnailPath = string.Empty,
                        OriginalFileName = string.Empty
                    }
                };
            }
        }

        private void SaveCompressImage(long maxsize, Stream ms, string savepath)
        {
            //Codecs y parámetro para bajar la calidad de la imagen y comprimir el tamaño
            long quality = 100L;
            var imgCodec = GetEncoder(ImageFormat.Jpeg);
            var codecParams = new EncoderParameters(1);

            //Hacerlo hasta que el tamaño sea menor la máximo que se pasa como parámetro
            Image imgCompress = Image.FromStream(ms);
            while (ms.Length > maxsize)
            {
                quality -= 2;
                codecParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                ms = new MemoryStream();
                imgCompress.Save(ms, imgCodec, codecParams);
                imgCompress = Image.FromStream(ms);
            }

            //Guardar la imagen comprimida
            imgCompress.Save(savepath);
            ms.Close();
            ms.Dispose();
            imgCompress.Dispose();
        }

        private void GenerateThumbnail(Stream ms, string savepath)
        {
            //Obtener imagen original y calcular las dimensiones para la miniatura
            Image imgFullSize = Image.FromStream(ms);
            int intWidth, intHeight;
            intWidth = imgFullSize.Width;
            intHeight = imgFullSize.Height;
            if (imgFullSize.Height > 200)
            {
                intHeight = 200;
                intWidth = (int)(((float)intHeight / imgFullSize.Height) * imgFullSize.Width);
            }

            //Generar la miniatura
            Image imgThumbNail = new Bitmap(intWidth, intHeight);
            var graph = Graphics.FromImage(imgThumbNail);
            graph.DrawImage(imgFullSize, 0, 0, intWidth, intHeight);

            //Codecs y parámetro para bajar la calidad de la imagen y comprimir el tamaño, la primera vez se hace al 100 y solo en caso de ser necesario se va comprimiendo
            long quality = 100L;
            var imgCodec = GetEncoder(ImageFormat.Jpeg);
            var codecParams = new EncoderParameters(1);

            do
            {
                codecParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                ms = new MemoryStream();
                imgThumbNail.Save(ms, imgCodec, codecParams);
                imgThumbNail = Image.FromStream(ms);
                quality -= 2;
            }
            while (ms.Length > 51200);

            //Guardar Miniatura
            imgThumbNail.Save(savepath);
            ms.Close();
            ms.Dispose();
            imgFullSize.Dispose();
            imgThumbNail.Dispose();
        }

        //Para ontener el Codec de Jpeg
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}