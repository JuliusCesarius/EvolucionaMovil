using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvolucionaMovil.Repositories;

namespace EvolucionaMovil.Models.BR
{
    public class EstadoCuentaBR : cabinet.patterns.clases.BusinessRulesBase<Movimiento>
    {
        private EstadoDeCuentaRepository _estadoDeCuentaRepository;
        private EstadoDeCuentaRepository estadoDeCuentaRepository
        {
            get
            {
                if(_estadoDeCuentaRepository==null){
                    _estadoDeCuentaRepository= new EstadoDeCuentaRepository();
                }
                return _estadoDeCuentaRepository;
            }
            set
            {
            }
        }
        /// <summary>
        /// Valida que no exista registros con la Referencia en el Banco especificado
        /// </summary>
        /// <param name="Referencia"></param>
        /// <param name="BancoId"></param>
        /// <returns></returns>
        public bool isValidReference(String Referencia, Int32 BancoId)
        {
            return !estadoDeCuentaRepository.context.Abonos.Any(x => x.Referencia == Referencia && x.BancoId == BancoId);
        }
    }
}