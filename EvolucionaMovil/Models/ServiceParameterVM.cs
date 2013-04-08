﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EvolucionaMovil.Models
{
    public class ServiceParameterVM
    {
        public int pageSize { get; set; }
        public int pageNumber { get; set; }
        public string searchString { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaFin { get; set; }
        public Boolean onlyAplicados { get; set; }
    }
}