using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cabinet.patterns.clases
{
    public class CrossValidationMessage
    {
        public enums.enumMessageType MessageType { get; set; }
        public int ValidationCode {get;set; }
        public string Message { get; set; }        
    }
}
