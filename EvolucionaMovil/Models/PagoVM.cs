using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EvolucionaMovil.Repositories;
using System.Linq;
using EvolucionaMovil.Models.Enums;
using EvolucionaMovil.Models.Interfaces;
using DataAnnotationsExtensions;
using System.ComponentModel;
namespace EvolucionaMovil.Models
{
    public class PagoVM : IEvolucionaMovil
    {

        [Required(ErrorMessage = "El nombre es requerido")]
        [DisplayName("Nombre cliente")]
        public string ClienteNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [DisplayName("Fecha de vencimiento")]
        public DateTime? FechaVencimiento { get; set; }
        public string FechaCreacionString { get { return FechaCreacion.ToString("dd/MMMM/yyy") + " " + FechaCreacion.ToShortTimeString(); } }
        public String FechaVencimientoString { get { return FechaVencimiento!=null?((DateTime)FechaVencimiento).ToString("dd/MMMM/yyy"):string.Empty; } }
        [Min(10)]
        [Required(ErrorMessage = "El importe es requerido")]
        public decimal? Importe { get; set; }
        [DisplayName("Importe")]
        public string ImporteString { get { return Importe.HasValue ? ((decimal)Importe).ToString("C") : null; } }

        public int PagoId { get; set; }
        [Min(1)]
        public int ServicioId { get; set; }
        public string ServicioNombre { get; set; }

        public int Status { get; set; }
        public string StatusString { get { return ((enumEstatusMovimiento)this.Status).ToString(); } }

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
        }

        public int PayCenterId { get; set; }
        public string PayCenterName { get; set; }
    }
}