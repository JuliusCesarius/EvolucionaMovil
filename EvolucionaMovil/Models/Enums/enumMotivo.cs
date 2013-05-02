using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models.Enums
{
    public enum enumMotivo
    {
        //(Abono)
        Deposito,
        //(Cargo) Cobro de servicio
        Comision,
        //(Abono y Cargo) Entre cuentas y PayCenters
        Traspaso,
        //(Abono) Comisión positiva por recomendado o a discreción del Staff
        Bonificacion,
        //(Cargo) Usado para pago de servicios
        Cobro,
        //(Cargo) Usado para pago de servicios
        Pago,
        //(Cargo) Compra de paquetes, etc
        Compra,
        //(Abono y Cargo) En caso de pagar servicio con saldo negativo
        Financiamiento
    }
}