using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tFormulario
    {
        public int idFormulario { get; set; }
        public string nombreFormulario { get; set; }
        public DateTime fechaEnvio { get; set; }
        public int idEmpleado { get; set; }
        public string nombreEmpleado { get; set; }
        public string observaciones { get; set; }
        public virtual List<tCampoFormulario> campos { get; set; }

    }
}