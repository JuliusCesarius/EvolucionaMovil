using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EvolucionaMovil.Models;

namespace cabinet.processPolicies.MVC.Models.Helpers
{
    public static class AutoMapperBootStrapperHelper
    {
        public static void Bootstrap()
        {
            Mapper.CreateMap<Prospecto, ProspectoVM>();
            Mapper.CreateMap<PayCenter, PayCenterVM>()
                    .ForMember(vm => vm.MaximoAFinanciar, opt => opt.MapFrom(m => Math.Round(m.MaximoAFinanciar, 2)));
            Mapper.CreateMap<PayCenterVM, PayCenter>()
                    .ForMember(m => m.Abonos, opt => opt.Ignore())
                    .ForMember(m => m.Cuentas, opt => opt.Ignore())
                    .ForMember(m => m.FechaCreacion, opt => opt.Ignore())
                    .ForMember(m => m.MaximoAFinanciar, opt => opt.MapFrom(vm => Convert.ToDecimal(vm.MaximoAFinanciar)));
            //.ForMember(m => m.Version, opt => opt.MapFrom(vm => string.IsNullOrEmpty(vm.Version)?"1":vm.Version))
            //.ForMember(m => m.Status, opt => opt.MapFrom(vm => Convert.ToInt16(vm.Status)))

            //******************Entity to ViewModel********************//
            Mapper.CreateMap<Abono, AbonoVM>();
            Mapper.CreateMap<Banco, BancoVM>();
            Mapper.CreateMap<CompraEvento, CompraEventoVM>();
            Mapper.CreateMap<CuentaBancaria, CuentaBancariaVM>();
            Mapper.CreateMap<Cuenta, CuentaVM>();
            Mapper.CreateMap<DetallePago, DetallePagoVM>();
            Mapper.CreateMap<DetalleServicio, DetalleServicioVM>();
            Mapper.CreateMap<Movimiento, EstadoCuentaVM>();
            Mapper.CreateMap<Pago, PagoVM>();
            Mapper.CreateMap<Paquete, PaqueteVM>();
            Mapper.CreateMap<PayCenter, PayCenterVM>();
            Mapper.CreateMap<Politica, PoliticaVM>();
            Mapper.CreateMap<Prospecto, ProspectoVM>();
            Mapper.CreateMap<Servicio, ServicioVM>();
            Mapper.CreateMap<Ticket, TicketVM>();
            Mapper.CreateMap<Usuario, UsuarioVM>();

            //******************ViewModel to Entity********************//
            Mapper.CreateMap<AbonoVM, Abono>();
            Mapper.CreateMap<BancoVM, Banco>()
                .ForMember(m => m.CuentasBancarias, opt => opt.Ignore());
            Mapper.CreateMap<CompraEventoVM, CompraEvento>();
            Mapper.CreateMap<CuentaBancariaVM, CuentaBancaria>();
            Mapper.CreateMap<CuentaVM, Cuenta>();
            Mapper.CreateMap<DetallePagoVM, DetallePago>();
            Mapper.CreateMap<DetalleServicioVM, DetalleServicio>();
            Mapper.CreateMap<EstadoCuentaVM, Movimiento>();
            Mapper.CreateMap<PagoVM, Pago>();
            Mapper.CreateMap<PaqueteVM, Paquete>();
            Mapper.CreateMap<PayCenterVM, PayCenter>();
            Mapper.CreateMap<PoliticaVM, Politica>();
            Mapper.CreateMap<ProspectoVM, Prospecto>();
            Mapper.CreateMap<ServicioVM, Servicio>()
                    .ForMember(m => m.DetalleServicios, opt => opt.Ignore());
            Mapper.CreateMap<TicketVM, Ticket>();
            Mapper.CreateMap<UsuarioVM, Usuario>();
        }
    }
}