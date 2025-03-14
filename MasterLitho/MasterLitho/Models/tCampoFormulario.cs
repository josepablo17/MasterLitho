using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterLitho.Models
{
    public class tCampoFormulario
    {
        public int idCampoFormulario { get; set; }
        public int idFormulario { get; set; }
        public string nombreCampo { get; set; }
        public string tipoCampo { get; set; }
        public bool esRequerido { get; set; }
        public virtual tFormulario formulario { get; set; }
    }
}