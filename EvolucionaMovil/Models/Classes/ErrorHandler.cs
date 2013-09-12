using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Repositories;
using cabinet.patterns.enums;
using System.IO;
using System.Text;
using EvolucionaMovil.Models.Extensions;

namespace EvolucionaMovil.Models.Classes
{
    public static class ErrorHandler
    {
        private static ErrorsRepository _errorsRepository;

        public static bool LogError(Exception Exception, object Data)
        {
            var baseException = Exception.GetBaseException();
            string data = null;
            try
            {
                if (Data != null)
                {
                    data = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
                }
            }
            catch (Exception ex)
            {
                data = "<<ocurrió un problema al serializar el objeto>>";
            }
            return LogError(null, enumMessageType.UnhandledException, baseException.Message, baseException.StackTrace, data);
        }

        public static bool LogError(string Message, enumMessageType ErrorType, string Data)
        {
            return LogError(null, ErrorType, Message, null, Data);
        }

        public static bool LogError(string ErrorCode, enumMessageType ErrorType, string Message, string Stacktrace, string Data)
        {
            Data = string.IsNullOrEmpty(Data) ? HttpContext.Current.Request.Params.ToString() : Data;
            ErrorLog errorLog = new ErrorLog
            {
                Data = Data,
                ErrorCode = ErrorCode,
                ErrorType = (short)enumMessageType.UnhandledException,
                FechaCreacion = DateTime.UtcNow.GetCurrentTime(),
                IP = HttpContext.Current.Request.UserHostAddress,
                Message = Message,
                Stacktrace = Stacktrace,
                UserName = HttpContext.Current.User.Identity.Name,
                URL = HttpContext.Current.Request.Url.ToString()
            };
            return LogError(errorLog);
        }
        public static bool LogError(ErrorLog ErrorLog)
        {
            try
            {
                if (_errorsRepository == null)
                {
                    _errorsRepository = new ErrorsRepository();
                }
                _errorsRepository.Add(ErrorLog);
                return _errorsRepository.Save();
            }
            catch (Exception ex)
            {
                //Intenta guardar en un log físico
                try
                {
                    using (StreamWriter writer = new StreamWriter(System.Web.HttpContext.Current.Server.MapPath("~/")+"errorLog.txt", true))
                    {
                        StringBuilder errorText = new StringBuilder();
                        errorText.Append(DateTime.UtcNow.GetCurrentTime().GetCurrentTime().ToShortDateString() + " " + DateTime.UtcNow.GetCurrentTime().ToShortTimeString());
                        errorText.Append(" | " + ((enumMessageType)ErrorLog.ErrorType).ToString());
                        errorText.Append(" | " + (string.IsNullOrEmpty(ErrorLog.ErrorCode) ? " " : ErrorLog.ErrorCode));
                        errorText.Append(" | " + ErrorLog.Message);
                        errorText.Append(" | " + ErrorLog.IP);
                        writer.WriteLine(errorText.ToString());
                    }
                    return true;
                }
                catch (Exception ex2)
                {
                    //Tampoco pudo guardar en arhivo
                    return false;
                }
            }
        }
    }
}