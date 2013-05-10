using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolucionaMovil.Models.Interfaces
{
    public interface IEvolucionaMovil
    {
        int PayCenterId { get; set; }
        string PayCenterName { get; }
    }
}
