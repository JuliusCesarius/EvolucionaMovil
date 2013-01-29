using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models.Enums
{
    public enum enumMotivo
    {
        //Ccompra de saldo
        Abono,
        //Ccobro de servicio
        Comision,
        //Entre cuentas
        Traslado,
        //Comisión positiva por recomendado
        Bonificacion,
        //compra de paquetes, etc
        Cargo,
        //Usado para pago de servicios
        Pago,
        //En caso de pagar servicio con saldo negativo
        Financiamiento
    }
}