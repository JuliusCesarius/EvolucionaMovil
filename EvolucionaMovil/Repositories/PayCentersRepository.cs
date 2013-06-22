using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Models;
using cabinet.patterns.clases;
using EvolucionaMovil.Models.Enums;

namespace EvolucionaMovil.Repositories
{
    public class PayCentersRepository : RepositoryBase<PayCenter, EvolucionaMovilBDEntities>
    {
        public PayCentersRepository()
            : base()
        {
        }

        public PayCentersRepository(EvolucionaMovilBDEntities context)
            : base(context)
        {
        }

        //public IEnumerable<Usuario> LoadUsuarios()
        //{
        //    return base.context.Usuarios.ToList();
        //}

        public IEnumerable<CuentaPayCenter> LoadTipoCuentas(Int32 PayCenterId)
        {
            return base.context.CuentasPayCenters.Where(x => x.PayCenterId == PayCenterId).ToList();
        }

        //public IEnumerable<Prospecto> LoadProspectos()
        //{
        //    return base.context.Prospectos.ToList();
        //}

        public PayCenter LoadByProspectoId(Int32 prospectoId)
        {
            return base.context.PayCenters.FirstOrDefault(x => x.ProspectoId == prospectoId);
        }

        public PayCenter LoadByIdName(string stringSearch)
        {
            Int32 IdBuscar = 0;
            int.TryParse(stringSearch, out IdBuscar);
            return base.context.PayCenters.FirstOrDefault(x => x.PayCenterId == IdBuscar || x.UserName == stringSearch);
        }

        public bool ExisteUsuario(string UserName)
        {
            return context.PayCenters.Any(p => p.UserName == UserName);
        }

        public IEnumerable<PayCenter> GetPayCenterBySearchString(string searchString)
        {
            var payCenters = context.PayCenters.Where(p => (p.UserName.Contains(searchString) || p.Nombre.Contains(searchString) )&& p.Baja == false);
            return payCenters;
        }

        public string GetLogotipo(int PayCenterId)
        {
            return context.PayCenters.Where(x => x.PayCenterId == PayCenterId).Select(x=>x.Logotipo).FirstOrDefault();
        }

        public CuentaPayCenter GetCuentaPayCenter(int PayCenterId, enumTipoCuenta TipoCuenta, int ProveedorId)
        {
            var payCenter = context.PayCenters.Where(p => p.PayCenterId == PayCenterId && p.Baja == false).FirstOrDefault();
            if (payCenter == null)
            {
                //todo:Implementar CrossValidationMessages
            }
            var cuentasPayCenter = payCenter.CuentasPayCenters.Where(p=>p.ProveedorId == ProveedorId && p.Baja==false).FirstOrDefault();
            return cuentasPayCenter;
        }
        /// <summary>
        /// Crea el registro de la Cuenta del PayCenter por proveedor. Permite asignar cuentas a un paycenter de forma dinámica
        /// </summary>
        /// <param name="PayCenterId"></param>
        /// <param name="TipoCuenta"></param>
        /// <param name="ProveedorId"></param>
        /// <returns>El objeto creado con Id con valor</returns>
        public CuentaPayCenter CreateCuentaPayCenter(int PayCenterId, enumTipoCuenta TipoCuenta, int ProveedorId)
        {
            var cuentaPayCenter = GetCuentaPayCenter(PayCenterId, TipoCuenta, ProveedorId);
            //Si existe no lo crea
            if (cuentaPayCenter != null)
            {
                //todo:Implementar CrossValidationMessages
                return null;
            }
            cuentaPayCenter = new CuentaPayCenter
            {
                PayCenterId = PayCenterId,
                TipoCuenta = (short)TipoCuenta.GetHashCode(),
                ProveedorId = ProveedorId
            };
            this.context.CuentasPayCenters.AddObject(cuentaPayCenter);
            this.Save();
            return cuentaPayCenter;
        }
        public string GetPayCenterNameById(int PayCenterId)
        {
            var payCenter = context.PayCenters.Where(p => p.PayCenterId == PayCenterId).FirstOrDefault();
            if (payCenter != null)
            {
                return payCenter.UserName;
            }
            else
            {
                return null;
            }
        }

        public int GetPayCenterByUserName(string UserName)
        {
            var payCenter = context.PayCenters.Where(p => p.UserName == UserName && p.Baja == false).FirstOrDefault();
            if (payCenter != null)
            {
                return payCenter.PayCenterId;
            }
            else
            {
                return 0;
            }
        }

        public string  GetPayCenterEmail(string UserName)
        {
            var payCenter = context.PayCenters.Where(p => p.UserName == UserName && p.Baja == false).FirstOrDefault();
            if (payCenter != null)
            {
                return payCenter.Email;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}