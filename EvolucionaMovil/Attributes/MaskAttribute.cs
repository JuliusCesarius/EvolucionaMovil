using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EvolucionaMovil.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaskAttribute : Attribute, IMetadataAware
    {
        private string _mask = string.Empty;
        public MaskAttribute(string mask)
        {
            _mask = mask;
        }
          
        public string Mask
        {
            get { return _mask; }
        }

        private const string ScriptText = "<script type='text/javascript'>" +
                                           "$(document).ready(function () {{" +
                                           "$('#{0}').mask('{1}');}});</script>";

        public const string templateHint = "_maskedInput";

        private int _count;

        public string Id
        {
            get { return "maskedInput_" + _count; }
        }

        internal HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            var list = Context.Items["Scripts"] as IList<string> ?? new List<string>();
            _count = list.Count;
            metadata.TemplateHint = templateHint;
            metadata.AdditionalValues[templateHint] = Id;
            list.Add(string.Format(ScriptText, Id, Mask));
            Context.Items["Scripts"] = list;
        }
    }
}