using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models.Enums
{
    public enum enumTipoMovimiento
    {
        //Movimiento de tipo negativo que disminuye el saldo en la cuenta donde se genera
        Cargo,
        //Movimiento de tipo positivo que incrementa el saldo en la cuenta donde se genera
        Abono
    }
}