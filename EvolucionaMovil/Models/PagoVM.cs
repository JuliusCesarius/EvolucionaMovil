using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Repositories;
using System.Linq;
using EvolucionaMovil.Models.Enums;
namespace EvolucionaMovil.Models
{
    public class PagoVM
    {

        [Required(ErrorMessage = "El nombre es requerido")]
        public string ClienteNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime? FechaVencimiento { get; set; }
        [Required(ErrorMessage = "El importe es requerido")]
        public decimal? Importe { get; set; }

        public int PagoId { get; set; }
        public int PayCenterId { get; set; }

        public int ServicioId { get; set; }
        public string ServicioNombre { get; set; }

        public int Status { get; set; }
        public string Estatus { get; set; }

        private IEnumerable<EnumWrapper> _Servicios { get; set; }
        public IEnumerable<EnumWrapper> Servicios
        {
            get
            {
                if (_Servicios == null)
                {
                    List<EnumWrapper> ls = new List<EnumWrapper>();
                    ServiciosRepository rServicios = new ServiciosRepository();
                    ls = rServicios.ListAll().Select(x => new EnumWrapper
                    {
                        Text = x.Nombre,
                        Value = x.ServicioId
                    }).ToList();
                    ls.Insert(0, new EnumWrapper { Text = "Seleccione un Servicio", Value = 0 });
                    _Servicios = ls;
                }
                return _Servicios;
            }
            set { _Servicios = value; }
        }

        public List<HistorialEstatusVM> HistorialEstatusVM { get; set; }
        public CambioEstatusVM CambioEstatusVM { get; set; }
        public List<DetallePago> DetallePagos { get; set; }

        public PagoVM()
        {
            ClienteNombre = null;
            DetallePagos = new List<DetallePago>();
            HistorialEstatusVM = new List<HistorialEstatusVM>();
            Estatus = ((enumEstatusMovimiento)Status).ToString();
        }
    }
}