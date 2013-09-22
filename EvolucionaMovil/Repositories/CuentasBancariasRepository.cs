using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using cabinet.patterns.clases;
using EvolucionaMovil.Models;

namespace EvolucionaMovil.Repositories
{
    public class CuentasBancariasRepository : RepositoryBase<CuentaBancaria, EvolucionaMovilBDEntities>
    {
        /// <summary>
        /// Devuelve la configuración de la Cuenta Bancaria relacionada con un proveedor específico.
        /// </summary>
        /// <param name="CuentaBancariaId"></param>
        /// <param name="ProveedorId"></param>
        /// <returns></returns>
        internal IEnumerable<BancoCuentaCaption> GetReferenceCaptions()
        {
            return this.context.BancoCuentaCaptions;
        }

        /// <summary>
        /// Permite guardar la configuración de la cuenta bancaria con la lista de ProveedoresId proporcionada
        /// </summary>
        /// <param name="CuentaBancariaId"></param>
        /// <param name="Proveedores"></param>
        /// <param name="EliminarNoListados">Si está en True, elimina todas las configuraciones del proveedor con la cuenta que no estén en la lista proporcionada</param>
        internal void SaveConfigCuentaProveedores(int CuentaBancariaId, List<int> Proveedores, bool EliminarNoListados)
        {
            var cuentaBancaria = this.context.CuentaBancarias.Where(x => x.CuentaId == CuentaBancariaId).FirstOrDefault();
            if (cuentaBancaria == null) {
                throw new Exception("No existe el la Cuenta Bancaria proporcionada");
            }
            List<int> provToAdd = new List<int>();
            foreach (var proveedorId in Proveedores)
            {
                var cuentaProveedor = cuentaBancaria.Proveedores.Where(x => x.ProveedorId == proveedorId).FirstOrDefault();
                if (cuentaProveedor == null)
                {
                    provToAdd.Add(proveedorId);
                }
                else
                {
                    context.ObjectStateManager.ChangeObjectState(cuentaProveedor, System.Data.EntityState.Modified);
                }
            }
            //Elimino las otras configuraciones que no existen
            var unchangedProvs =cuentaBancaria.Proveedores.Where(x=>x.EntityState==System.Data.EntityState.Unchanged).ToList();
            foreach (var proveedor in unchangedProvs)
            {
                cuentaBancaria.Proveedores.Remove(proveedor);
            }
            provToAdd.ForEach(x =>
            {
                var prov = context.Proveedors.Where(y => y.ProveedorId == x).FirstOrDefault();
                if (prov != null)
                {
                    cuentaBancaria.Proveedores.Add(prov);
                }
            });
            context.SaveChanges();
        }
    }
}