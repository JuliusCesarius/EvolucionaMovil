using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cabinet.patterns.clases;

namespace cabinet.patterns.interfaces
{
    public interface ICrossValidation
    {
        bool Succeed { get; set; }
        List<CrossValidationMessage> ValidationMessages { get; set; }
        bool AddValidationMessage(int ValidationCode);
        bool AddValidationMessage(enums.enumMessageType MessageType, string Message);
    }
}
